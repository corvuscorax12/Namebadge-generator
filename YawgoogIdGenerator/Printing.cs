using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace YawgoogIdGenerator
{

    internal class Printing
    {
       public PrintTicket pTicket;
       public PrintDocument printDocument = new PrintDocument();
       public PrintQueue printer;

       public string printsettingsxml { get; set; }
       public string printerName { get; set; }


        public void settings(CurrentSettings csettings,Settings settings)
        {
           PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                pTicket = printDialog.PrintTicket;
                printer = printDialog.PrintQueue;

                string xmlString;
                using (MemoryStream stream = pTicket.GetXmlStream())
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                       xmlString = reader.ReadToEnd();
                    printsettingsxml = xmlString;
                    printerName = printer.Name;
                    }


                if (MessageBox.Show("Save As Default (This will Save Settings Aswell)", "Save?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    csettings.PrinterName = printer.FullName;
                    csettings.PrinterSettings = xmlString;
                    settings.saveJson(csettings);                  
                }
            }
        }
        public void loadSettings()
        {
            using (LocalPrintServer printerServer = new LocalPrintServer())
            {
                printer = printerServer.GetPrintQueue(printerName);

                byte[] xmlBytes = Encoding.UTF8.GetBytes(printsettingsxml);
                using (MemoryStream stream = new MemoryStream(xmlBytes))
                {
                    pTicket = new PrintTicket(stream);
                }

            }
        }
        public void print(BitmapImage img)
        {
                Image imgtoprint = new Image();
                imgtoprint.Source = img;

                
                PrintDialog dialog = new PrintDialog { PrintQueue = printer, PrintTicket = pTicket };

                dialog.PrintVisual(imgtoprint, $"PrintJob {DateTime.Now.ToString()}");
        }
      

    }
}
