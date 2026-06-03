using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YawgoogIdGenerator
{
    internal class Settings
    {
        private  readonly string folderPath;
        private  readonly string filePath;
        public   string PhotoPath;
        public Settings()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            folderPath = Path.Combine(appData, "YawgoogIdGen");
            filePath = Path.Combine(folderPath, "settings.txt");
        }
        public  void saveValue(string value)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            File.WriteAllText(filePath, value);
        }
        public  string loadValue(string defaultValue = "")
        {
            if (!File.Exists(filePath))
            {
                return defaultValue;
            }
            return File.ReadAllText(filePath);
        }
    }
}
