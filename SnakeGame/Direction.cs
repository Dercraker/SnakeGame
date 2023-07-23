using System;
using System.Collections.Generic;

namespace SnakeGame;
public class Direction
{

    public static readonly Direction Up = new(-1, 0);
    public static readonly Direction Down = new(1, 0);
    public static readonly Direction Left = new(0, -1);
    public static readonly Direction Right = new(0, 1);

    public int RowOffset { get; set; }
    public int ColOffset { get; set; }

    private Direction(int rowOffset, int colOffset)
    {
        RowOffset = rowOffset;
        ColOffset = colOffset;
    }

    public Direction Opposite() => new(-RowOffset, -ColOffset);
    public override bool Equals(object obj) => obj is Direction direction && RowOffset == direction.RowOffset && ColOffset == direction.ColOffset;
    public override int GetHashCode() => HashCode.Combine(RowOffset, ColOffset);

    public static bool operator ==(Direction left, Direction right) => EqualityComparer<Direction>.Default.Equals(left, right);
    public static bool operator !=(Direction left, Direction right) => !(left == right);
}
