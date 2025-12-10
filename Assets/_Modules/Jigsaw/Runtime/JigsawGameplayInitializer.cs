using UnityEngine;

public class JigsawGameplayInitializer : MonoBehaviour
{
    [SerializeField] private JigsawGameflowHandler gameflowHandler;
    [SerializeField] private JigsawInputHandler inputHandler;
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

        var positions = new Vector2[this.totalRow, this.totalCol];

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

                MatrixPos emptyCardMatrixPos = GetEmptyCardMatrixPos(row, col);
                var correctMatrixPos = new MatrixPos(row, col);

                int currentCardRow = emptyCardMatrixPos.Row;
                int currentCardColumn = emptyCardMatrixPos.Column;
                float cardPosX = (currentCardColumn - 1) * (w / this.pixelsPerUnit) + this.columnGap * currentCardColumn;
                float cardPosY = (currentCardRow - 1) * (h / this.pixelsPerUnit) + this.rowGap * currentCardRow;
                cardGameObject.transform.position = new Vector3(
                    cardPosX,
                    cardPosY,
                    0);

                card.InitCard(correctMatrixPos, emptyCardMatrixPos, sprite);

                this.cardMatrix[currentCardRow, currentCardColumn] = card;
                positions[currentCardRow, currentCardColumn] = new Vector2(cardPosX, cardPosY);
            }
        }

        this.inputHandler.Init(this.cardMatrix, positions);
        this.gameflowHandler.Init(this.inputHandler, this.cardMatrix);
    }

    private MatrixPos GetEmptyCardMatrixPos(int currentRow, int currentCol)
    {
        int row = Random.Range(0, this.totalRow);
        int col = Random.Range(0, this.totalCol);
        Card emptyCard = this.cardMatrix[row, col];
        bool isSamePos = row == currentRow && col == currentCol;

        while (emptyCard != null || isSamePos)
        {
            row = Random.Range(0, this.totalRow);
            col = Random.Range(0, this.totalCol);
            emptyCard = this.cardMatrix[row, col];
            isSamePos = row == currentRow && col == currentCol;
        }

        return new MatrixPos(row, col);
    }
}