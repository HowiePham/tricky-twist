using Cysharp.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardVisual cardVisual;
    [SerializeField] private CardInteracting cardInteracting;
    [SerializeField] private CardOutline cardOutline;
    [SerializeField] private MatrixPos currentMatrixPos;
    [SerializeField] private MatrixPos correctMatrixPos;
    [SerializeField] private bool isCompleted;

    public MatrixPos CurrentMatrixPos => this.currentMatrixPos;
    public MatrixPos CorrectMatrixPos => this.correctMatrixPos;
    public bool IsCompleted => this.isCompleted;
    public CardOutline Outline => this.cardOutline;

    public void InitCard(MatrixPos correctMatrixPos, MatrixPos currentMatrixPos, Sprite cardSprite)
    {
        this.correctMatrixPos = correctMatrixPos;
        this.currentMatrixPos = currentMatrixPos;
        this.cardVisual.SetSpriteVisual(cardSprite);

        // Update outline positions sau khi set sprite
        if (this.cardOutline != null)
        {
            this.cardOutline.UpdateOutlinePositions();
        }
    }

    public void PrioritizeOrderLayer()
    {
        this.cardVisual.PrioritizeOrderLayer();
    }

    public void ResetOrderLayer()
    {
        this.cardVisual.ResetOrderLayer();
    }

    public async UniTask MoveTo(MatrixPos newMatrixPos, Vector2 newPos)
    {
        this.currentMatrixPos = newMatrixPos;
        PrioritizeOrderLayer();
        await this.cardInteracting.MoveTo(newPos);
        ResetOrderLayer();
        this.isCompleted = this.correctMatrixPos.Equals(newMatrixPos);
    }

    public Bounds GetBounds()
    {
        return this.cardVisual.GetBounds();
    }

    public void UpdateOutline(Direction direction, bool active)
    {
        if (this.cardOutline != null)
        {
            this.cardOutline.SetSideActive(direction, active);
        }
    }

    public void ShowAllOutlines()
    {
        if (this.cardOutline != null)
        {
            this.cardOutline.SetAllSidesActive(true);
        }
    }

    public void HideAllOutlines()
    {
        if (this.cardOutline != null)
        {
            this.cardOutline.SetAllSidesActive(false);
        }
    }
}