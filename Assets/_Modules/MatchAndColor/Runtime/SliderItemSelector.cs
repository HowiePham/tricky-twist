using Lean.Touch;
using Mimi.Interactions.Dragging;
using Mimi.Interactions.Dragging.Extensions;
using UnityEngine;
using UnityEngine.Events;

public class SliderItemSelector : MonoBehaviour
{
    [Header("Events")] public UnityEvent OnItemReadyToDrag;
    public UnityEvent OnItemEndDragging;

    [Header("Settings")] [SerializeField] private float dragThreshold = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LeanSelectableByFinger draggableItem;
    private ItemSlider parentSlider;
    private LeanFinger currentFinger;
    private Vector3 fingerDownPosition;
    private bool hasDraggedEnough = false;

    void Start()
    {
        this.parentSlider = GetComponentInParent<ItemSlider>();
    }

    void OnEnable()
    {
        LeanTouch.OnFingerDown += OnFingerDown;
        LeanTouch.OnFingerUpdate += OnFingerUpdate;
        LeanTouch.OnFingerUp += OnFingerUp;
    }

    void OnDisable()
    {
        LeanTouch.OnFingerDown -= OnFingerDown;
        LeanTouch.OnFingerUpdate -= OnFingerUpdate;
        LeanTouch.OnFingerUp -= OnFingerUp;
    }

    private void OnFingerDown(LeanFinger finger)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        if (this.spriteRenderer.bounds.Contains(worldPos))
        {
            this.currentFinger = finger;
            this.fingerDownPosition = worldPos;
            this.hasDraggedEnough = false;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (finger != this.currentFinger) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        // Kiểm tra xem đã kéo đủ xa chưa
        if (!this.hasDraggedEnough)
        {
            float dragDistance = Vector3.Distance(worldPos, this.fingerDownPosition);
            if (dragDistance > this.dragThreshold)
            {
                this.hasDraggedEnough = true;

                // Thông báo cho slider biết item đang được chọn để drag
                if (this.parentSlider != null)
                {
                    this.parentSlider.NotifyItemDragStart(finger);
                }

                // Trigger Unity Event - script khác sẽ xử lý drag
                this.OnItemReadyToDrag?.Invoke();
                this.draggableItem.SelectSelf(finger);
            }
        }
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (finger != this.currentFinger) return;

        if (this.hasDraggedEnough)
        {
            if (this.parentSlider != null)
            {
                this.parentSlider.NotifyItemDragEnd();
                this.OnItemEndDragging?.Invoke();
            }
        }

        this.currentFinger = null;
        this.hasDraggedEnough = false;
    }
}