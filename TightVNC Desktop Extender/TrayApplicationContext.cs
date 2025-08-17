using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Tray_TightVNC_Poller_Service
{
    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon _notifyIcon;
        private ToolStripMenuItem _statusMenuItem;
        private ToolStripMenuItem _startStopMenuItem;
        private AppSettings _settings;
        private CancellationTokenSource _cancellationTokenSource;

        public bool IsDarkModeEnabled()
        {
            try
            {
                int? appsUseLightTheme = (int?)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", null);

                if (appsUseLightTheme.HasValue)
                {
                    return appsUseLightTheme.Value == 0;
                }
            }
            catch
            {
                // Handle exceptions if any
            }

            // Default to light mode if the key is not found or an error occurs
            return false;
        }

        bool isDarkMode = false;

        public TrayApplicationContext()
        {
            isDarkMode = IsDarkModeEnabled();

            // --- NEW STARTUP LOGIC ---
            _settings = SettingsManager.LoadSettings();

            // Check if settings are valid. If not, this is a first run or the config is corrupt.
            if (!_settings.IsValid())
            {
                // Show the options form modally. The user MUST configure the app to continue.
                using (var optionsForm = new OptionsForm(_settings, isDarkMode))
                {
                    MessageBox.Show("Welcome! Please configure the VNC server settings to begin.", "First-Time Setup", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    if (optionsForm.ShowDialog() == DialogResult.OK)
                    {
                        // User saved the settings, so we update our instance and save them to the file.
                        _settings = optionsForm.CurrentSettings;
                        SettingsManager.SaveSettings(_settings);
                    }
                    else
                    {
                        // User cancelled the initial setup. The application cannot run.
                        
                        Process.GetCurrentProcess().Kill();

                        return; // Exit the constructor immediately.
                    }
                }
            }
            // --- END OF NEW STARTUP LOGIC ---

            InitializeComponent();
            StartMonitoring();
        }

        private void InitializeComponent()
        {
            // This method is now called only after settings are guaranteed to be valid.
            _statusMenuItem = new ToolStripMenuItem("Status: Initializing...");
            _startStopMenuItem = new ToolStripMenuItem("Stop Monitoring", null, OnStartStopClick);
            var optionsMenuItem = new ToolStripMenuItem("Options", null, OnOptionsClick);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExitClick);

            var contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add(_statusMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(_startStopMenuItem);
            contextMenuStrip.Items.Add(optionsMenuItem);
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            contextMenuStrip.Items.Add(exitMenuItem);

            _notifyIcon = new NotifyIcon
            {
                Icon = Properties.Resources.appDark,
                ContextMenuStrip = contextMenuStrip,
                Visible = true,
                Text = "VNC Extender"
            };

            if (isDarkMode)
            {
                _notifyIcon.Icon = Properties.Resources.appCornflowerBlue;
            }
            else
            {
                _notifyIcon.Icon = Properties.Resources.appDark;
            }
        }

        private void OnOptionsClick(object sender, EventArgs e)
        {
            bool wasPaused = _settings.IsPollingPaused;
            _settings.IsPollingPaused = true;

            using (var optionsForm = new OptionsForm(_settings, isDarkMode))
            {
                if (optionsForm.ShowDialog() == DialogResult.OK)
                {
                    _settings = optionsForm.CurrentSettings;
                    // *** NEW: Save settings whenever they are changed ***
                    SettingsManager.SaveSettings(_settings);
                }
            }
            _settings.IsPollingPaused = wasPaused;
        }

        // ... The rest of the TrayApplicationContext class remains the same ...
        // (OnStartStopClick, StartMonitoring, StopMonitoring, ManageVncConnectionLifecycle, etc.)
        private void OnStartStopClick(object sender, EventArgs e)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                StartMonitoring();
            }
            else
            {
                StopMonitoring();
            }
        }

        private void StartMonitoring()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => ManageVncConnectionLifecycle(_cancellationTokenSource.Token));
            _startStopMenuItem.Text = "Stop Monitoring";
        }

        private void StopMonitoring()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            CloseVncWindows();
            UpdateStatus("Status: Idle");
            _startStopMenuItem.Text = "Start Monitoring";
        }

        private async Task ManageVncConnectionLifecycle(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                UpdateStatus("Status: Waiting for connection...");
                while (!await VncChecker.IsVncServerActiveAsync(_settings.Host, _settings.Port))
                {
                    if (token.IsCancellationRequested) return;
                    await HandlePauseAndDelay(token, "Status: Waiting for connection...");
                }

                if (token.IsCancellationRequested) return;

                UpdateStatus("Status: Connected! Running automation...");
                TightVNCAutomator.RunAutomation(_settings.Host, _settings.HideVncWindowFromTaskbar);

                UpdateStatus("Status: Connected! Monitoring connection...");
                while (await VncChecker.IsVncServerActiveAsync(_settings.Host, _settings.Port))
                {
                    if (token.IsCancellationRequested) return;
                    await HandlePauseAndDelay(token, "Status: Connected! Monitoring connection...");
                }

                if (token.IsCancellationRequested) return;

                UpdateStatus("Status: Disconnected. Closing VNC windows...");
                CloseVncWindows();
                UpdateStatus("Status: Disconnected. Will try to reconnect...");
                await Task.Delay(2000, token);
            }
        }

        private void CloseVncWindows()
        {
            try
            {
                Process[] vncProcesses = Process.GetProcessesByName("tvnviewer");
                if (vncProcesses.Length > 0)
                {
                    Console.WriteLine($"Found {vncProcesses.Length} TightVNC viewer process(es). Terminating...");
                    foreach (var process in vncProcesses)
                    {
                        if (!process.HasExited) process.Kill();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while finding/killing VNC processes: {ex.Message}");
            }
        }

        private async Task HandlePauseAndDelay(CancellationToken token, string activeStatusText)
        {
            while (_settings.IsPollingPaused && !token.IsCancellationRequested)
            {
                UpdateStatus("Status: Paused");
                await Task.Delay(500, token);
            }
            UpdateStatus(activeStatusText);
            await Task.Delay(_settings.PollIntervalMilliseconds, token);
        }

        private void UpdateStatus(string text)
        {
            if (_statusMenuItem?.Owner?.IsHandleCreated == true && _statusMenuItem.Owner.InvokeRequired)
            {
                try
                {
                    _statusMenuItem.Owner.Invoke(new Action(() => _statusMenuItem.Text = text));
                }
                catch (ObjectDisposedException) { /* Ignore */ }
            }
            else if (_statusMenuItem != null)
            {
                _statusMenuItem.Text = text;
            }
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            StopMonitoring();
            _notifyIcon.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _notifyIcon?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
