using UnityEngine;
using UnityEngine.Events;

public class JigsawGameflowHandler : MonoBehaviour
{
    private JigsawInputHandler jigsawInputHandler;
    private Card[,] cardMatrix;
    public UnityEvent OnJigsawGameWin;

    public void Init(JigsawInputHandler jigsawInputHandler, Card[,] cardMatrix)
    {
        this.cardMatrix = cardMatrix;
        this.jigsawInputHandler = jigsawInputHandler;

        this.jigsawInputHandler.OnCardSwapped.AddListener(CheckCardMatrix);
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
        OnJigsawGameWin?.Invoke();
    }

    private void OnDisable()
    {
        this.jigsawInputHandler.OnCardSwapped.RemoveListener(CheckCardMatrix);
    }
}