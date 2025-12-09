using UnityEngine;
using System.Collections.Generic;

public class PuzzleGroup : MonoBehaviour
{
    public List<PuzzlePiece> pieces = new List<PuzzlePiece>();
    private static int globalSortingOrder = 0;
    private PuzzleManager puzzleManager;

    public void Initialize(PuzzleManager manager)
    {
        puzzleManager = manager;
    }

    public void AddPiece(PuzzlePiece piece)
    {
        if (!pieces.Contains(piece))
        {
            pieces.Add(piece);
            piece.myGroup = this;
            piece.transform.SetParent(transform);
        }
    }

    public void MoveGroup(Vector3 newPosition)
    {
        if (pieces.Count == 0) return;

        Vector3 delta = newPosition - pieces[0].transform.position;

        foreach (var piece in pieces)
        {
            piece.transform.position += delta;
        }
    }

    public void BringToFront()
    {
        globalSortingOrder += 10;
        foreach (var piece in pieces)
        {
            piece.SetSortingOrder(globalSortingOrder);
        }
    }

    public void MergeWith(PuzzleGroup otherGroup)
    {
        if (otherGroup == this) return;

        List<PuzzlePiece> piecesToMove = new List<PuzzlePiece>(otherGroup.pieces);

        foreach (var piece in piecesToMove)
        {
            otherGroup.pieces.Remove(piece);
            AddPiece(piece);
        }

        if (puzzleManager != null)
        {
            puzzleManager.OnGroupsMerged(this, otherGroup);
        }

        Destroy(otherGroup.gameObject);
    }

    public List<Vector2Int> GetOccupiedGridPositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (var piece in pieces)
        {
            positions.Add(new Vector2Int(piece.currentRow, piece.currentCol));
        }

        return positions;
    }

    public Vector2Int GetTopLeftGridPosition()
    {
        if (pieces.Count == 0) return Vector2Int.zero;

        int minRow = int.MaxValue;
        int minCol = int.MaxValue;

        foreach (var piece in pieces)
        {
            if (piece.currentRow < minRow) minRow = piece.currentRow;
            if (piece.currentCol < minCol) minCol = piece.currentCol;
        }

        return new Vector2Int(minRow, minCol);
    }
}