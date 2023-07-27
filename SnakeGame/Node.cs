namespace SnakeGame;
public class Node
{
    public Node(int row, int col)
    {
        Row = row;
        Col = col;
        G = 0;
        H = 0;
        Parent = null;
    }

    public int Row { get; set; }
    public int Col { get; set; }
    public int G { get; set; }
    public int H { get; set; }
    public int F => G + H;
    public Node Parent { get; set; }
}
