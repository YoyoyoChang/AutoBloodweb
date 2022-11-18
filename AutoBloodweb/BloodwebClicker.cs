using System.Runtime.InteropServices;
using System.Drawing;

namespace AutoBloodweb
{
    public class BloodwebClicker
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int RESOLUTION = 2;
        private Rectangle _bloodWebRect = new Rectangle(360, 250, 1040, 1050);
        private Rectangle _levelupRect = new Rectangle(770, 700, 260, 160);
        private Point _lastClickableNode;
        private int continuousErrorsCount;
        private Bitmap? _bloodwebImg;
        private Bitmap? _levelupImg;

        public void Run()
        {
            ResetMousePosition();
            _bloodwebImg = TakeScreenshot(_bloodWebRect);
            _levelupImg = TakeScreenshot(_levelupRect);

            try
            {
                ResetMousePosition();
                var clickableNode = FindClickableNode();
                ClickNode(clickableNode.X, clickableNode.Y);

                if (ArePointsSimilar(_lastClickableNode, clickableNode, 150))
                {
                    throw new ClickSimilarPointsException("Previously node can not be validated, could be out of bloodpoints.");
                }
                continuousErrorsCount = 0;
                _lastClickableNode = clickableNode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                switch (e)
                {
                    case NoClickableNodeException:
                        {
                            if (IsPrestigeReady(out var prestigePoint))
                            {
                                Console.WriteLine("Prestige is ready, leveling up now.");
                                PrestigeLevelUp(prestigePoint);
                            }

                            ResetMousePosition();
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                            break;
                        }
                    case ClickSimilarPointsException:
                        {
                            continuousErrorsCount += 1;
                            Console.WriteLine($"{continuousErrorsCount} continuous errors occur, about to abort reaching 3 times error.");
                            if (continuousErrorsCount >= 3)
                            {
                                Console.WriteLine($"{continuousErrorsCount} continuous errors occur. Abortíng the operation.");
                                Environment.Exit(0);
                            }
                            break;
                        }
                    default:
                        Console.WriteLine("Aborting the operation.");
                        Environment.Exit(0);
                        break;
                }
            }

            Run();
        }

        private static Bitmap TakeScreenshot(Rectangle rectangle)
        {
            var fullImg = ScreenCapture.CaptureActiveWindow();
            var cloneRect = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            //out of memory issue?
            return fullImg.Clone(cloneRect, fullImg.PixelFormat);
        }

        private static void ClickNode(int x, int y)
        {
            SetCursorPos(x + 35, y + 10);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(600);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private static void PrestigeLevelUp(Point point)
        {
            var x = point.X + 10;
            var y = point.Y + 10;

            SetCursorPos(x, y);
            Thread.Sleep(500);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(2000);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            Thread.Sleep(5000);
        }

        private Point FindClickableNode()
        {
            var clickableNode = Point.Empty;

            var stop = false;
            try
            {
                for (int i = 0; i < _bloodwebImg.Width && !stop; i++)
                {
                    for (int j = 0; j < _bloodwebImg.Height && !stop; j++)
                    {
                        if (clickableNode == Point.Empty && PixelGroupColorMatch(_bloodwebImg, i, j, 3, 13, Color.FromArgb(255, 235, 225, 177), 20))
                        {
                            clickableNode.X = i + _bloodWebRect.X;
                            clickableNode.Y = j + _bloodWebRect.Y;
                            stop = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (clickableNode != Point.Empty)
            {
                Console.WriteLine($"Found clickable node at {clickableNode.X}, {clickableNode.Y}");
                SetCursorPos(clickableNode.X, clickableNode.Y);
                return clickableNode;
            }
            else
            {
                throw new NoClickableNodeException("Could not find clickable node.");
            }
        }

        private bool IsPrestigeReady(out Point prestigePoint)
        {
            for (int i = 0; i < _levelupImg.Width; i++)
            {
                for (int j = 0; j < _levelupImg.Height; j++)
                {
                    if (PixelGroupColorMatch(_levelupImg, i, j, 5, 11, Color.FromArgb(255, 113, 7, 7), 8))
                    {
                        prestigePoint = new Point(i + _levelupRect.X, j + _levelupRect.Y);
                        return true;
                    }
                }
            }
            prestigePoint = Point.Empty;
            return false;
        }


        private static bool AreColorsSimilar(Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) < tolerance &&
                   Math.Abs(c1.G - c2.G) < tolerance &&
                   Math.Abs(c1.B - c2.B) < tolerance;
        }

        private static bool ArePointsSimilar(Point p1, Point p2, int tolerance)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tolerance;
        }

        private static bool PixelGroupColorMatch(Bitmap img, int i, int j, int widthSize, int heightSize, Color targetColor, int tolerance)
        {
            if (j + heightSize > img.Height || i + widthSize > img.Width)
                return false;

            var pixel0 = img.GetPixel(i, j);
            var pixels = new List<Color> { pixel0 };
            for (int height = 1; height < heightSize; height += RESOLUTION)
            {
                for (int width = 0; width < widthSize; width += RESOLUTION)
                {
                    var pixel = img.GetPixel(i + width, j + height);
                    pixels.Add(pixel);
                }
            }

            return pixels.All(p => AreColorsSimilar(p, targetColor, tolerance));
        }

        private static void ResetMousePosition()
        {
            SetCursorPos(0, 0);
        }
    }
}
