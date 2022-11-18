using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace AutoBloodweb
{
    public class BloodwebClicker
    {
        //This is a replacement for Cursor.Position in WinForms
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private Point _bloodWebLocation = new Point(360, 250);
        private Point _lastClickableNode;
        private int consecutiveErrorsCount;
        private Bitmap _bloodweb;


        public void Run()
        {
            ResetMousePosition();
            _bloodweb = TakeScreenshot();

            try
            {
                ResetMousePosition();
                var clickableNode = FindClickableNode();
                ClickNode(clickableNode.X, clickableNode.Y);

                if (ArePointsSimilar(_lastClickableNode, clickableNode, 10))
                {
                    throw new Exception("Could not validate previously identified node, out of bloodpoints?");
                }
                consecutiveErrorsCount = 0;
                _lastClickableNode = clickableNode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                switch (e.Message)
                {
                    case "Could not find clickable node.":
                        {
                            if (IsPrestigeReady(out var prestigePoint))
                            {
                                Console.WriteLine("Prestige ready, leveling up now.");
                                PrestigeLevelUp(prestigePoint);
                            }

                            ResetMousePosition();
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                            break;
                        }
                    case "Could not validate previously identified node, out of bloodpoints?":
                        {
                            consecutiveErrorsCount += 1;
                            Console.WriteLine($"{consecutiveErrorsCount} consecutive errors occur, about to abort reaching 5 times error.");
                            if (consecutiveErrorsCount >= 5)
                            {
                                Console.WriteLine($"{consecutiveErrorsCount} consecutive errors occur. Abortíng the operation.");
                                Environment.Exit(0);
                            }
                            //ResetMousePosition();
                            //mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            //Thread.Sleep(3000);
                            break;
                        }
                    default:
                        Console.WriteLine("Aborting.");
                        Environment.Exit(0);
                        break;
                }
            }

            Run();
        }

        private void ClickNode(int x, int y)
        {
            SetCursorPos(x + 35, y + 10);
            Thread.Sleep(10);
            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            Thread.Sleep(600);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
            //For the gift pack to continue
            //Thread.Sleep(100);
            //mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            //mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);
        }

        private void PrestigeLevelUp(Point point)
        {
            //var x = 900;
            //var y = 780;
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
                for (int i = 0; i < _bloodweb.Width && !stop; i++)
                {
                    for (int j = 0; j < _bloodweb.Height && !stop; j++)
                    {

                        //if (clickableNode == Point.Empty && PixelGroupColorMatch(i, j, 6, Color.FromArgb(255, 222, 214, 169), 15))
                        if (clickableNode == Point.Empty && PixelGroupColorMatch(i, j, 2, 8, Color.FromArgb(255, 228, 221, 174), 13))
                        {
                            clickableNode.X = i + _bloodWebLocation.X;
                            clickableNode.Y = j + _bloodWebLocation.Y;
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
                throw new Exception("Could not find clickable node.");
            }
        }

        private bool IsPrestigeReady(out Point prestigePoint)
        {
            for (int i = 0; i < _bloodweb.Width; i++)
            {
                for (int j = 0; j < _bloodweb.Height; j++)
                {
                    if (PixelGroupColorMatch(i, j, 1, 10, Color.FromArgb(255, 113, 7, 7), 8))
                    {
                        prestigePoint = new Point(i + _bloodWebLocation.X, j + _bloodWebLocation.Y);
                        return true;
                    }
                }
            }
            prestigePoint = Point.Empty;
            return false;
        }

        private Bitmap TakeScreenshot()
        {
            var fullImg = ScreenCapture.CaptureActiveWindow();
            //var cloneRect = new Rectangle(220, 150, 1000, 850);
            var cloneRect = new Rectangle(_bloodWebLocation.X, _bloodWebLocation.Y, 1040, 1050);
            //out of memory issue?
            return fullImg.Clone(cloneRect, fullImg.PixelFormat);
        }

        private bool AreColorsSimilar(Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) < tolerance &&
                   Math.Abs(c1.G - c2.G) < tolerance &&
                   Math.Abs(c1.B - c2.B) < tolerance;
        }

        private bool ArePointsSimilar(Point p1, Point p2, int tolerance)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tolerance;
        }

        private bool PixelGroupColorMatch(int i, int j, int widthSize, int heightSize, Color targetColor, int tolerance)
        {
            if (j + heightSize > _bloodweb.Height || i + widthSize > _bloodweb.Width)
                return false;

            var pixel0 = _bloodweb.GetPixel(i, j);
            var pixels = new List<Color> { pixel0 };
            for (int index = 1; index < heightSize; index++)
            {
                for (int col = 0; col < widthSize; col++)
                {
                    var pixel = _bloodweb.GetPixel(i + col, j + index);
                    pixels.Add(pixel);
                }
            }

            return pixels.All(p => AreColorsSimilar(p, targetColor, tolerance));
        }

        private void ResetMousePosition()
        {
            SetCursorPos(0, 0);
        }
    }
}
