using System;

[Serializable]
public struct MatrixPos
{
    public int Row;
    public int Column;

    public MatrixPos(int row, int column)
    {
        this.Row = row;
        this.Column = column;
    }

    public bool IsNeighborPos(MatrixPos pos, int maxRow, int maxCol, out Direction directionToTarget)
    {
        directionToTarget = Direction.None;

        if (pos.Row < 0 || pos.Row >= maxRow ||
            pos.Column < 0 || pos.Column >= maxCol)
        {
            return false;
        }

        if (this.Row == pos.Row && this.Column == pos.Column)
        {
            return false;
        }

        if (this.Row - 1 == pos.Row && this.Column == pos.Column)
        {
            directionToTarget = Direction.Down;
            return true;
        }

        if (this.Row + 1 == pos.Row && this.Column == pos.Column)
        {
            directionToTarget = Direction.Up;
            return true;
        }

        if (this.Row == pos.Row && this.Column + 1 == pos.Column)
        {
            directionToTarget = Direction.Right;
            return true;
        }

        if (this.Row == pos.Row && this.Column - 1 == pos.Column)
        {
            directionToTarget = Direction.Left;
            return true;
        }

        return false;
    }

    public bool Equals(MatrixPos matrixPos)
    {
        return this.Row == matrixPos.Row && this.Column == matrixPos.Column;
    }
}