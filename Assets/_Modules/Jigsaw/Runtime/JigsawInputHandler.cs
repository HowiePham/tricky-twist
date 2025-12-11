using System;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using Mimi.Audio;
using Mimi.Services.ScriptableObject.Audio;
using UnityEngine;
using UnityEngine.Events;

public class JigsawInputHandler : MonoBehaviour
{
    [SerializeField] private Card selectedCard;
    public UnityEvent OnCardSwapped;

    [Header("Sound")] [SerializeField] private BaseAudioServiceSO audioPlayer;
    [SerializeField, SoundKey] private string placeSoundKey;
    [SerializeField, SoundKey] private string pickSoundKey;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 5f)] private float pitch = 1f;

    private Card[,] cardMatrix;
    private Vector2[,] positions;
    private Vector3 fingerOffset;

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
    }

    public void Init(Card[,] cardMatrix, Vector2[,] positions)
    {
        this.cardMatrix = cardMatrix;
        this.positions = positions;

        foreach (Card card in this.cardMatrix)
        {
            CheckCardCanConnect(card);
        }
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (this.selectedCard == null)
        {
            return;
        }

        Vector3 fingerPos = finger.GetWorldPosition(10);
        this.selectedCard.transform.position = this.fingerOffset + fingerPos;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        Card selectedCard = CardNearFinger(finger);
        if (selectedCard == null)
        {
            return;
        }

        SelectCard(finger, selectedCard);
    }

    private void SelectCard(LeanFinger finger, Card selectedCard)
    {
        PlaySound(this.pickSoundKey);
        this.selectedCard = selectedCard;
        this.selectedCard.PrioritizeOrderLayer();
        Vector3 fingerPos = finger.GetWorldPosition(10);
        this.fingerOffset = this.selectedCard.transform.position - fingerPos;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        SwapCard(finger);
    }

    private void DeselectCard()
    {
        this.selectedCard.ResetOrderLayer();
        this.selectedCard = null;
    }

    private async UniTask SwapCard(LeanFinger finger)
    {
        if (this.selectedCard == null)
        {
            return;
        }

        Card targetCard = CardNearFinger(finger);
        Card selectedCard = this.selectedCard;

        DeselectCard();

        MatrixPos selectedCardCurrentMatrixPos = selectedCard.CurrentMatrixPos;
        int currentRow = selectedCardCurrentMatrixPos.Row;
        int currentCol = selectedCardCurrentMatrixPos.Column;

        if (targetCard == null)
        {
            await selectedCard.MoveTo(selectedCardCurrentMatrixPos, this.positions[currentRow, currentCol]);
            PlaySound(this.placeSoundKey);
            return;
        }

        MatrixPos targetMatrixPos = targetCard.CurrentMatrixPos;
        int targetRow = targetMatrixPos.Row;
        int targetCol = targetMatrixPos.Column;

        UniTask task1 = targetCard.MoveTo(selectedCardCurrentMatrixPos, this.positions[currentRow, currentCol]);
        UniTask task2 = selectedCard.MoveTo(targetMatrixPos, this.positions[targetRow, targetCol]);
        this.cardMatrix[currentRow, currentCol] = targetCard;
        this.cardMatrix[targetRow, targetCol] = selectedCard;
        await UniTask.WhenAll(task1, task2);
        PlaySound(this.placeSoundKey);

        CheckCardCanConnect(selectedCard);
        CheckCardCanConnect(targetCard);
        OnCardSwapped?.Invoke();
    }

    private Card CardNearFinger(LeanFinger finger)
    {
        Vector3 fingerPos = finger.GetWorldPosition(10);

        foreach (Card card in this.cardMatrix)
        {
            Bounds cardBounds = card.GetBounds();

            if (!cardBounds.Contains(fingerPos) || card == this.selectedCard)
            {
                continue;
            }

            return card;
        }

        return null;
    }


    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
    }

    private void CheckCardCanConnect(Card card)
    {
        MatrixPos currentPosA = card.CurrentMatrixPos;

        Debug.Log($"=== Checking Card {card.gameObject.name} at current position ({currentPosA.Row}, {currentPosA.Column})");

        int currentRow = currentPosA.Row;
        int currentCol = currentPosA.Column;

        CheckNeighborConnection(card, currentRow - 1, currentCol, Direction.Down);
        CheckNeighborConnection(card, currentRow + 1, currentCol, Direction.Up);
        CheckNeighborConnection(card, currentRow, currentCol - 1, Direction.Left);
        CheckNeighborConnection(card, currentRow, currentCol + 1, Direction.Right);
    }

    private void CheckNeighborConnection(Card currentCard, int neighborRow, int neighborCol, Direction dirToTarget)
    {
        int maxRow = this.cardMatrix.GetLength(0);
        int maxCol = this.cardMatrix.GetLength(1);

        if (neighborRow < 0 || neighborRow >= maxRow ||
            neighborCol < 0 || neighborCol >= maxCol)
        {
            Debug.Log($"  [{dirToTarget}] Out of bounds");
            currentCard.UpdateOutline(dirToTarget, false);
            return;
        }

        Card neighborCard = this.cardMatrix[neighborRow, neighborCol];
        if (neighborCard == null)
        {
            Debug.Log($"  [{dirToTarget}] No card");
            currentCard.UpdateOutline(dirToTarget, false);
            return;
        }

        MatrixPos currentCardCorrectPos = currentCard.CorrectMatrixPos;
        MatrixPos correctPosNeighbor = neighborCard.CorrectMatrixPos;

        bool canConnect = currentCardCorrectPos.IsNeighborPos(correctPosNeighbor, maxRow, maxCol, out Direction neighborDirection);
        correctPosNeighbor.IsNeighborPos(currentCardCorrectPos, maxRow, maxCol, out Direction neighborDirectionToTarget);

        if (canConnect && neighborDirection == dirToTarget)
        {
            currentCard.UpdateOutline(dirToTarget, false);
            neighborCard.UpdateOutline(neighborDirectionToTarget, false);
            Debug.Log($"  [{dirToTarget}] ✓ CAN CONNECT! Neighbor correct pos: ({correctPosNeighbor.Row}, {correctPosNeighbor.Column}) --- {neighborCard.gameObject.name} --- {neighborDirection}");
        }
        else
        {
            currentCard.UpdateOutline(dirToTarget, true);
            neighborCard.UpdateOutline(neighborDirectionToTarget, true);
            Debug.Log($"  [{dirToTarget}] ✗ Cannot connect. Neighbor correct pos: ({correctPosNeighbor.Row}, {correctPosNeighbor.Column}) --- {neighborCard.gameObject.name} --- {neighborDirection}");
        }
    }

    private void PlaySound(string soundKey)
    {
        this.audioPlayer.PlaySound(soundKey, this.volume, this.pitch);
    }
}