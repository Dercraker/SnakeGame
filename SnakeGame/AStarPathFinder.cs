using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame;
public class AStarPathFinder
{
    private readonly int rows, cols;
    private readonly EGridValue[,] grid;
    private readonly List<Position> snakePositions;
    private readonly Position target;

    public AStarPathFinder(int rows, int cols, EGridValue[,] grid, List<Position> snakePositions, Position target)
    {
        this.rows = rows;
        this.cols = cols;
        this.grid = grid;
        this.snakePositions = snakePositions;
        this.target = target;
    }

    private int Heuristic(Position from, Position to) => Math.Abs(from.Row - to.Row) + Math.Abs(from.Column - to.Column);

    private bool IsWalkable(Position position)
    {
        if (position.Row < 0 || position.Row >= rows || position.Column < 0 || position.Column >= cols)
            return false;

        if (grid[position.Row, position.Column] is EGridValue.Outside or EGridValue.Snake)
            return false;

        return true;
    }

    private List<Position> GetNeighbors(Position position)
    {
        List<Position> neighbors = new();

        foreach (Direction direction in new[] { Direction.Up, Direction.Right, Direction.Down, Direction.Left })
        {
            Position neighbor = position.Translate(direction);

            if (IsWalkable(neighbor))
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    private List<Position> ReconstructPath(Dictionary<Position, Position> cameFrom, Position current)
    {
        List<Position> path = new() { current };

        while (cameFrom.TryGetValue(current, out Position previous))
        {
            current = previous;
            path.Insert(0, current);
        }

        return path;
    }

    public List<Position> FindPath()
    {
        Position start = snakePositions.First();
        HashSet<Position> openSet = new() { start };
        Dictionary<Position, Position> cameFrom = new();
        Dictionary<Position, int> gScore = new() { { start, 0 } };

        while (openSet.Count > 0)
        {
            Position current = GetLowestFScore(openSet, gScore);

            if (current == target)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);

            foreach (Position neighbor in GetNeighbors(current))
            {
                int tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new();
    }

    private Position GetLowestFScore(HashSet<Position> openSet, Dictionary<Position, int> gScore)
    {
        int lowestFScore = int.MaxValue;
        Position lowestFNode = null;

        foreach (Position node in openSet)
        {
            if (gScore.TryGetValue(node, out int g))
            {
                int f = g + Heuristic(node, target);
                if (f < lowestFScore)
                {
                    lowestFScore = f;
                    lowestFNode = node;
                }
            }
        }

        return lowestFNode;
    }
}
