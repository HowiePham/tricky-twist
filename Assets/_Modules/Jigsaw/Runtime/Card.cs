using Cysharp.Threading.Tasks;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardVisual cardVisual;
    [SerializeField] private CardInteracting cardInteracting;
    [SerializeField] private MatrixPos currentMatrixPos;
    [SerializeField] private MatrixPos correctMatrixPos;
    [SerializeField] private bool isCompleted;

    public MatrixPos CurrentMatrixPos => this.currentMatrixPos;

    public MatrixPos CorrectMatrixPos => this.correctMatrixPos;

    public bool IsCompleted => this.isCompleted;

    public void InitCard(MatrixPos correctMatrixPos, MatrixPos currentMatrixPos, Sprite cardSprite)
    {
        this.correctMatrixPos = correctMatrixPos;
        this.currentMatrixPos = currentMatrixPos;
        this.cardVisual.SetSpriteVisual(cardSprite);
    }

    public void Select()
    {
        this.cardVisual.PrioritizeOrderLayer();
    }

    public void Deselect()
    {
        this.cardVisual.ResetOrderLayer();
    }

    public async UniTask MoveTo(MatrixPos newMatrixPos, Vector2 newPos)
    {
        this.currentMatrixPos = newMatrixPos;
        await this.cardInteracting.MoveTo(newPos);

        this.isCompleted = this.correctMatrixPos.Equals(newMatrixPos);
    }

    public Bounds GetBounds()
    {
        return this.cardVisual.GetBounds();
    }
}