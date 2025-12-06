using System.Threading;
using Cysharp.Threading.Tasks;
using Mimi.VisualActions;
using UnityEngine;

public class RevealAllColor : VisualAction
{
    [SerializeField] private ColorRevealController colorRevealController;

    protected override async UniTask OnExecuting(CancellationToken cancellationToken)
    {
        this.colorRevealController.RevealAllColor();
    }
}