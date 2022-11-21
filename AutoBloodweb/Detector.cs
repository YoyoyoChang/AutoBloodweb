using System.Drawing;

namespace AutoBloodweb
{
    public class Detector
    {
        private const int STEP_WIDTH = 2;
        private const int STEP_HEIGHT = 4;
        private readonly Color _clickableRingColor = Color.FromArgb(255, 235, 225, 177);
        private readonly Color _prestigeColor = Color.FromArgb(255, 113, 7, 7);
        private readonly int _clickableRingTolerance = 30;
        private readonly int _prestigeTolerance = 8;

        // Below fields should adjust by different resolution, current are compatible with 2560 x 1440
        private readonly Rectangle _bloodWebRect = new Rectangle(360, 250, 1100, 1100);
        private readonly Rectangle _levelupRect = new Rectangle(770, 700, 260, 160);
        private readonly int _dxLeftToRight = 101;
        private readonly int _dxLeftToBottom = 45;
        private readonly int _dyLeftToBottom = 60;
        private Bitmap? _bloodwebImg;
        private Bitmap? _prestigeImg;

        public Point FindClickableNode()
        {
            _bloodwebImg = TakeScreenshot(_bloodWebRect);
            _prestigeImg = TakeScreenshot(_levelupRect);

            var clickableNode = Point.Empty;
            var stop = false;

            try
            {
                for (int i = 0; i + Math.Max(_dxLeftToRight, _dxLeftToBottom) < _bloodwebImg.Width && !stop; i++)
                {
                    for (int j = 0; j + _dyLeftToBottom < _bloodwebImg.Height && !stop; j++)
                    {
                        // Find clickable node by verify left & right & bottom groups of pixels 
                        if (clickableNode == Point.Empty &&
                            PixelGroupColorMatch(_bloodwebImg, i, j, STEP_WIDTH * 2, STEP_HEIGHT * 3, _clickableRingColor, _clickableRingTolerance) &&
                            PixelGroupColorMatch(_bloodwebImg, i + _dxLeftToRight, j, STEP_WIDTH * 2, STEP_HEIGHT * 3, _clickableRingColor, _clickableRingTolerance) &&
                            PixelGroupColorMatch(_bloodwebImg, i + _dxLeftToBottom, j + _dyLeftToBottom, STEP_WIDTH * 5, STEP_HEIGHT * 1, _clickableRingColor, _clickableRingTolerance))
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
                return clickableNode;
            }
            else
            {
                throw new NoClickableNodeException("Could not find clickable node.");
            }
        }

        public bool IsPrestigeReady(out Point prestigePoint)
        {
            for (int i = 0; i < _prestigeImg.Width; i++)
            {
                for (int j = 0; j < _prestigeImg.Height; j++)
                {
                    if (PixelGroupColorMatch(_prestigeImg, i, j, STEP_WIDTH * 3, STEP_HEIGHT * 4, _prestigeColor, _prestigeTolerance))
                    {
                        prestigePoint = new Point(i + _levelupRect.X, j + _levelupRect.Y);
                        return true;
                    }
                }
            }
            prestigePoint = Point.Empty;
            return false;
        }

        private static bool PixelGroupColorMatch(Bitmap img, int i, int j, int widthSize, int heightSize, Color targetColor, int tolerance)
        {
            if (j + heightSize > img.Height || i + widthSize > img.Width)
                return false;

            var pixels = new List<Color> { };

            for (int height = 0; height < heightSize; height += STEP_HEIGHT)
            {
                for (int width = 0; width < widthSize; width += STEP_WIDTH)
                {
                    var pixel = img.GetPixel(i + width, j + height);
                    pixels.Add(pixel);
                }
            }

            return pixels.All(p => AreColorsSimilar(p, targetColor, tolerance));
        }

        private static bool AreColorsSimilar(Color c1, Color c2, int tolerance)
        {
            return Math.Abs(c1.R - c2.R) < tolerance &&
                   Math.Abs(c1.G - c2.G) < tolerance &&
                   Math.Abs(c1.B - c2.B) < tolerance;
        }

        private static Bitmap TakeScreenshot(Rectangle rectangle)
        {
            var fullImg = ScreenCapture.CaptureActiveWindow();
            var cloneRect = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            return fullImg.Clone(cloneRect, fullImg.PixelFormat);
        }
    }
}
