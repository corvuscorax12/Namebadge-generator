using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using static QuestPDF.Helpers.Colors;

namespace YawgoogIdGenerator
{
    public class CurrentSettings
    {
        public string PhotoPath { get; set; }
       // public string Default_CSV { get; set; }
        public int BackupMode { get; set; }
        public int Year { get; set; }
        public int SetToCurrentYear { get; set; }
        public int SaveAfterPrint   { get; set; }
        public string DefaultStaffList { get; set; }

        public string PrinterName { get; set; }

        public int IgnoreDone { get; set; }

        public string PrinterSettings { get; set; }

        public CurrentSettings()
        {
            PhotoPath = "";
            BackupMode = 0;
            Year = 1970;
            SetToCurrentYear = 1;
            SaveAfterPrint = 1;
            IgnoreDone = 1;
            DefaultStaffList = "";
            PrinterName = "";
            PrinterSettings = "";
        }
    }
    internal class Settings
    {
        private  readonly string folderPath;
        private  readonly string filePath;
        public   string PhotoPath;
        public  List<CurrentSettings> currentSettings = new List<CurrentSettings>();
        public Settings()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folderPath = Path.Combine(appData, "YawgoogIdGen");
            filePath = Path.Combine(folderPath, "settings.json");
        }
        public  void saveValue(string value)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            File.WriteAllText(filePath, value);
        }
        public void saveJson(CurrentSettings _newSettings)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(folderPath);
            }
            currentSettings.Clear();
            currentSettings.Add(_newSettings);

            string json = JsonSerializer.Serialize(currentSettings);
            File.WriteAllText(filePath, json);
        }
        public void loadValue(string defaultValue = "")
        {
            if (!File.Exists(filePath))
            {
                currentSettings.Add(new CurrentSettings());
            }
            else
            {
                var json = File.ReadAllText(filePath);
                CurrentSettings[] dict = JsonSerializer.Deserialize<CurrentSettings[]>(json);
                currentSettings.Add(dict[0]);
            }

        }
    }
}
