// ========== SCRIPT 1: PuzzlePiece.cs (FIXED - Merge by Real Neighbors) ==========
using UnityEngine;
using System.Collections;

public class PuzzlePiece : MonoBehaviour
{
    [Header("Piece Info")]
    public int correctRow;
    public int correctCol;
    public int currentRow;
    public int currentCol;
    
    [Header("Settings")]
    public float snapDistance = 0.5f;
    public float moveSpeed = 10f;
    
    [Header("References")]
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Camera mainCamera;
    
    [Header("State")]
    private bool isDragging = false;
    private bool isPlaced = false;
    private Vector3 dragOffset;
    private Vector3 startDragPosition;
    
    [Header("Group")]
    public PuzzleGroup myGroup;
    
    private PuzzleManager puzzleManager;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        puzzleManager = FindObjectOfType<PuzzleManager>();
        
        CreateInitialGroup();
    }

    void CreateInitialGroup()
    {
        GameObject groupObj = new GameObject($"Group_{correctRow}_{correctCol}");
        myGroup = groupObj.AddComponent<PuzzleGroup>();
        myGroup.Initialize(puzzleManager);
        myGroup.AddPiece(this);
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        
        isDragging = true;
        startDragPosition = transform.position;
        
        Vector3 mousePos = GetMouseWorldPosition();
        dragOffset = transform.position - mousePos;
        
        myGroup.BringToFront();
    }

    void OnMouseDrag()
    {
        if (!isDragging || isPlaced) return;
        
        Vector3 mousePos = GetMouseWorldPosition();
        myGroup.MoveGroup(mousePos + dragOffset);
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        
        isDragging = false;
        
        Vector2Int nearestGrid = puzzleManager.GetNearestGridPosition(transform.position);
        
        if (puzzleManager.CanSwapToGrid(myGroup, nearestGrid))
        {
            puzzleManager.SwapGroup(myGroup, nearestGrid);
            
            // ✅ SAU KHI SWAP, KIỂM TRA MERGE NGAY
            CheckMergeWithNeighbors();
            
            // Kiểm tra xem có đúng vị trí cuối cùng không
            CheckIfCorrectPosition();
        }
        else
        {
            StartCoroutine(ReturnToPosition(startDragPosition));
        }
    }

    void CheckIfCorrectPosition()
    {
        // Kiểm tra xem tất cả pieces trong group có đúng vị trí không
        bool allCorrect = true;
        foreach (var piece in myGroup.pieces)
        {
            if (piece.currentRow != piece.correctRow || piece.currentCol != piece.correctCol)
            {
                allCorrect = false;
                break;
            }
        }
        
        if (allCorrect)
        {
            isPlaced = true;
            PlaySnapEffect();
            puzzleManager.OnGroupPlaced(myGroup);
        }
    }

    void CheckMergeWithNeighbors()
    {
        // ✅ KIỂM TRA 4 HƯỚNG XUNG QUANH VỊ TRÍ HIỆN TẠI
        TryMergeWithCurrentNeighbor(currentRow - 1, currentCol); // Trên
        TryMergeWithCurrentNeighbor(currentRow + 1, currentCol); // Dưới
        TryMergeWithCurrentNeighbor(currentRow, currentCol - 1); // Trái
        TryMergeWithCurrentNeighbor(currentRow, currentCol + 1); // Phải
    }

    void TryMergeWithCurrentNeighbor(int neighborCurrentRow, int neighborCurrentCol)
    {
        // Lấy piece đang ở vị trí neighbor trong grid hiện tại
        PuzzlePiece neighbor = puzzleManager.GetPieceByCurrentPosition(neighborCurrentRow, neighborCurrentCol);
        
        if (neighbor == null || neighbor.myGroup == myGroup) return;
        
        // ✅ KIỂM TRA XEM 2 PIECE CÓ PHẢI NEIGHBOR TRÊN ẢNH GỐC KHÔNG
        bool isRealNeighbor = IsRealNeighborOnOriginalImage(neighbor);
        
        if (isRealNeighbor)
        {
            Debug.Log($"✅ MERGE: Piece ({correctRow},{correctCol}) at current({currentRow},{currentCol}) " +
                     $"+ Piece ({neighbor.correctRow},{neighbor.correctCol}) at current({neighbor.currentRow},{neighbor.currentCol})");
            
            myGroup.MergeWith(neighbor.myGroup);
            PlayMergeEffect();
            
            // ✅ SAU KHI MERGE, KIỂM TRA LẠI NEIGHBORS CỦA GROUP MỚI
            // Vì group đã lớn hơn, có thể có thêm neighbors để merge
            CheckMergeWithNeighbors();
        }
    }

    bool IsRealNeighborOnOriginalImage(PuzzlePiece other)
    {
        // Kiểm tra xem 2 piece có phải neighbor trên ảnh gốc không
        int rowDiff = Mathf.Abs(correctRow - other.correctRow);
        int colDiff = Mathf.Abs(correctCol - other.correctCol);
        
        // Neighbor = cách nhau đúng 1 ô theo hàng HOẶC cột (không chéo)
        bool isNeighbor = (rowDiff == 1 && colDiff == 0) || (rowDiff == 0 && colDiff == 1);
        
        if (isNeighbor)
        {
            Debug.Log($"🔍 Real neighbors check: ({correctRow},{correctCol}) & ({other.correctRow},{other.correctCol}) = TRUE");
        }
        
        return isNeighbor;
    }

    IEnumerator ReturnToPosition(Vector3 targetPosition)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        
        while (elapsed < 0.3f)
        {
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = targetPosition;
    }

    void PlaySnapEffect()
    {
        StartCoroutine(ScalePulse());
    }

    void PlayMergeEffect()
    {
        Debug.Log($"🔗 Piece ({correctRow},{correctCol}) merged! Group now has {myGroup.pieces.Count} pieces");
    }

    IEnumerator ScalePulse()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        float time = 0;
        while (time < 0.15f)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, time / 0.15f);
            time += Time.deltaTime;
            yield return null;
        }
        
        time = 0;
        while (time < 0.15f)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, time / 0.15f);
            time += Time.deltaTime;
            yield return null;
        }
        
        transform.localScale = originalScale;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    public void SetSortingOrder(int order)
    {
        spriteRenderer.sortingOrder = order;
    }

    public void UpdateGridPosition(int row, int col)
    {
        currentRow = row;
        currentCol = col;
    }

    public bool IsPlaced() => isPlaced;
}