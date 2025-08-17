using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace Tray_TightVNC_Poller_Service
{
    public static class SettingsManager
    {
        private static readonly string _settingsFilePath;

        static SettingsManager()
        {
            // Store settings.json in the same directory as the executable
            _settingsFilePath = Path.Combine(Application.StartupPath, "settings.json");
        }

        public static AppSettings LoadSettings()
        {
            if (!File.Exists(_settingsFilePath))
            {
                // Return a new, empty settings object if the file doesn't exist
                return new AppSettings();
            }

            try
            {
                string json = File.ReadAllText(_settingsFilePath);
                // If the file is somehow empty or corrupt, JsonConvert might return null
                return JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings, will use defaults: {ex.Message}");
                // Return a new object on error to trigger the first-run setup
                return new AppSettings();
            }
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
