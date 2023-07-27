using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SnakeGame;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Dictionary<EGridValue, ImageSource> gridValToImage = new()
    {
        {EGridValue.Empty, Images.Empty },
        {EGridValue.Snake, Images.SnakeBody },
        {EGridValue.Food, Images.Apple },

    };

    private readonly Dictionary<Direction, int> dirToRotation = new()
    {
        { Direction.Up, 0 },
        { Direction.Right, 90 },
        { Direction.Down, 180 },
        { Direction.Left, 270 },
    };

    private readonly int rows = 20, cols = 20;
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool gameRunning;

    public MainWindow()
    {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new(rows, cols);
    }

    private async Task RunGame()
    {
        Draw();
        await ShowCountDown();
        Overlay.Visibility = Visibility.Hidden;
        await GameLoop();
        await ShowGameOver();
        gameState = new GameState(rows, cols);
    }

    private async void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Overlay.Visibility is Visibility.Visible)
            e.Handled = true;

        if (!gameRunning)
        {
            gameRunning = true;
            await RunGame();
            gameRunning = false;
        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (gameState.GameOver)
            return;

        switch (e.Key)
        {
            case Key.Left:
                gameState.ChangeDirection(Direction.Left); break;
            case Key.Right:
                gameState.ChangeDirection(Direction.Right); break;
            case Key.Up:
                gameState.ChangeDirection(Direction.Up); break;
            case Key.Down:
                gameState.ChangeDirection(Direction.Down); break;
        }
    }

    //Replaced for ai with A*
    //private async Task GameLoop()
    //{
    //    while (!gameState.GameOver)
    //    {
    //        await Task.Delay(100);
    //        gameState.Move();
    //        Draw();
    //    }
    //}
    private async Task GameLoop()
    {
        while (!gameState.GameOver)
        {
            await Task.Delay(100);

            Position foodPostion = GetFoodPosition();
            Direction aiDirection = gameState.GetNextMove(foodPostion);

            gameState.ChangeDirection(aiDirection);

            gameState.Move();
            Draw();
        }
    }

    private Image[,] SetupGrid()
    {
        Image[,] images = new Image[rows, cols];
        GameGrid.Rows = rows;
        GameGrid.Columns = cols;
        GameGrid.Width = GameGrid.Height * (cols / (double)rows);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Image image = new()
                {
                    Source = Images.Empty,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };

                images[r, c] = image;
                GameGrid.Children.Add(image);
            }
        }

        return images;
    }

    private void Draw()
    {
        DrawGrid();
        DrawSnakeHead();
        ScoreText.Text = $"SCORE: {gameState.Score}";
    }

    private void DrawGrid()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                EGridValue gridValue = gameState.Grid[r, c];
                gridImages[r, c].Source = gridValToImage[gridValue];
                gridImages[r, c].RenderTransform = Transform.Identity;
            }
        }
    }

    private void DrawSnakeHead()
    {
        Position headPos = gameState.HeadPosition();
        Image image = gridImages[headPos.Row, headPos.Column];
        image.Source = Images.SnakeHead;

        int rotation = dirToRotation[gameState.Dir];
        image.RenderTransform = new RotateTransform(rotation);
    }

    private Position GetFoodPosition()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (gameState.Grid[r, c] == EGridValue.Food)
                    return new(r, c);
            }
        }
        return new(-1, -1);
    }

    private async Task DrawDeadSnake()
    {
        List<Position> positions = new(gameState.SnakePosition());

        for (int i = 0; i < positions.Count; i++)
        {
            Position pos = positions[i];
            ImageSource source = (i is 0) ? Images.DeadSnakeHead : Images.DeadSnakeBody;
            gridImages[pos.Row, pos.Column].Source = source;
            await Task.Delay(50);
        }
    }

    private async Task ShowCountDown()
    {
        for (int i = 3; i >= 1; i--)
        {
            OverlayText.Text = i.ToString();
            await Task.Delay(500);
        }
    }

    private async Task ShowGameOver()
    {
        await DrawDeadSnake();
        await Task.Delay(500);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "PRESS ANY KEY TO RESTART";
    }
}
