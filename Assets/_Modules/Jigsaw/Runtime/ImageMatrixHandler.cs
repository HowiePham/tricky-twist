using UnityEngine;

public class ImageMatrixHandler : MonoBehaviour
{
    [SerializeField] private Texture2D sourceTexture;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private float pixelsPerUnit = 100f;
    [SerializeField] private int totalRow;
    [SerializeField] private int totalCol;
    [SerializeField] private float rowGap;
    [SerializeField] private float columnGap;
    private Card[,] cardMatrix;

    private void Start()
    {
        int w = this.sourceTexture.width / this.totalCol;
        int h = this.sourceTexture.height / this.totalRow;

        this.cardMatrix = new Card[this.totalRow, this.totalCol];

        for (var row = 0; row < this.totalRow; row++)
        {
            for (var col = 0; col < this.totalCol; col++)
            {
                var rect = new Rect(col * w, row * h, w, h);
                var pivot = new Vector2(0.5f, 0.5f);
                var sprite = Sprite.Create(this.sourceTexture, rect, pivot, this.pixelsPerUnit);

                Card card = Instantiate(this.cardPrefab, this.transform);
                GameObject cardGameObject = card.gameObject;
                cardGameObject.name = $"Piece_{col}_{row}";

                MatrixPos emptyCardMatrixPos = GetEmptyCardMatrixPos();
                var correctMatrixPos = new MatrixPos(row, col);

                cardGameObject.transform.position = new Vector3(
                    (emptyCardMatrixPos.Column - 1) * (w / this.pixelsPerUnit) + this.columnGap * emptyCardMatrixPos.Column,
                    (emptyCardMatrixPos.Row - 1) * (h / this.pixelsPerUnit) + this.rowGap * emptyCardMatrixPos.Row,
                    0);

                card.InitCard(correctMatrixPos, emptyCardMatrixPos, sprite);

                this.cardMatrix[emptyCardMatrixPos.Row, emptyCardMatrixPos.Column] = card;
            }
        }
    }

    private MatrixPos GetEmptyCardMatrixPos()
    {
        int row = Random.Range(0, this.totalRow);
        int col = Random.Range(0, this.totalCol);
        Card emptyCard = this.cardMatrix[row, col];
        while (emptyCard != null)
        {
            row = Random.Range(0, this.totalRow);
            col = Random.Range(0, this.totalCol);
            emptyCard = this.cardMatrix[row, col];
        }

        return new MatrixPos(row, col);
    }
}