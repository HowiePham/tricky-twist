using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardVisual cardVisual;
    [SerializeField] private MatrixPos currentMatrixPos;
    [SerializeField] private MatrixPos correctMatrixPos;


    public void InitCard(MatrixPos correctMatrixPos, MatrixPos currentMatrixPos, Sprite cardSprite)
    {
        this.correctMatrixPos = correctMatrixPos;
        this.currentMatrixPos = currentMatrixPos;
        this.cardVisual.SetSpriteVisual(cardSprite);
    }

    public Bounds GetBounds()
    {
        return this.cardVisual.GetBounds();
    }
}