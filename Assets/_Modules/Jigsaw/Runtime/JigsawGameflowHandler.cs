using UnityEngine;

public class JigsawGameflowHandler : MonoBehaviour
{
    private JigsawInputHandler jigsawInputHandler;
    private Card[,] cardMatrix;

    public void Init(JigsawInputHandler jigsawInputHandler, Card[,] cardMatrix)
    {
        this.cardMatrix = cardMatrix;
        this.jigsawInputHandler = jigsawInputHandler;

        this.jigsawInputHandler.OnCardSwapped += CheckCardMatrix;
    }

    private void CheckCardMatrix()
    {
        foreach (Card card in this.cardMatrix)
        {
            if (card.IsCompleted)
            {
                continue;
            }

            return;
        }

        Debug.Log($"--- (JIGSAW) Winnnnnn");
    }

    private void OnDisable()
    {
        this.jigsawInputHandler.OnCardSwapped -= CheckCardMatrix;
    }
}