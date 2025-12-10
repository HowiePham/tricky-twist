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

    public bool Equals(MatrixPos matrixPos)
    {
        return this.Row == matrixPos.Row && this.Column == matrixPos.Column;
    }
}