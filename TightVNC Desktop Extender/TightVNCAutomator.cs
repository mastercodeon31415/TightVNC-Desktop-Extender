using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Tray_TightVNC_Poller_Service
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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X; public int Y; }

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        #endregion

        #region Constants

        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;

        #endregion

        public static void RunAutomation(string hostName, bool hideFromTaskbar)
        {
            string windowClass = "TvnWindowClass";
            string windowCaption = $"{hostName.ToLower()} - TightVNC Viewer";
            IntPtr vncWindowHandle;

            // --- NEW: Check for an existing VNC window first ---
            vncWindowHandle = FindWindow(windowClass, windowCaption);

            if (vncWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("No existing TightVNC window found. Starting a new process...");
                Process vncProcess = null;
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\Program Files\TightVNC\tvnviewer.exe",
                        Arguments = $"-fullscreen {hostName}",
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

                // Wait for the new window to appear
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(250); // Give the process time to initialize
                    vncWindowHandle = FindWindow(windowClass, windowCaption);
                    if (vncWindowHandle != IntPtr.Zero)
                    {
                        Console.WriteLine("Newly started TightVNC window found.");
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Existing TightVNC window found. Reusing and repositioning it.");
            }

            // --- Common logic for both new and existing windows ---
            if (vncWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("Could not find the TightVNC window after attempting to start/find it.");
                return;
            }

            // *** NEW: Apply the taskbar visibility style ***
            int extendedStyle = GetWindowLong(vncWindowHandle, GWL_EXSTYLE);
            if (hideFromTaskbar)
            {
                // To hide from taskbar, add TOOLWINDOW style and remove APPWINDOW style
                SetWindowLong(vncWindowHandle, GWL_EXSTYLE, (extendedStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
                Console.WriteLine("Hiding VNC window from the taskbar.");
            }
            else
            {
                // To show on taskbar, add APPWINDOW style and remove TOOLWINDOW style
                SetWindowLong(vncWindowHandle, GWL_EXSTYLE, (extendedStyle | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW);
                Console.WriteLine("Ensuring VNC window is visible on the taskbar.");
            }

            // --- Reposition and Maximize Logic (works for new or existing windows) ---
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

            // --- Simulate Hardware Click ---
            Console.WriteLine("Bringing window to the foreground to ensure focus.");
            SetForegroundWindow(vncWindowHandle);
            Thread.Sleep(250);

            var point = new POINT { X = 400, Y = 12 };
            ClientToScreen(vncWindowHandle, ref point);
            Console.WriteLine($"Calculated absolute screen coordinates: ({point.X}, {point.Y})");

            SetCursorPos(point.X, point.Y);
            Console.WriteLine("System cursor moved.");
            Thread.Sleep(100);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
            Console.WriteLine("Hardware mouse click simulated.");
        }
    }
}
