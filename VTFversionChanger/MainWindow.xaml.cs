using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VTFversionChanger.Models;
using VTFversionChanger.Tool;
using VTFversionChanger.Tool.Utils;

namespace VTFversionChanger
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Attributes

        private readonly LogText logText = new LogText();
        private readonly Models.ProgressBar progressBar = new Models.ProgressBar();

        // Default Green color of a progress bar
        private readonly SolidColorBrush progressBarColorGreen = new SolidColorBrush(Color.FromRgb(6, 176, 37));
        // Orange Color for a progress bar (represent Cancel)
        private readonly SolidColorBrush progressBarColorOrange = new SolidColorBrush(Color.FromRgb(250, 155, 50));
        // Red Color for a progress bar (represent Error)
        private readonly SolidColorBrush progressBarColorRed = new SolidColorBrush(Color.FromRgb(238, 70, 70));

        /// <summary>
        /// Delegate function used to give multiple purpose to the Cancel Button in Logs
        /// </summary>
        private delegate void CancelButton();

        /// <summary>
        /// Delegate executed by the Cancel Button
        /// </summary>
        private CancelButton cancelButton;

        /// <summary>
        /// ConverterVtfFolder used to be able to access the VTF converter asynchronously
        /// </summary>
        private ConverterVtfFolder currentVTFConverter;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            InitProgressBar();

            InitLogTextBox();
        }

        #endregion

        #region Initialize GUI

        private void InitProgressBar()
        {
            LogsProgressBar.DataContext = progressBar;
        }

        private void InitLogTextBox()
        {
            LogsTextBox.DataContext = logText;
        }

        // LogsTextBox

        #endregion

        #region Events Drag and Drop

        /// <summary>
        /// Allow dragging on TextBox
        /// </summary>
        /// <param name="e"></param>
        private void DragOverTextBoxFix(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
        }

        /// <summary>
        /// Return the list of dropped path for the DragDrop event
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string[] DragDropGetPaths(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                return e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }
            return null;
        }

        /// <summary>
        /// Extract the path of the first dragged file matching the mode
        /// </summary>
        /// <param name="e">Event data</param>
        /// <param name="mode">Mode</param>
        /// <returns>The path is a matching file found, null otherwise</returns>
        private string DragDropGetPath(DragEventArgs e, DragDropMode mode)
        {
            string filePath = null;
            string[] droppedFilePaths = DragDropGetPaths(e);
            if (droppedFilePaths != null)
            {
                switch (mode)
                {
                    case DragDropMode.Any:
                        // Get the first dragged file
                        if (droppedFilePaths.Length > 0)
                        {
                            filePath = droppedFilePaths[0];
                        }
                        break;
                    case DragDropMode.Vtf:
                        foreach (string path in droppedFilePaths)
                        {
                            // Get the first VTF dragged if multiple files are dragged
                            if (FileUtils.IsFileExtension(path, FileConstants.ExtensionVtf))
                            {
                                filePath = path;
                                break;
                            }
                        }
                        break;
                    case DragDropMode.Directory:
                        foreach (string path in droppedFilePaths)
                        {
                            // Get the directory path of the dragged files
                            if (System.IO.Directory.Exists(path))
                            {
                                filePath = path;
                                break;
                            }
                            else if (System.IO.File.Exists(path))
                            {
                                filePath = System.IO.Path.GetDirectoryName(path);
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return filePath;
        }

        #endregion


        #region Events Generic GUI

        /// <summary>
        /// Lock all tabs but Logs
        /// </summary>
        private void LockNonLogTabs()
        {
            TabVtfFolder.IsEnabled = false;
            TabVtfFile.IsEnabled = false;
        }

        /// <summary>
        /// Unlock all tabs
        /// </summary>
        private void UnlockAllTabs()
        {
            TabVtfFolder.IsEnabled = true;
            TabVtfFile.IsEnabled = true;
            TabLogs.IsEnabled = true;
        }

        #endregion

        #region Convert VTF Folder

        private void VtfFolderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNotEmpty = !string.IsNullOrEmpty(VtfFolderTextBox.Text);
            ConvertVtfFolderButton.IsEnabled = isNotEmpty;
        }

        private void VtfFolderBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string directoryPath = FileUtils.OpenDirectoryDialog();
            if (directoryPath != null)
            {
                VtfFolderTextBox.Text = directoryPath;
            }
        }


        private void VtfFolderTextBox_Drop(object sender, DragEventArgs e)
        {
            string directoryPath = DragDropGetPath(e, DragDropMode.Directory);
            if (directoryPath != null)
            {
                VtfFolderTextBox.Text = directoryPath;
            }
        }



        private void ConvertVtfFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string vtfDirectory = VtfFolderTextBox.Text;
            bool recursive = (bool)VtfFolderCheckboxRecursive.IsChecked;
            bool verbose = (bool)VtfFolderCheckboxVerbose.IsChecked;
            bool stopOnError = (bool)VtfFolderCheckboxError.IsChecked;

            if (System.IO.Directory.Exists(vtfDirectory))
            {
                List<string> listVtf;
                try
                {
                    listVtf = FileUtils.GetFiles(vtfDirectory, FileConstants.PatternExtensionVtf, recursive);
                }
                catch(Exception ex)
                {
                    string messageText = $"Error getting the list of VTF file in: {vtfDirectory}\nException: {ex.Message}";
                    MessageBox.Show(messageText, MessageConstants.MessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (listVtf != null && listVtf.Count != 0)
                {
                    string recursiveText = recursive ? "and its subfolders " : string.Empty;
                    string questionText = $"Found {listVtf.Count} VTF files in {vtfDirectory} {recursiveText}\nContinue ?";

                    MessageBoxResult result = MessageBox.Show(questionText,
                                            MessageConstants.MessageTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            ResetLogsUIElements();
                            logText.AppendLine($"Folder: {vtfDirectory}\nRecursive: {recursive}\nVerbose: {verbose}\nStop on error: {stopOnError}\n");
                            ExecuteConvertVtfFolder(listVtf, verbose, stopOnError);
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }
                else
                {
                    MessageBox.Show(MessageConstants.MessageFolderNoVTF, MessageConstants.MessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(MessageConstants.MessageFolderNotFound, MessageConstants.MessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void ExecuteConvertVtfFolder(List<string> listVtf, bool verbose, bool stopOnError)
        {
            ConverterVtfFolder converter = new ConverterVtfFolder(progressBar, logText, listVtf, verbose, stopOnError);
            currentVTFConverter = converter;

            // Change the current tab and lock all others
            MainTabControl.SelectedItem = TabLogs;
            LockNonLogTabs();

            // Make the Cancel Button stop the conversion process
            cancelButton = new CancelButton(VtfFolderCancelConversion);
            LogsCancelButton.IsEnabled = true;

            MessageBoxImage messageIcon = MessageBoxImage.None;
            string messageText = string.Empty;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                await ExecuteTaskConvertVtfFolder(converter);
                sw.Stop();
                messageIcon = MessageBoxImage.Information;
                messageText = MessageConstants.MessageVtfFolderSuccess;
            }
            catch (CancelConversionVTFException ex)
            {
                // User Cancel
                sw.Stop();
                SetCancelProgressBar();
                messageIcon = MessageBoxImage.Warning;
                messageText = MessageConstants.MessageVtfFolderCancel;
            }
            catch (ErrorConversionVTFException ex)
            {
                // Error during the conversion
                sw.Stop();
                SetErrorProgressBar();
                messageIcon = MessageBoxImage.Error;
                messageText = MessageConstants.MessageVtfFolderError;
            }
            finally
            {
                logText.AppendLine($"Time elapsed: {sw.ElapsedMilliseconds} ms");
                LogsCancelButton.IsEnabled = false;
                LogsTextBox.ScrollToEnd();
                MessageBox.Show(messageText, MessageConstants.MessageTitle, MessageBoxButton.OK, messageIcon);
            }

            // Unlock the tabs
            cancelButton = null;
            UnlockAllTabs();

        }

        private async Task ExecuteTaskConvertVtfFolder(ConverterVtfFolder converter)
        {
            try
            {
                await Task.Run(() =>
                {
                    converter.Start();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Cancel the current VTF Folder conversion
        /// </summary>
        private void VtfFolderCancelConversion()
        {
            if (currentVTFConverter != null)
            {
                // Disable the Cancel Button
                LogsCancelButton.IsEnabled = false;
                // Stop the process after the current conversion is done
                currentVTFConverter.Stop();
            }
        }

        #endregion

        #region Convert VTF File

        private void VtfFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isNotEmpty = !string.IsNullOrEmpty(VtfFileTextBox.Text);
            ConvertVtfFileButton.IsEnabled = isNotEmpty;
        }

        private void VtfFileBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            string filePath = FileUtils.OpenFileDialog(FileFilters.Vtf);
            if (filePath != null)
            {
                VtfFileTextBox.Text = filePath;
            }
        }

        private void VtfFileTextBox_Drop(object sender, DragEventArgs e)
        {
            string filePath = DragDropGetPath(e, DragDropMode.Vtf);
            if (filePath != null)
            {
                VtfFileTextBox.Text = filePath;
            }
        }

        private void ConvertVtfFileButton_Click(object sender, RoutedEventArgs e)
        {
            string vtfFile = VtfFileTextBox.Text;
            if (System.IO.File.Exists(vtfFile))
            {
                if (FileUtils.IsFileExtension(vtfFile, FileConstants.ExtensionVtf))
                {
                    MessageBoxImage messageIcon = MessageBoxImage.None;
                    string messageText = string.Empty;
                    try
                    {
                        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        bool success = ConvertVtfFile.ConvertVTFVersion(vtfFile);
                        sw.Stop();
                        if (success)
                        {
                            messageIcon = MessageBoxImage.Information;
                            messageText = $"Converted {vtfFile} to 7.4\nTime elapsed: {sw.ElapsedMilliseconds} ms";
                        }
                        else
                        {
                            messageIcon = MessageBoxImage.Warning;
                            messageText = $"Couldn't convert the file.\n{vtfFile} is not 7.5";
                        }
                    }
                    catch (WrongVTFIdException ex)
                    {
                        messageIcon = MessageBoxImage.Warning;
                        messageText = $"The file is not a valid VTF:\n{ex.Message}";
                    }
                    catch (Exception ex)
                    {
                        
                        messageIcon = MessageBoxImage.Error;
                        messageText = $"Error while converting the file:\n{ex.Message}";

                    }
                    finally
                    {
                        MessageBox.Show(messageText, MessageConstants.MessageTitle, MessageBoxButton.OK, messageIcon);
                    }
                }
                else
                {
                    MessageBox.Show(MessageConstants.MessageFileNotVtf, MessageConstants.MessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show(MessageConstants.MessageFileNotFound, MessageConstants.MessageTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Logs

        /// <summary>
        /// Reset the Log textbox and the ProgressBar value (+ Color)
        /// </summary>
        private void ResetLogsUIElements()
        {
            logText.Clear();
            progressBar.Reset();
            SetDefaultProgressBar();
        }

        /// <summary>
        /// Make the ProgressBar green (default color)
        /// </summary>
        private void SetDefaultProgressBar()
        {
            LogsProgressBar.Foreground = progressBarColorGreen;
        }

        /// <summary>
        /// Make the ProgressBar orange to represent a cancel action by the user
        /// </summary>
        private void SetCancelProgressBar()
        {
            LogsProgressBar.Foreground = progressBarColorOrange;
        }

        /// <summary>
        /// Make the ProgressBar red to represent an error
        /// </summary>
        private void SetErrorProgressBar()
        {
            LogsProgressBar.Foreground = progressBarColorRed;
        }

        private void LogsCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancelButton != null && cancelButton.Target != null)
            {
                // Execute the instancied method of the delegate
                cancelButton();
            }
        }

        #endregion


        #region Debuggage async

        /// <summary>
        /// Stop execution for a specific amount of time without blocking the UI
        /// </summary>
        /// <param name="interval">The time to wait in milliseconds</param>
        private static void Wait(int interval)
        {
            ExecuteWait(() => System.Threading.Thread.Sleep(interval));
        }

        private static void ExecuteWait(Action action)
        {
            System.Windows.Threading.DispatcherFrame waitFrame = new System.Windows.Threading.DispatcherFrame();

            // Use callback to "pop" dispatcher frame
            IAsyncResult op = action.BeginInvoke(dummy => waitFrame.Continue = false, null);

            // this method will block here but window messages are pumped
            System.Windows.Threading.Dispatcher.PushFrame(waitFrame);

            // this method may throw if the action threw. caller's responsibility to handle.
            action.EndInvoke(op);
        }

        #endregion

    }


    public enum DragDropMode
    {
        Any,
        Vtf,
        Directory
    }


}
