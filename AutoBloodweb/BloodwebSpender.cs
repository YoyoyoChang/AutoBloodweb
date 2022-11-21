using System.Runtime.InteropServices;
using System.Drawing;
using AutoBloodweb.Exceptions;

namespace AutoBloodweb
{
    public class BloodwebSpender
    {
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private readonly int ERROR_WARNING_COUNT = 5;
        private readonly int ERROR_TOLERANCE_COUNT = 7;
        private readonly Detector _detector;
        private int _continuousSimilarPointsCount;
        private int _continuousNoClickableNodeCount;
        private Point _lastClickableNode;

        public BloodwebSpender(Detector detector)
        {
            _detector = detector;
        }

        public void Run()
        {
            ResetMousePosition();

            try
            {
                ResetMousePosition();
                var clickableNode = _detector.FindClickableNode();
                SetCursorPos(clickableNode.X, clickableNode.Y);
                ClickNode(clickableNode.X, clickableNode.Y);

                if (ArePointsSimilar(_lastClickableNode, clickableNode, 200))
                {
                    throw new ClickSimilarPointsException("Previously node is invalid.");
                }

                _continuousSimilarPointsCount = 0;
                _continuousNoClickableNodeCount = 0;
                _lastClickableNode = clickableNode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                switch (e)
                {
                    case NoClickableNodeException:
                        {
                            if (_detector.IsPrestigeReady(out var prestigePoint))
                            {
                                Console.WriteLine("Prestige is ready, leveling up now.");
                                PrestigeLevelUp(prestigePoint);
                            }

                            ResetMousePosition();
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

                            _continuousNoClickableNodeCount += 1;
                            ErrorCheck(_continuousNoClickableNodeCount);

                            break;
                        }
                    case ClickSimilarPointsException:
                        {
                            _continuousSimilarPointsCount += 1;
                            ErrorCheck(_continuousSimilarPointsCount);

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

        private static bool ArePointsSimilar(Point p1, Point p2, int tolerance)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) < tolerance;
        }

        private static void ErrorCheck(int suspiciousMoveCount)
        {
            if (suspiciousMoveCount >= ERROR_WARNING_COUNT)
            {
                Console.WriteLine($"{suspiciousMoveCount} continuous errors occur, about to abort reaching {ERROR_TOLERANCE_COUNT} times error.");
            }

            if (suspiciousMoveCount >= ERROR_TOLERANCE_COUNT)
            {
                Console.WriteLine($"{suspiciousMoveCount} continuous errors occur. Abortíng the operation. Could be out of bloodpoints or error.");
                Environment.Exit(0);
            }
        }

        private static void ResetMousePosition()
        {
            SetCursorPos(0, 0);
        }
    }
}
