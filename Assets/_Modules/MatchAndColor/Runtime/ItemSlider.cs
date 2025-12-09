using System.Collections.Generic;
using Lean.Touch;
using Mimi.Interactions.Dragging;
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

    [SerializeField] private List<GameObject> spawnedItems = new List<GameObject>();
    private Vector3 dragStartPosition;
    private float currentOffset = 0f;
    private float targetOffset = 0f;
    [SerializeField] private bool isDragging = false;
    private int centerItemIndex = 0;

    void Start()
    {
        if (itemContainer == null)
        {
            itemContainer = transform;
        }

        UpdateItemPositions();
    }

    void Update()
    {
        if (!isDragging && enableSnapping)
        {
            currentOffset = Mathf.Lerp(currentOffset, targetOffset, Time.deltaTime * snapSpeed);
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

        // Check if touch is on slider area
        if (IsInSliderArea(worldPos))
        {
            isDragging = true;
            dragStartPosition = worldPos;
        }
    }

    private void OnFingerUpdate(LeanFinger finger)
    {
        if (!isDragging) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(finger.ScreenPosition);
        worldPos.z = 0f;

        float delta = worldPos.x - dragStartPosition.x;
        currentOffset += delta * slideSpeed;

        if (spawnedItems.Count > 0)
        {
            float maxOffset = (spawnedItems.Count - 1) * itemSpacing;
            currentOffset = Mathf.Clamp(currentOffset, -maxOffset, 0f);
        }

        dragStartPosition = worldPos;
    }

    private void OnFingerUp(LeanFinger finger)
    {
        if (!isDragging) return;

        isDragging = false;

        if (enableSnapping)
        {
            SnapToNearestItem();
        }
    }

    private void UpdateItemPositions()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] == null) continue;

            float targetX = i * itemSpacing + currentOffset;
            Vector3 pos = spawnedItems[i].transform.localPosition;
            pos.x = targetX;
            spawnedItems[i].transform.localPosition = pos;

            // float distanceFromCenter = Mathf.Abs(targetX);
            // float scale = Mathf.Lerp(1f, 0.7f, distanceFromCenter / (itemSpacing * 2));
            // spawnedItems[i].transform.localScale = Vector3.one * scale;
        }
    }

    private void UpdateItemSelection()
    {
        if (spawnedItems.Count == 0) return;

        float closestDist = float.MaxValue;
        int closestIndex = 0;

        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] == null) continue;

            float dist = Mathf.Abs(spawnedItems[i].transform.localPosition.x);
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
        if (spawnedItems.Count == 0) return;

        targetOffset = -centerItemIndex * itemSpacing;
        targetOffset = Mathf.Clamp(targetOffset, -(spawnedItems.Count - 1) * itemSpacing, 0f);
    }

    private bool IsInSliderArea(Vector3 worldPos)
    {
        if (spriteRenderer == null) return false;
        return spriteRenderer.bounds.Contains(worldPos);
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