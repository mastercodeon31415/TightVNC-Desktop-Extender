using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tray_TightVNC_Poller_Service
{
    public class AppSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int PollIntervalMilliseconds { get; set; }
        public bool IsPollingPaused { get; set; }

        // *** NEW: Setting to control taskbar visibility ***
        public bool HideVncWindowFromTaskbar { get; set; } = false; // Default to showing the icon

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Host) &&
                   Port > 0 &&
                   PollIntervalMilliseconds > 0;
        }
    }
}
