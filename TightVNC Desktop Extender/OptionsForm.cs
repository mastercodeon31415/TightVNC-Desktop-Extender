using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tray_TightVNC_Poller_Service
{
    public partial class OptionsForm : Form
    {
        public AppSettings CurrentSettings { get; private set; }

        public OptionsForm(AppSettings settings, bool isDarkMode)
        {
            InitializeComponent();
            CurrentSettings = settings;
            LoadSettings();

            if (isDarkMode)
            {
                this.Icon = Properties.Resources.appCornflowerBlue;
            }
            else
            {
                this.Icon = Properties.Resources.appDark;
            }
        }

        private void LoadSettings()
        {
            txtHost.Text = CurrentSettings.Host;
            numPort.Value = CurrentSettings.Port > 0 ? CurrentSettings.Port : 5900;
            numInterval.Value = CurrentSettings.PollIntervalMilliseconds > 0 ? CurrentSettings.PollIntervalMilliseconds : 1000;
            btnPauseResume.Text = CurrentSettings.IsPollingPaused ? "Resume Polling" : "Pause Polling";
            // *** NEW: Load the setting into the checkbox ***
            chkHideFromTaskbar.Checked = CurrentSettings.HideVncWindowFromTaskbar;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHost.Text))
            {
                MessageBox.Show("Host name cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CurrentSettings.Host = txtHost.Text;
            CurrentSettings.Port = (int)numPort.Value;
            CurrentSettings.PollIntervalMilliseconds = (int)numInterval.Value;
            // *** NEW: Save the setting from the checkbox ***
            CurrentSettings.HideVncWindowFromTaskbar = chkHideFromTaskbar.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnPauseResume_Click(object sender, EventArgs e)
        {
            CurrentSettings.IsPollingPaused = !CurrentSettings.IsPollingPaused;
            btnPauseResume.Text = CurrentSettings.IsPollingPaused ? "Resume Polling" : "Pause Polling";
        }
    }
}
