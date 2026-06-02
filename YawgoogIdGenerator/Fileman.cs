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
                if (desiredRatio >= 0.6 && desiredRatio < 0.70)
                {
                    return 0;
                }
                // 3:4 aspect ratio 
                else if (desiredRatio >= 0.70 && desiredRatio < 0.775)
                {
                    return 1;
                }
                // 4:5 aspect ratio
                else if (desiredRatio >= 0.775 && desiredRatio <= 0.9 )
                {
                    return 2;
                }
                // 1:1
                else if (desiredRatio >= 0.9 && desiredRatio <= 1.1)
                {
                    return 3;
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
