using Lean.Touch;
using UnityEngine;

public class CardInteracting : MonoBehaviour
{
    [SerializeField] private Card card;
    [SerializeField] private bool isSelected;
    private Vector3 fingerOffset;
    public bool IsSelected => this.isSelected;

    private void OnEnable()
    {
        LeanTouch.OnFingerDown += FingerDownHandler;
        LeanTouch.OnFingerUp += FingerUpHandler;
        LeanTouch.OnFingerUpdate += FingerUpdateHandler;
    }

    private void FingerUpdateHandler(LeanFinger finger)
    {
        if (!IsSelected)
        {
            return;
        }

        Vector3 fingerPos = finger.GetWorldPosition(10);
        this.card.transform.position = this.fingerOffset + fingerPos;
    }

    private void FingerDownHandler(LeanFinger finger)
    {
        if (!CanSelectCard(finger))
        {
            return;
        }

        this.isSelected = true;

        Vector3 fingerPos = finger.GetWorldPosition(10);
        this.fingerOffset = this.card.transform.position - fingerPos;
    }

    private void FingerUpHandler(LeanFinger finger)
    {
        this.isSelected = false;
    }

    private bool CanSelectCard(LeanFinger finger)
    {
        Vector3 fingerPos = finger.GetWorldPosition(10);

        return this.card.GetBounds().Contains(fingerPos);
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerDown -= FingerDownHandler;
        LeanTouch.OnFingerUp -= FingerUpHandler;
        LeanTouch.OnFingerUpdate -= FingerUpdateHandler;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(this.card.transform.position, this.card.GetBounds().size);
    }
#endif
}