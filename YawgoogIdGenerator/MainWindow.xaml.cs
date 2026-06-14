using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Configuration.Internal;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Printing;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Runtime.InteropServices;
namespace YawgoogIdGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Fileman fileman;
        Dataman dataman;
        Stack<List<ParseInfo>> undoredo = new Stack<List<ParseInfo>>();
        IdGen genid = new IdGen();
        PrintDocument printDocument = new PrintDocument();
        PrintQueue printer;
        bool fileOpened = false;
        PrintTicket pTicket;
        Settings settings =  new Settings();
        List<int> validIds = new List<int>();
        Printing printing = new Printing();


        private void ApplySettingsIntoPage()
        {
            var _settings = settings.currentSettings[0];
            if (_settings.PhotoPath == null)
            {
                tbx_PhotoPathDirectory.Text = "";
            }
            else
            {
            tbx_PhotoPathDirectory.Text = _settings.PhotoPath;
            }
            tbx_year.Text = _settings.Year.ToString();
            if (_settings.SetToCurrentYear != 0)
            {
                chk_currentYear.IsChecked = true;

            }
            else
            {
                chk_currentYear.IsChecked = false;
            }

            switch (_settings.BackupMode)
            {
                case 0:
                    rad_psession.IsChecked = true;
                    rad_csession.IsChecked = false;
                    break;
                case 1:
                    rad_psession.IsChecked = false;
                    rad_csession.IsChecked = true;
                    break;
                default:
                    rad_psession.IsChecked = true;
                    rad_csession.IsChecked = false;
                    break;
            }
            if (_settings.SaveAfterPrint != 0)
            {
                chk_saveAfterPrint.IsChecked = true;

            }
            else
            {
                chk_saveAfterPrint.IsChecked = false;
            }
            tbx_defaultStaffList.Text = _settings.DefaultStaffList;

            printing.printerName = _settings.PrinterName;
            printing.printsettingsxml = _settings.PrinterSettings;

        }
        public MainWindow()
        {
            settings.loadValue();
            if (settings.currentSettings[0].PhotoPath != "")
            {
                try
                {
                    Directory.SetCurrentDirectory(settings.currentSettings[0].PhotoPath);
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot Find the Path For the Photos please select it again");

                }
            }
            //settings.PhotoPath = settings.loadValue("");
            //Directory.SetCurrentDirectory(settings.PhotoPath);
            InitializeComponent();
            ApplySettingsIntoPage();
            fileman = new Fileman();
            dataman = new Dataman();
            dtg_staffData.AutoGenerateColumns = false;
            LoadLogoIntoWpf();
            if (tbx_defaultStaffList.Text == "")
            {
        
            }
            else if (File.Exists(tbx_defaultStaffList.Text) == true)
            {
                loadfiles(tbx_defaultStaffList.Text);
            }
            else
            {
                MessageBox.Show("Cannot Find The Default File Try Setting it again");
            }
            if (printing.printerName != "" && printing.printsettingsxml != "")
            {    
             printing.loadSettings();
            }




        }
        public void LoadLogoIntoWpf()
        {
            var assembly = Assembly.GetExecutingAssembly();

            // 1. Read the embedded resource exactly like before
            using (Stream? stream = assembly.GetManifestResourceStream("logo"))
            {
                if (stream == null) return; // Fail-safe check

                using (MemoryStream ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Position = 0;

                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    img_Logo.Source = bitmap;
                }
            }
        }
        private void loadfiles(string fName)
        {
                fileman.SetCsv(fName);
                dataman.populateData(fileman.GetCsv());
                undoredo.Clear();
                dtg_staffData.ItemsSource = null;
                dtg_staffData.ItemsSource = dataman.data;
                undoredo.Push(CloneData(dataman.data));
                btn_add.IsEnabled = true;
                btn_save.IsEnabled = true;
                btn_saveas.IsEnabled = true;
                btn_exportIDs.IsEnabled = true;
                btn_createBackup.IsEnabled = true;
                update_CropStatus();
                tbx_idIndex.Text = "0";
                fileOpened = true;

         } 
        private List<ParseInfo> CloneData(List<ParseInfo> source)
        {
            return source.Select(x => new ParseInfo(x)).ToList();
        }
        private void btn_opencsv_Click(object sender, RoutedEventArgs e)
        {
            genid.clearCards();
            validIds.Clear();
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "text files (*.txt)|*.txt";
            if (fileDialog.ShowDialog()==true)
            {
                loadfiles(fileDialog.FileName);
            }


        }
        private async void update_CropStatus()
        {
            var dataList = dataman.data;
            if (dataList == null) return;
            btn_add.IsEnabled = false;
            btn_createNewCSV.IsEnabled = false;
            btn_opencsv.IsEnabled = false;
            btn_exportIDs.IsEnabled = false;
            dtg_staffData.IsEnabled = false;
            btn_createBackup.IsEnabled = false;
            await Task.Run(() => {
                foreach (var item in dataList)
                {
                    var row = item as ParseInfo;
                    int ratios = fileman.CheckPhoto(row.Photo);
                    if (ratios == 4)
                    {
                        row.Warnings = "🖼️";
                    }
                    else
                    {
                        row.Warnings = "";
                    }
                }
            });
            dtg_staffData.IsEnabled = true;
            btn_add.IsEnabled = true;
            btn_createNewCSV.IsEnabled = true;
            btn_opencsv.IsEnabled = true;
            btn_exportIDs.IsEnabled = true;
            btn_createBackup.IsEnabled = true;

        }

        private void BrowseFile_Click(object sender, RoutedEventArgs e)
        {


            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image files (*.jpg,*.jpeg,*.JPG,*.png,*.gif)|*.jpeg;*.jpg;*.JPG;*.png;*.gif|All files (*.*)|*.*";

            var button = sender as Button;
            var row = button.DataContext as ParseInfo;

            if (fileDialog.ShowDialog() == true)
            {

                if (row != null)
                {
                    row.Photo = fileDialog.SafeFileName;
                    int ratios = fileman.CheckPhoto(row.Photo);
                    if (ratios == 4)
                    {
                        row.Warnings = "🖼️";
                    }
                    else
                    {
                        row.Warnings = "";
                    }
                }


            }
            dtg_staffData.CommitEdit();
            dtg_staffData.CommitEdit();
            dtg_staffData.Items.Refresh();
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            dataman.data.Add(new ParseInfo());
            dtg_staffData.ItemsSource = null;
            dtg_staffData.ItemsSource = dataman.data;
            undoredo.Push(CloneData(dataman.data));

        }

        private void dtg_staffData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btn_exportSelected.IsEnabled = true;
        }

        private void btn_saveas_Click(object sender, RoutedEventArgs e)
        {
            dataman.saveasData();
        }
        private async void btn_exportIDs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                genid.clearCards();
                validIds.Clear();
                int year;
                btn_exportIDs.IsEnabled = false;
                if (chk_currentYear.IsChecked == true)
                {
                    year = DateTime.Now.Year;
                }
                else
                {
                    year = Convert.ToInt16(tbx_year.Text);
                }
                await Task.Run(() =>
                {
                    for (int i = 0; i < dataman.data.Count; i++)
                    {
                        if (dataman.data[i].Done != true)
                        {
                            int ratioMode;
                            if (dataman.data[i].Photo != null && dataman.data[i].Photo != "")
                            {
                                ratioMode = fileman.CheckPhoto(dataman.data[i].Photo);
                            }
                            else
                            {
                                ratioMode = 5;
                            }

                            if (genid.genid(dataman, i, ratioMode,year) != null)
                            {
                                validIds.Add(i);
                            }
                        }
                    }
                });
                btn_exportIDs.IsEnabled = true;
                var img = genid.GetImage(0);
                if (img != null)
                {
                    img_Id.Source = img;
                    tab_ViewTab.SelectedIndex = 1;
                }
                dtg_staffData.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btn_createNewCSV_Click(object sender, RoutedEventArgs e)
        {
            fileman.CreateCsv();
            if (dataman.populateData(fileman.GetCsv()))
            {
                dtg_staffData.ItemsSource = null;
                dtg_staffData.ItemsSource = dataman.data;
                btn_add.IsEnabled = true;
                btn_save.IsEnabled = true;
                btn_saveas.IsEnabled = true;
                btn_exportIDs.IsEnabled = true;
                fileOpened = true;
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            dataman.saveData(fileman.GetCsv());
            update_CropStatus();
        }

        private void dtg_keydown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)
            {

                if (undoredo.Count > 0)
                {
                    undoredo.Pop();
                    try
                    {
                        var prev = CloneData(undoredo.Peek());
                        dtg_staffData.ItemsSource = null;
                        dataman.data = prev;
                        dtg_staffData.ItemsSource = dataman.data;

                    }
                    catch (Exception)
                    {

                    }
                    dtg_staffData.Items.Refresh();
                    dtg_staffData.Focus();
                    dtg_staffData.SelectedIndex = 0;

                }

            }
            else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                dataman.saveData(fileman.GetCsv());
                update_CropStatus();
                MessageBox.Show("Saved");
            }
        }

        private void dtg_afteredit(object sender, DataGridCellEditEndingEventArgs e)
        {
            undoredo.Push(CloneData(dataman.data));

        }

        private void dtg_ondit(object sender, AddingNewItemEventArgs e)
        {
            //undoredo.Push(new List<ParseInfo>(dataman.data));
        }

        private void dtg_staffData_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var grid = sender as DataGrid;
                int nextRowIdx = grid.Items.IndexOf(grid.CurrentItem) + 1;
                if (nextRowIdx < grid.Items.Count)
                {
                    e.Handled = true;
                    grid.CurrentCell = new DataGridCellInfo(grid.Items[nextRowIdx], grid.Columns[0]);
                    grid.BeginEdit();
                }

            }
            if (e.Key == Key.Delete)
            {
                e.Handled = true;

            }
        }

        private void btn_decid_Click(object sender, RoutedEventArgs e)
        {
            int rowIdx = int.Parse(tbx_idIndex.Text);
            rowIdx--;
            if (rowIdx >= 0)
            {
                var img = genid.GetImage(rowIdx);
                img_Id.Source = img;
                tbx_idIndex.Text = rowIdx.ToString();
            }
        }

        private void btn_incid_Click(object sender, RoutedEventArgs e)
        {
            int rowIdx = int.Parse(tbx_idIndex.Text);
            rowIdx++;
            if (rowIdx < genid.getCount)
            {
                var img = genid.GetImage(rowIdx);
                img_Id.Source = img;
                tbx_idIndex.Text = rowIdx.ToString();

            }
        }
        bool singleprint = false;


        private void btn_print_Click(object sender, RoutedEventArgs e)
        {
            if (printing.printer == null || printing.pTicket == null)
            {
                MessageBox.Show("Invalid Print Settings Have Please click the settings button before printing");

            }
            else
            {
                int rowIdx = int.Parse(tbx_idIndex.Text);
                var img = genid.GetImage(rowIdx);
                var datItem = dtg_staffData.Items[validIds[rowIdx]] as ParseInfo;
                dataman.data[validIds[rowIdx]].Done = true;

                datItem.Done = true;
                printing.print(img);
                if (chk_saveAfterPrint.IsChecked == true)
                {
                    dataman.saveData(fileman.GetCsv());
                }

            }


        }



        private void btn_printsettings_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printing.settings(SaveSettings(true),settings);

        }


        private void btn_OpenPhoto_Click(object sender, RoutedEventArgs e)
        {
            var selected = dtg_staffData.SelectedItem as ParseInfo;

            // Fail-safe check to verify a row and a photo path exist
            if (selected != null && !string.IsNullOrEmpty(selected.Photo) && System.IO.File.Exists(selected.Photo))
            {
                try
                {
                    string absolutePath = System.IO.Path.GetFullPath(selected.Photo);

                    // Open Windows File Explorer and automatically highlight/select the specific photo
                    var psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{absolutePath}\"", // The /select flag tells windows to highlight this exact file
                        UseShellExecute = true
                    };

                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to open folder location: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("No valid photo file path found for the selected staff member.");
            }
        }

        private void btn_exportSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dtg_staffData.SelectedItem != null)
                {

                    genid.clearCards();
                    validIds.Clear();
                    tbx_idIndex.Text = "0";

                    var selected = dtg_staffData.SelectedItem as ParseInfo;
                    int row = dtg_staffData.SelectedIndex;

                    Dataman tmpdata = new Dataman(); 
                    tmpdata.data = new List<ParseInfo> { selected };
                    int year;
                    if (chk_currentYear.IsChecked == true)
                    {
                        year = DateTime.Now.Year;
                    }
                    else
                    {
                        year = Convert.ToInt16(tbx_year.Text);
                    }
                    if (selected.Done != true)
                    {
                        int ratioMode;
                        if (selected.Photo != null && selected.Photo != "")
                        {
                            ratioMode = fileman.CheckPhoto(selected.Photo);
                        }
                        else
                        {
                            ratioMode = 5;
                        }

                        if (genid.genid(tmpdata, 0, ratioMode,year) != null)
                        {
                            
                            validIds.Add(row);
                        }
                    }
                    var img = genid.GetImage(0);
                    if (img != null)
                    {
                        img_Id.Source = img;
                        tab_ViewTab.SelectedIndex = 1;
                    }
                    dtg_staffData.Items.Refresh();
                }
            }
            catch (Exception)
            {
            }
        }


        private void btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter != null )
            {
                object rowItemToDelete = button.CommandParameter;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this specific row? \nThis action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning
                    );
                if (result == MessageBoxResult.OK)
                {

                    if (dtg_staffData.ItemsSource is System.Collections.IList list)
                    {
                        dtg_staffData.CancelEdit();
                        list.Remove(rowItemToDelete);
                    }
                    else
                    {
                        dtg_staffData.CancelEdit();
                        dtg_staffData.Items.Remove(rowItemToDelete);
                    }
                    dtg_staffData.Items.Refresh();
                }
            }
        }

        private void btn_SetPhotoDirectory_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog photoDirectory = new OpenFolderDialog { };
            if (photoDirectory.ShowDialog() == true)
            {
                Directory.SetCurrentDirectory(photoDirectory.FolderName);
                if (MessageBox.Show("Save as Default", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    tbx_PhotoPathDirectory.Text = photoDirectory.FolderName;
                    SaveSettings();
                }
                update_CropStatus();

            }
        }

        private CurrentSettings SaveSettings(bool nosave = false)
        {
            CurrentSettings _settings = new CurrentSettings();
            _settings.PhotoPath = tbx_PhotoPathDirectory.Text;
            _settings.Year = Convert.ToInt16(tbx_year.Text);
            if (chk_currentYear.IsChecked == true)
            {
                _settings.SetToCurrentYear = 1;
            }
            else
            {
                _settings.SetToCurrentYear = 0;
            }

            if (rad_psession.IsChecked == true)
            {
                _settings.BackupMode = 0;
            }
            else if (rad_csession.IsChecked == true)
            {
                _settings.BackupMode = 1;
            }
            if (chk_saveAfterPrint.IsChecked == true)
            {
                _settings.SaveAfterPrint = 1;
            }
            else
            {
                _settings.SaveAfterPrint = 0;
            }
            _settings.DefaultStaffList = tbx_defaultStaffList.Text;
            if (!nosave)
            {
                settings.saveJson( _settings );
                return _settings;
            }
            _settings.PrinterName = printing.printerName;
            _settings.PrinterSettings = printing.printsettingsxml;
            return _settings;

        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (fileOpened)
            {
                if (MessageBox.Show("Save Changes?", "Save Changes", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    dataman.saveData(fileman.GetCsv());
                }       
            }

        }

        private void btn_createBackup_Click(object sender, RoutedEventArgs e)
        {
            if (rad_psession.IsChecked == true)
            { 
                fileman.CreateBackUp(fileman.GetCsv());
                Thread.Sleep(1000);
            }
            if (rad_csession.IsChecked == true)
            {
                dataman.saveData(fileman.GetCsv());
                fileman.CreateBackUp(fileman.GetCsv());
                Thread.Sleep(1000);

            }

        }

        private void tbx_year_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);

        }

        private void btn_PhotoPathSetting_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog photoDirectory = new OpenFolderDialog { };
            if (photoDirectory.ShowDialog() == true)
            {
                Directory.SetCurrentDirectory(photoDirectory.FolderName);
                tbx_PhotoPathDirectory.Text = photoDirectory.FolderName;
                update_CropStatus();

            }
        }

        private void chk_currentYear_Checked(object sender, RoutedEventArgs e)
        {
            tbx_year.Text = DateTime.Now.Year.ToString();
        }

        private void btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            //Make it look like its doing somthing :P (it is but doesnt feel like it)
            Mouse.OverrideCursor = Cursors.Wait;
            Thread.Sleep(500);
            Mouse.OverrideCursor = null;

        }

        private void btn_StaffListSetting_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "text files (*.txt)|*.txt";
            if (fileDialog.ShowDialog() == true)
            {
                tbx_defaultStaffList.Text = fileDialog.FileName;
            }
        }

        private void btn_ClearStaffListSetting_Click(object sender, RoutedEventArgs e)
        {
            tbx_defaultStaffList.Text = "";
        }

        private void btn_ClearPhotoPathSetting_Click(object sender, RoutedEventArgs e)
        {
            tbx_PhotoPathDirectory.Text = "";
        }
    }
}