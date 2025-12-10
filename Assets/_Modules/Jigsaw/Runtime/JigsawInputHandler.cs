using System;
using Cysharp.Threading.Tasks;
using Lean.Touch;
using UnityEngine;

public class JigsawInputHandler : MonoBehaviour
{
    [SerializeField] private Card selectedCard;

    private Card[,] cardMatrix;
    private Vector2[,] positions;
    private Vector3 fingerOffset;

    public Action OnCardSwapped;

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
            selectedCard.MoveTo(selectedCardCurrentMatrixPos, this.positions[currentRow, currentCol]);
            return;
        }

        MatrixPos targetMatrixPos = targetCard.CurrentMatrixPos;
        int targetRow = targetMatrixPos.Row;
        int targetCol = targetMatrixPos.Column;

        targetCard.MoveTo(selectedCardCurrentMatrixPos, this.positions[currentRow, currentCol]);
        await selectedCard.MoveTo(targetMatrixPos, this.positions[targetRow, targetCol]);

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
}