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
        public MainWindow()
        {
            settings.PhotoPath = settings.loadValue("");
            Directory.SetCurrentDirectory(settings.PhotoPath);
            InitializeComponent();
            fileman = new Fileman();
            dataman = new Dataman();
            dtg_staffData.AutoGenerateColumns = false;
            LoadLogoIntoWpf();



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
            if (fileDialog.ShowDialog() == true)
            {

                fileman.SetCsv(fileDialog.FileName);
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
                btn_exportIDs.IsEnabled = false;
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

                            if (genid.genid(dataman, i, ratioMode) != null)
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

        private void btn_print_Click(object sender, RoutedEventArgs e)
        {
            if (printer == null || pTicket == null)
            {
                MessageBox.Show("Invalid Print Settings Have Please click the settings button before printing");

            }
            else
            {

                int rowIdx = int.Parse(tbx_idIndex.Text);
                var img = genid.GetImage(rowIdx);
                var datItem = dtg_staffData.Items[validIds[rowIdx]] as ParseInfo;
                datItem.Done = true;
                Image imgtoprint = new Image();
                imgtoprint.Source = img;
                PrintDialog dialog = new PrintDialog { PrintQueue = printer, PrintTicket = pTicket };
                dialog.PrintVisual(imgtoprint, $"PrintJob {DateTime.Now.ToString()}");
                dtg_staffData.Items.Refresh();


            }


        }



        private void btn_printsettings_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printer = printDialog.PrintQueue;
                pTicket = printDialog.PrintTicket;
            }

        }

        private void btn_quickID_Click(object sender, RoutedEventArgs e)
        {

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
                    Dataman tmpdata = new Dataman(); 
                    tmpdata.data = new List<ParseInfo> { selected };

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

                        if (genid.genid(tmpdata, 0, ratioMode) != null)
                        {
                            validIds.Add(0);
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
            if(photoDirectory.ShowDialog() == true)
            {
                settings.PhotoPath = photoDirectory.FolderName;
                Directory.SetCurrentDirectory(settings.PhotoPath);
                if(MessageBox.Show("Save as Default","Confirmation",MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    settings.saveValue(settings.PhotoPath);
                }
                update_CropStatus();

            }




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
            fileman.CreateBackUp(fileman.GetCsv());
            Thread.Sleep(1000);

        }
    }
}