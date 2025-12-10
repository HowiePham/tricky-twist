using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CardInteracting : MonoBehaviour
{
    [SerializeField] private float movingDuration = 0.5f;
    private Tween movingTween;

    public async UniTask MoveTo(Vector2 position)
    {
        this.movingTween.Kill();
        this.movingTween = this.transform.DOMove(position, this.movingDuration);
        await this.movingTween.AsyncWaitForCompletion();
    }
}