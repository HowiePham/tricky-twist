using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class RevealColorAtPos : VisualAction
{
    [SerializeField] private Transform targetPos;
    [SerializeField] private ColorRevealController colorRevealController;
    [SerializeField] private float customRevealRadius = 3f;

    [Header("Custom Reveal")] public bool useCustomRevealRadius = false;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.colorRevealController.RevealColorAt(this.targetPos.position,this.customRevealRadius);
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        // Vẽ outline trong Scene view để dễ nhìn
        Gizmos.color = Color.red;
        if (useCustomRevealRadius)
        {
            // Vẽ vòng tròn với custom radius
            Gizmos.DrawWireSphere(this.targetPos.position, customRevealRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(this.targetPos.position, 0.5f);
        }
    }
#endif
}