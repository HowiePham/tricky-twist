using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;

public class ItemSlider : MonoBehaviour
{
    [Header("Slider Settings")] 
    [SerializeField] private float itemSpacing = 2f;
    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private bool enableSnapping = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Item Settings")] 
    [SerializeField] private Transform itemContainer;
    [SerializeField] private List<GameObject> spawnedItems = new List<GameObject>();
    
    private Vector3 dragStartPosition;
    private float currentOffset = 0f;
    private float targetOffset = 0f;
    [SerializeField] private bool isDragging = false;
    private int centerItemIndex = 0;
    
    // Tracking để phân biệt drag slider vs drag item
    private LeanFinger currentFinger;
    private bool isItemBeingDragged = false;

    void Start()
    {
        if (this.itemContainer == null)
        {
            this.itemContainer = this.transform;
        }

        UpdateItemPositions();
    }

    void Update()
    {
        if (!isDragging && this.enableSnapping)
        {
            this.currentOffset = Mathf.Lerp(this.currentOffset, this.targetOffset, Time.deltaTime * this.snapSpeed);
        }

        UpdateItemPositions();
        UpdateItemSelection();
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

        // Kiểm tra xem có chạm vào item nào không
        GameObject touchedItem = GetTouchedItem(worldPos);
        
        if (touchedItem != null)
        {
            // Touch vào item -> để script khác (DraggableItem) xử lý
            isItemBeingDragged = true;
            currentFinger = finger;
            return;
        }

        // Touch vào vùng trống của slider -> scroll slider
        if (IsInSliderArea(worldPos))
        {
            isDragging = true;
            isItemBeingDragged = false;
            dragStartPosition = worldPos;
            currentFinger = finger;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        // Nếu đang drag item, không scroll slider
        if (isItemBeingDragged) return;
        
        // Chỉ xử lý finger đang được track
        if (finger != currentFinger) return;
        
        if (!isDragging) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        float delta = worldPos.x - dragStartPosition.x;
        this.currentOffset += delta * slideSpeed;

        if (this.spawnedItems.Count > 0)
        {
            float maxOffset = (this.spawnedItems.Count - 1) * itemSpacing;
            this.currentOffset = Mathf.Clamp(this.currentOffset, -maxOffset, 0f);
        }

        dragStartPosition = worldPos;
    }

    private void OnFingerUp(LeanFinger finger)
    {
        // Chỉ xử lý finger đang được track
        if (finger != currentFinger) return;
        
        if (isDragging)
        {
            isDragging = false;

            if (this.enableSnapping)
            {
                SnapToNearestItem();
            }
        }
        
        isItemBeingDragged = false;
        currentFinger = null;
    }

    private GameObject GetTouchedItem(Vector3 worldPos)
    {
        // Kiểm tra xem có item nào được touch không
        foreach (GameObject item in spawnedItems)
        {
            if (item == null) continue;
            
            Collider2D collider = item.GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(worldPos))
            {
                return item;
            }
        }
        return null;
    }

    private void UpdateItemPositions()
    {
        for (int i = 0; i < this.spawnedItems.Count; i++)
        {
            if (this.spawnedItems[i] == null) continue;

            float targetX = i * itemSpacing + this.currentOffset;
            Vector3 pos = this.spawnedItems[i].transform.localPosition;
            pos.x = targetX;
            this.spawnedItems[i].transform.localPosition = pos;
        }
    }

    private void UpdateItemSelection()
    {
        if (this.spawnedItems.Count == 0) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < this.spawnedItems.Count; i++)
        {
            if (this.spawnedItems[i] == null) continue;

            float dist = Mathf.Abs(this.spawnedItems[i].transform.localPosition.x);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        centerItemIndex = closestIndex;
    }

    private void SnapToNearestItem()
    {
        if (this.spawnedItems.Count == 0) return;

        this.targetOffset = -centerItemIndex * itemSpacing;
        this.targetOffset = Mathf.Clamp(this.targetOffset, -(this.spawnedItems.Count - 1) * itemSpacing, 0f);
    }

    private bool IsInSliderArea(Vector3 worldPos)
    {
        if (spriteRenderer == null) return false;
        return spriteRenderer.bounds.Contains(worldPos);
    }

    // Public method để check xem slider có đang được drag không
    public bool IsSliderDragging()
    {
        return isDragging;
    }

    // Public method để item có thể báo là nó đang được drag
    public void NotifyItemDragStart(LeanFinger finger)
    {
        isItemBeingDragged = true;
        isDragging = false;
        currentFinger = finger;
    }

    public void NotifyItemDragEnd()
    {
        isItemBeingDragged = false;
        currentFinger = null;
    }

    void OnDrawGizmos()
    {
        if (spriteRenderer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spriteRenderer.bounds.center, spriteRenderer.bounds.size);
        }
    }
}