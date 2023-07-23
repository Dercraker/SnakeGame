using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnakeGame;
public static class Images
{
    private static ImageSource LoadImage(string path) => new BitmapImage(new Uri($"Assets/{path}", UriKind.Relative));

    public static ImageSource Empty => LoadImage("Empty.png");
    public static ImageSource SnakeBody => LoadImage("Body.png");
    public static ImageSource SnakeHead => LoadImage("Head.png");
    public static ImageSource Apple => LoadImage("Food.png");
    public static ImageSource DeadSnakeBody => LoadImage("DeadBody.png");
    public static ImageSource DeadSnakeHead => LoadImage("DeadHead.png");

}
