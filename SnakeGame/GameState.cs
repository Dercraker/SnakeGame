using System;
using System.Collections.Generic;

namespace SnakeGame;
public class GameState
{
    public int Rows { get; set; }
    public int Cols { get; set; }
    public EGridValue[,] Grid { get; set; }
    public Direction Dir { get; private set; }
    public int Score { get; set; }
    public bool GameOver { get; set; }

    private readonly LinkedList<Direction> dirChanges = new();
    private readonly LinkedList<Position> snakePositions = new();
    private readonly Random random = new();

    public GameState(int rows, int cols)
    {
        Rows = rows;
        Cols = cols;
        Grid = new EGridValue[Rows, Cols];

        Dir = Direction.Right;
        AddSnake();
        AddFood();
    }

    private void AddSnake()
    {
        int r = Rows / 2;
        for (int c = 0; c < 3; c++)
        {
            Grid[r, c] = EGridValue.Snake;
            snakePositions.AddFirst(new Position(r, c));
        }
    }

    private IEnumerable<Position> EmptyPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Grid[r, c] == EGridValue.Empty)
                    yield return new Position(r, c);
            }
        }
    }

    private void AddFood()
    {
        List<Position> empty = new(EmptyPositions());

        if (empty.Count == 0)
            return;

        Position pos = empty[random.Next(empty.Count)];
        Grid[pos.Row, pos.Column] = EGridValue.Food;
    }

    public Position HeadPosition() => snakePositions.First.Value;
    public Position TailPosition() => snakePositions.Last.Value;

    public IEnumerable<Position> SnakePosition() => snakePositions;

    private void AddHead(Position pos)
    {
        snakePositions.AddFirst(pos);
        Grid[pos.Row, pos.Column] = EGridValue.Snake;
    }

    private void RemoveTail()
    {
        Position tail = snakePositions.Last.Value;
        Grid[tail.Row, tail.Column] = EGridValue.Empty;
        snakePositions.RemoveLast();
    }

    private Direction GetLastDirection()
    {
        if (dirChanges.Count is 0)
            return Dir;

        return dirChanges.Last.Value;
    }

    private bool CanChangeDirection(Direction newDir)
    {
        if (dirChanges.Count == 2)
            return false;

        Direction lastDir = GetLastDirection();
        return newDir != lastDir && newDir != lastDir.Opposite();
    }

    public void ChangeDirection(Direction dir)
    {
        if (CanChangeDirection(dir))
            dirChanges.AddLast(dir);
    }

    private bool OutSideGrid(Position pos) => pos.Row < 0 || pos.Column < 0 || pos.Row >= Rows || pos.Column >= Cols;

    private EGridValue WillHit(Position newHeadPos)
    {
        if (OutSideGrid(newHeadPos))
            return EGridValue.Outside;

        if (newHeadPos == TailPosition())
            return EGridValue.Empty;

        return Grid[newHeadPos.Row, newHeadPos.Column];
    }

    public void Move()
    {
        if (dirChanges.Count > 0)
        {
            Dir = dirChanges.First.Value;
            dirChanges.RemoveFirst();
        }

        Position newHeadPosition = HeadPosition().Translate(Dir);
        EGridValue hit = WillHit(newHeadPosition);

        if (hit is EGridValue.Outside or EGridValue.Snake)
            GameOver = true;
        else if (hit == EGridValue.Empty)
        {
            RemoveTail();
            AddHead(newHeadPosition);
        }
        else if (hit == EGridValue.Food)
        {
            AddHead(newHeadPosition);
            Score++;
            AddFood();
        }
    }
}
