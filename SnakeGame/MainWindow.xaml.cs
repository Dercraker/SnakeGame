using System.Collections.Generic;
using System.Media;
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
        {EGridValue.Obstacle, Images.Obstacle },

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
    private readonly SoundPlayer dieSoundPlayer = new(Assets.Resource.SnakeDie);
    private readonly SoundPlayer countDownSoundPlayer = new(Assets.Resource.CountDown);
    private readonly SoundPlayer startSoundPlayer = new(Assets.Resource.Start);

    public MainWindow()
    {
        InitializeComponent();
        gridImages = SetupGrid();
        gameState = new(rows, cols);
    }

    private void OpenSettingsBtn_Click(object sender, RoutedEventArgs e)
    {

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

    #region Game
    private async void StartGame_Click(object sender, RoutedEventArgs e)
    {
        if (Overlay.Visibility is Visibility.Visible)
            e.Handled = true;

        MenuView.Visibility = Visibility.Hidden;
        GameView.Visibility = Visibility.Visible;

        if (!gameRunning)
        {
            gameRunning = true;
            await RunGame();
            gameRunning = false;
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

    private async Task RunGame()
    {
        Draw();
        await ShowCountDown();
        Overlay.Visibility = Visibility.Hidden;
        await GameLoop();
        await ShowGameOver();
        gameState = new GameState(rows, cols);
    }

    private async Task GameLoop()
    {
        while (!gameState.GameOver)
        {
            await Task.Delay(100);
            gameState.Move();
            Draw();
        }
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

    private async Task ShowCountDown()
    {
        for (int i = 3; i >= 1; i--)
        {
            countDownSoundPlayer.Play();
            OverlayText.Text = i.ToString();
            await Task.Delay(500);
        }
        startSoundPlayer.Play();
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

    private async Task ShowGameOver()
    {
        dieSoundPlayer.Play();
        await DrawDeadSnake();
        await Task.Delay(500);
        Overlay.Visibility = Visibility.Visible;
        OverlayText.Text = "PRESS ANY KEY TO RESTART";
    }
    #endregion
}
