using System.Collections.Generic;
using System.Linq;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

public class ItemSlider : MonoBehaviour
{
    [Header("Slider Settings")] [SerializeField]
    private float itemSpacing = 2f;

    [SerializeField] private float slideSpeed = 5f;
    [SerializeField] private float snapSpeed = 10f;
    [SerializeField] private bool enableSnapping = true;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Item Settings")] [SerializeField]
    private Transform itemContainer;

    [SerializeField] private List<SliderItemSelector> sliderItemSelectors;

    private Vector3 dragStartPosition;
    private float currentOffset = 0f;
    private float targetOffset = 0f;
    [SerializeField] private bool isDragging = false;
    private int centerItemIndex = 0;

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

        GameObject touchedItem = GetTouchedItem(worldPos);

        if (touchedItem != null)
        {
            isItemBeingDragged = true;
            currentFinger = finger;
            return;
        }

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
        if (isItemBeingDragged) return;

        if (finger != currentFinger) return;

        if (!isDragging) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        float delta = worldPos.x - dragStartPosition.x;
        this.currentOffset += delta * slideSpeed;

        if (this.sliderItemSelectors.Count > 0)
        {
            float maxOffset = (this.sliderItemSelectors.Count - 1) * itemSpacing;
            this.currentOffset = Mathf.Clamp(this.currentOffset, -maxOffset, 0f);
        }

        dragStartPosition = worldPos;
    }

    private void OnFingerUp(LeanFinger finger)
    {
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
        foreach (var item in this.sliderItemSelectors)
        {
            if (item == null) continue;

            var collider = item.GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(worldPos))
            {
                return item.gameObject;
            }
        }

        return null;
    }

    private void UpdateItemPositions()
    {
        for (int i = 0; i < this.sliderItemSelectors.Count; i++)
        {
            if (this.sliderItemSelectors[i] == null) continue;

            float targetX = i * itemSpacing + this.currentOffset;
            Vector3 pos = this.sliderItemSelectors[i].transform.localPosition;
            pos.x = targetX;
            this.sliderItemSelectors[i].transform.localPosition = pos;

            float distanceFromCenter = Mathf.Abs(targetX);
            float scale = Mathf.Lerp(1f, 0.7f, distanceFromCenter / (itemSpacing * 2));
            this.sliderItemSelectors[i].transform.localScale = Vector3.one * scale;
        }
    }

    private void UpdateItemSelection()
    {
        if (this.sliderItemSelectors.Count == 0) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < this.sliderItemSelectors.Count; i++)
        {
            if (this.sliderItemSelectors[i] == null) continue;

            float dist = Mathf.Abs(this.sliderItemSelectors[i].transform.localPosition.x);
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
        if (this.sliderItemSelectors.Count == 0) return;

        this.targetOffset = -centerItemIndex * itemSpacing;
        this.targetOffset = Mathf.Clamp(this.targetOffset, -(this.sliderItemSelectors.Count - 1) * itemSpacing, 0f);
    }

    private bool IsInSliderArea(Vector3 worldPos)
    {
        if (spriteRenderer == null) return false;
        return spriteRenderer.bounds.Contains(worldPos);
    }

    public bool IsSliderDragging()
    {
        return isDragging;
    }

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

    public void RemoveItemFromList(SliderItemSelector sliderItemSelector)
    {
        this.sliderItemSelectors.Remove(sliderItemSelector);
    }

#if UNITY_EDITOR
    [Button]
    private void GetAllItemSlider()
    {
        this.sliderItemSelectors = GetComponentsInChildren<SliderItemSelector>().ToList();
    }
#endif
}