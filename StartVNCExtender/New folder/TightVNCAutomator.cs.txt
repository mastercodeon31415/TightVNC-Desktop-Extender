using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms; // Added for screen detection

namespace StartVNCExtender
{
    public class TightVNCAutomator
    {
        #region P/Invoke Declarations

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // THE FIX: New API calls for hardware-level input simulation
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        #endregion

        #region Constants

        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;

        // Constants for mouse_event
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;

        #endregion

        public static void RunAutomation()
        {
            // ... (Steps 1, 2, and 3 are unchanged) ...
            Process vncProcess = null;
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\TightVNC\tvnviewer.exe",
                    Arguments = "-fullscreen Devtop1-W530",
                    WindowStyle = ProcessWindowStyle.Minimized
                };
                vncProcess = Process.Start(startInfo);
                Console.WriteLine("TightVNC viewer process started in a minimized state.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start TightVNC viewer: {ex.Message}");
                return;
            }
            Thread.Sleep(2000);
            string windowClass = "TvnWindowClass";
            string windowCaption = "devtop1-w530 - TightVNC Viewer";
            IntPtr vncWindowHandle = IntPtr.Zero;
            for (int i = 0; i < 10; i++)
            {
                vncWindowHandle = FindWindow(windowClass, windowCaption);
                if (vncWindowHandle != IntPtr.Zero)
                {
                    Console.WriteLine("TightVNC window found.");
                    break;
                }
                Thread.Sleep(125);
            }
            if (vncWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("Could not find the TightVNC window.");
                vncProcess?.Kill();
                return;
            }
            Screen secondaryScreen = Screen.AllScreens.FirstOrDefault(s => !s.Primary);
            if (secondaryScreen == null)
            {
                Console.WriteLine("No secondary display found. Maximizing on primary display.");
                ShowWindow(vncWindowHandle, SW_MAXIMIZE);
            }
            else
            {
                Console.WriteLine("Restoring window to normal state before moving...");
                ShowWindow(vncWindowHandle, SW_RESTORE);
                Thread.Sleep(250);
                int secondaryScreenX = secondaryScreen.Bounds.X;
                int secondaryScreenY = secondaryScreen.Bounds.Y;
                SetWindowPos(vncWindowHandle, HWND_TOP, secondaryScreenX, secondaryScreenY, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                Console.WriteLine($"Window moved to secondary display at ({secondaryScreenX}, {secondaryScreenY}).");
                Thread.Sleep(250);
                Console.WriteLine("Maximizing window on the secondary display.");
                ShowWindow(vncWindowHandle, SW_MAXIMIZE);
            }
            Thread.Sleep(1000);

            // --- NEW AND IMPROVED STEP 4: HARDWARE CLICK ---

            // 4a. Bring the window to the absolute foreground
            Console.WriteLine("Bringing window to the foreground to ensure focus.");
            SetForegroundWindow(vncWindowHandle);
            Thread.Sleep(250);

            // 4b. Define the client coordinates and convert them to absolute screen coordinates
            var point = new POINT { X = 400, Y = 12 };
            ClientToScreen(vncWindowHandle, ref point);
            Console.WriteLine($"Calculated absolute screen coordinates: ({point.X}, {point.Y})");

            // 4c. Move the system cursor to the target coordinates
            SetCursorPos(point.X, point.Y);
            Console.WriteLine("System cursor moved.");
            Thread.Sleep(100);

            // 4d. Simulate a left mouse button down and up event
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50); // A brief pause between down and up is good practice
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("Hardware mouse click simulated.");
        }
    }
}
