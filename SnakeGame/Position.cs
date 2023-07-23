using System;
using System.Collections.Generic;

namespace SnakeGame;
public class Position
{
    public int Row { get; set; }
    public int Column { get; set; }

    public Position(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public Position Translate(Direction dir) => new(Row + dir.RowOffset, Column + dir.ColOffset);
    public override bool Equals(object obj) => obj is Position position && Row == position.Row && Column == position.Column;
    public override int GetHashCode() => HashCode.Combine(Row, Column);

    public static bool operator ==(Position left, Position right) => EqualityComparer<Position>.Default.Equals(left, right);
    public static bool operator !=(Position left, Position right) => !(left == right);
}
