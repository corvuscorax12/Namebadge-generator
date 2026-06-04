using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Windows;

namespace YawgoogIdGenerator
{
    /// <summary>
    /// File management Stuff
    /// </summary>

    class Fileman
    {
        private string csvPath;
        //TODO: Return stuff for error handeling
        public void CreateCsv()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            Stream mysteam;
            saveFileDialog.Filter = "CSV FILES|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                if ((mysteam = saveFileDialog.OpenFile()) != null)
                {
                    mysteam.Close();
                }
                csvPath = saveFileDialog.FileName;

            }
        }
        public void CreateBackUp(string  csvPath)
        {
            var newcsvpath = csvPath + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")+".txt";
            File.Copy(csvPath, newcsvpath);
        }
        public void SetCsv(string CSVPath)
        {
            csvPath = CSVPath;

        }
        public string GetCsv()
        {
            return csvPath;
        }


        public int CheckPhoto(string photo)
        {
            try
            { 

                Image img = Image.FromFile(photo);
                float desiredRatio = (float)(img.Width) / (float)(img.Height);
                // 2:3 aspect ratio
                if (desiredRatio <= 1.12)
                {
                    return 0;
                }

                else
                {
                    return 4;
                }
            }
            catch (Exception)
            {
                return 4;
            }
        }

    }
       

}
