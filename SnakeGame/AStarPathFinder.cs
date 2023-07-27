using System;
using System.Collections.Generic;

namespace SnakeGame;
public class AStarPathFinder
{
    private readonly int rows, cols;
    private readonly EGridValue[,] grid;
    private readonly List<Position> snakePositions;

    public AStarPathFinder(int rows, int cols, EGridValue[,] grid, List<Position> snakePositions)
    {
        this.rows = rows;
        this.cols = cols;
        this.grid = grid;
        this.snakePositions = snakePositions;
    }

    private int Heurisitic(Node current, Position target) => Math.Abs(current.Row - target.Row) + Math.Abs(current.Col - target.Column);

    private bool IsValid(int row, int col)
    {
        //check out of range of array
        if (row < 0 || col < 0 || row >= rows || col >= cols)
            return false;

        //Check if given position are not outisde grid Obstacle or snake body
        if (grid[row, col] is EGridValue.Outside or EGridValue.Obstacle or EGridValue.Snake)
            return false;

        return true;
    }

    private List<Node> GetNeighbors(Node current)
    {
        List<Node> neighbors = new();

        int[] rowOffsets = { -1, 0, 1, 0 };
        int[] colOffsets = { 0, 1, 0, -1 };

        for (int i = 0; i < 4; i++)
        {
            int newRow = current.Row + rowOffsets[i];
            int newCol = current.Col + colOffsets[i];

            if (IsValid(newRow, newCol))
                neighbors.Add(new(newRow, newCol));
        }

        return neighbors;
    }

    private List<Position> ReconstructPath(Node node)
    {
        List<Position> path = new();
        Node current = node;

        while (current != null)
        {
            path.Insert(0, new(current.Row, current.Col));
            current = current.Parent;
        }

        return path;
    }

    public List<Position> FindPath(Position start, Position end)
    {
        Node startNode = new(start.Row, start.Column);
        Node targetNode = new(end.Row, end.Column);

        if (!IsValid(end.Row, end.Column))
            return new();

        HashSet<Node> openSet = new() { startNode };
        HashSet<Node> closedSet = new();

        Dictionary<Node, int> gScores = new()
        {
            [startNode] = 0
        };

        while (openSet.Count > 0)
        {
            Node current = null;
            int lowestFScore = int.MaxValue;

            foreach (Node node in openSet)
            {
                int fScore = node.F;
                if (fScore < lowestFScore)
                {
                    current = node;
                    lowestFScore = fScore;
                }
            }

            if (current == targetNode)
                ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Node neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int tentativeGScore = gScores[current] + 1;

                if (!openSet.Contains(neighbor) || tentativeGScore < gScores[neighbor])
                {
                    neighbor.Parent = current;
                    gScores[neighbor] = tentativeGScore;
                    neighbor.H = Heurisitic(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return new();
    }
}
