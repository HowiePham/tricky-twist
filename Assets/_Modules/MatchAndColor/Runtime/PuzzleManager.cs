// ========== SCRIPT 3: PuzzleManager.cs (FIXED) ==========
using UnityEngine;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public Sprite puzzleImage;
    public int rows = 4;
    public int cols = 4;
    public float cellSpacing = 0.05f;
    
    [Header("Prefab")]
    public GameObject piecePrefab;
    
    [Header("Grid Visual (Optional)")]
    public bool showGridGizmos = true;
    
    // Grid system
    private PuzzlePiece[,] gridPieces;
    private PuzzlePiece[,] correctPieces;
    private Vector3[,] gridWorldPositions;
    
    private float cellWidth;
    private float cellHeight;
    
    private int placedGroupsCount = 0;
    private List<PuzzleGroup> allGroups = new List<PuzzleGroup>();

    void Start()
    {
        InitializeGrid();
        GeneratePuzzle();
    }

    void InitializeGrid()
    {
        if (puzzleImage == null)
        {
            Debug.LogError("Puzzle Image is not assigned!");
            return;
        }
        
        gridPieces = new PuzzlePiece[rows, cols];
        correctPieces = new PuzzlePiece[rows, cols];
        gridWorldPositions = new Vector3[rows, cols];
        
        // ✅ TÍNH KÍCH THƯỚC CHÍNH XÁC
        Texture2D texture = puzzleImage.texture;
        float pixelsPerUnit = puzzleImage.pixelsPerUnit;
        
        // Kích thước piece THEO PIXELS
        float pieceWidthPixels = texture.width / (float)cols;
        float pieceHeightPixels = texture.height / (float)rows;
        
        // Chuyển sang WORLD UNITS (Unity units)
        cellWidth = pieceWidthPixels / pixelsPerUnit;
        cellHeight = pieceHeightPixels / pixelsPerUnit;
        
        Debug.Log($"📏 Texture: {texture.width}x{texture.height}px");
        Debug.Log($"📐 PixelsPerUnit: {pixelsPerUnit}");
        Debug.Log($"🔲 Piece Size: {pieceWidthPixels}x{pieceHeightPixels}px = {cellWidth}x{cellHeight} units");
        
        // Tính toán vị trí grid
        float totalWidth = cols * cellWidth + (cols - 1) * cellSpacing;
        float totalHeight = rows * cellHeight + (rows - 1) * cellSpacing;
        Vector3 startPos = new Vector3(-totalWidth / 2, -totalHeight / 2, 0);
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = startPos.x + c * (cellWidth + cellSpacing) + cellWidth / 2;
                float y = startPos.y + r * (cellHeight + cellSpacing) + cellHeight / 2;
                gridWorldPositions[r, c] = new Vector3(x, y, 0);
            }
        }
    }

    void GeneratePuzzle()
    {
        if (puzzleImage == null)
        {
            Debug.LogError("Puzzle Image is not assigned!");
            return;
        }
        
        Texture2D texture = puzzleImage.texture;
        float pixelsPerUnit = puzzleImage.pixelsPerUnit;
        
        int pieceWidthPixels = texture.width / cols;
        int pieceHeightPixels = texture.height / rows;
        
        // Tạo danh sách vị trí để shuffle
        List<Vector2Int> availablePositions = new List<Vector2Int>();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                availablePositions.Add(new Vector2Int(r, c));
            }
        }
        
        // Shuffle positions
        for (int i = availablePositions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = availablePositions[i];
            availablePositions[i] = availablePositions[j];
            availablePositions[j] = temp;
        }
        
        int posIndex = 0;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // ✅ CẮT SPRITE CHÍNH XÁC
                Rect spriteRect = new Rect(
                    c * pieceWidthPixels,
                    (rows - 1 - r) * pieceHeightPixels,  // Unity sprite từ dưới lên
                    pieceWidthPixels,
                    pieceHeightPixels
                );
                
                // ✅ TẠO SPRITE VỚI ĐÚNG PIXELSPERUNIT
                Sprite pieceSprite = Sprite.Create(
                    texture,
                    spriteRect,
                    new Vector2(0.5f, 0.5f),  // Pivot ở giữa
                    pixelsPerUnit  // QUAN TRỌNG: Dùng cùng pixelsPerUnit với ảnh gốc
                );
                
                // Tạo GameObject
                GameObject pieceObj = Instantiate(piecePrefab, transform);
                pieceObj.name = $"Piece_{r}_{c}";
                
                PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
                piece.correctRow = r;
                piece.correctCol = c;
                
                // Gán vị trí ngẫu nhiên trong grid
                Vector2Int shuffledPos = availablePositions[posIndex];
                piece.currentRow = shuffledPos.x;
                piece.currentCol = shuffledPos.y;
                posIndex++;
                
                // Đặt piece vào vị trí grid
                pieceObj.transform.position = gridWorldPositions[piece.currentRow, piece.currentCol];
                
                // Gán sprite
                SpriteRenderer sr = pieceObj.GetComponent<SpriteRenderer>();
                sr.sprite = pieceSprite;
                
                // ✅ KHÔNG SCALE - Giữ nguyên 1:1
                pieceObj.transform.localScale = Vector3.one;
                
                // ✅ COLLIDER SIZE CHÍNH XÁC
                BoxCollider2D collider = pieceObj.GetComponent<BoxCollider2D>();
                if (collider == null)
                {
                    collider = pieceObj.AddComponent<BoxCollider2D>();
                }
                collider.size = new Vector2(cellWidth, cellHeight);
                
                // Lưu vào grid
                gridPieces[piece.currentRow, piece.currentCol] = piece;
                correctPieces[r, c] = piece;
                
                // Thêm group vào list
                if (!allGroups.Contains(piece.myGroup))
                {
                    allGroups.Add(piece.myGroup);
                }
            }
        }
        
        Debug.Log($"✅ Generated {rows}x{cols} = {rows * cols} pieces");
    }

    public Vector2Int GetNearestGridPosition(Vector3 worldPosition)
    {
        float minDistance = float.MaxValue;
        Vector2Int nearestGrid = Vector2Int.zero;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float distance = Vector3.Distance(worldPosition, gridWorldPositions[r, c]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestGrid = new Vector2Int(r, c);
                }
            }
        }
        
        return nearestGrid;
    }

    public bool CanSwapToGrid(PuzzleGroup group, Vector2Int targetGrid)
    {
        List<Vector2Int> occupiedPositions = group.GetOccupiedGridPositions();
        Vector2Int topLeft = group.GetTopLeftGridPosition();
        Vector2Int delta = targetGrid - topLeft;
        
        foreach (var pos in occupiedPositions)
        {
            Vector2Int newPos = pos + delta;
            
            if (newPos.x < 0 || newPos.x >= rows || newPos.y < 0 || newPos.y >= cols)
            {
                return false;
            }
        }
        
        return true;
    }

    public void SwapGroup(PuzzleGroup group, Vector2Int targetGrid)
    {
        List<Vector2Int> oldPositions = group.GetOccupiedGridPositions();
        Vector2Int topLeft = group.GetTopLeftGridPosition();
        Vector2Int delta = targetGrid - topLeft;
        
        List<PuzzlePiece> affectedPieces = new List<PuzzlePiece>();
        List<Vector2Int> newPositions = new List<Vector2Int>();
        
        foreach (var pos in oldPositions)
        {
            Vector2Int newPos = pos + delta;
            newPositions.Add(newPos);
            
            PuzzlePiece targetPiece = gridPieces[newPos.x, newPos.y];
            if (targetPiece != null && !group.pieces.Contains(targetPiece))
            {
                affectedPieces.Add(targetPiece);
            }
        }
        
        foreach (var pos in oldPositions)
        {
            gridPieces[pos.x, pos.y] = null;
        }
        
        int affectedIndex = 0;
        foreach (var oldPos in oldPositions)
        {
            if (affectedIndex < affectedPieces.Count)
            {
                PuzzlePiece affectedPiece = affectedPieces[affectedIndex];
                affectedPiece.UpdateGridPosition(oldPos.x, oldPos.y);
                affectedPiece.transform.position = gridWorldPositions[oldPos.x, oldPos.y];
                gridPieces[oldPos.x, oldPos.y] = affectedPiece;
                affectedIndex++;
            }
        }
        
        for (int i = 0; i < group.pieces.Count; i++)
        {
            PuzzlePiece piece = group.pieces[i];
            Vector2Int newPos = newPositions[i];
            
            piece.UpdateGridPosition(newPos.x, newPos.y);
            piece.transform.position = gridWorldPositions[newPos.x, newPos.y];
            gridPieces[newPos.x, newPos.y] = piece;
        }
    }

    public PuzzlePiece GetPieceByCorrectPosition(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;
        
        return correctPieces[row, col];
    }

    // ✅ THÊM METHOD LẤY PIECE THEO VỊ TRÍ HIỆN TẠI
    public PuzzlePiece GetPieceByCurrentPosition(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;
        
        return gridPieces[row, col];
    }

    public void OnGroupPlaced(PuzzleGroup group)
    {
        placedGroupsCount++;
        Debug.Log($"Group placed correctly! Total: {placedGroupsCount}");
        
        CheckPuzzleCompleted();
    }

    public void OnGroupsMerged(PuzzleGroup survivingGroup, PuzzleGroup mergedGroup)
    {
        if (allGroups.Contains(mergedGroup))
        {
            allGroups.Remove(mergedGroup);
        }
    }

    void CheckPuzzleCompleted()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                PuzzlePiece piece = gridPieces[r, c];
                if (piece == null || piece.correctRow != r || piece.correctCol != c)
                {
                    return;
                }
            }
        }
        
        OnPuzzleCompleted();
    }

    void OnPuzzleCompleted()
    {
        Debug.Log("🎉 PUZZLE COMPLETED! 🎉");
    }

    void OnDrawGizmos()
    {
        if (!showGridGizmos || gridWorldPositions == null) return;
        
        Gizmos.color = Color.yellow;
        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector3 pos = gridWorldPositions[r, c];
                Gizmos.DrawWireCube(pos, new Vector3(cellWidth, cellHeight, 0));
            }
        }
    }
}