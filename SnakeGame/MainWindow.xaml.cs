using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    private readonly Stopwatch _stopwatch = new();
    private readonly List<TimeSpan> _time = new();
    private readonly int gridSize = 50;
    private readonly int playDelay_ms = 1;

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

    private readonly int rows, cols;
    private readonly Image[,] gridImages;
    private GameState gameState;
    private bool gameRunning;

    public MainWindow()
    {
        rows = gridSize;
        cols = gridSize;

        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new(rows, cols);
    }

    private void ShowDebugTime()
    {
        TimeSpan max = _time.Max();
        Debug.WriteLine($"Max time : {max.Hours} hours, {max.Minutes} minutes, {max.Seconds} seconds, {max.Milliseconds} ms, {max.Nanoseconds} ns");
        TimeSpan min = _time.Min();
        Debug.WriteLine($"Min time : {min.Hours} hours, {min.Minutes} minutes, {min.Seconds} seconds, {min.Milliseconds} ms, {min.Nanoseconds} ns");
        TimeSpan avg = TimeSpan.FromTicks((long)_time.Average(ts => ts.Ticks));
        Debug.WriteLine($"AVG time : {avg.Hours} hours, {avg.Minutes} minutes, {avg.Seconds} seconds, {avg.Milliseconds} ms, {avg.Nanoseconds} ns");
    }

    private async Task RunGame()
    {
        Draw();
        await ShowCountDown();
        Overlay.Visibility = Visibility.Hidden;
        await GameLoop();
        await ShowGameOver();
        //gameState = new GameState(rows, cols);
        //ShowDebugTime();
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

    private async Task GameLoop()
    {
        while (!gameState.GameOver)
        {
            await Task.Delay(playDelay_ms);
            _stopwatch.Start();
            if (gameState.Grid.Cast<EGridValue>().Contains(EGridValue.Food))
            {
                List<Position> pathToFood = gameState.FindPathToFood();

                if (pathToFood.Count > 1)
                {
                    Position nextMove = pathToFood[1];
                    int rowDiff = nextMove.Row - gameState.HeadPosition().Row;
                    int colDiff = nextMove.Column - gameState.HeadPosition().Column;

                    if (rowDiff != 0)
                    {
                        if (rowDiff > 0)
                            gameState.ChangeDirection(Direction.Down);
                        else
                            gameState.ChangeDirection(Direction.Up);
                    }
                    else
                    {
                        if (colDiff > 0)
                            gameState.ChangeDirection(Direction.Right);
                        else
                            gameState.ChangeDirection(Direction.Left);
                    }
                }
            }

            gameState.Move();

            _stopwatch.Stop();
            _time.Add(_stopwatch.Elapsed);
            _stopwatch.Reset();

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
            await Task.Delay(50);
        }
    }

    private async Task ShowGameOver()
    {
        await DrawDeadSnake();
        await Task.Delay(500);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "PRESS ANY KEY TO RESTART";

        gameState = new GameState(rows, cols);
        ShowDebugTime();
        await Task.Delay(2000);
        await RunGame();
    }
}
