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
}