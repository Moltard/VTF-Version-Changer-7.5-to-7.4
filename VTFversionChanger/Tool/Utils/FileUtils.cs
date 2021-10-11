using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTFversionChanger.Tool.Utils
{
    internal enum FileFilters
    {
        None,
        Vtf
    }

    internal static class FileUtils
    {
        #region Constants

        private const string FilterAny = "All files (*.*)|*.*";
        private const string FilterVtf = "Valve Texture File (*.vtf)|*.vtf";
        private const string FolderSelection = "[Folder Selection]";

        #endregion

        #region Methods - Files / Folders




        /// <summary>
        /// Return true if the file has the given extension
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static bool IsFileExtension(string filename, string extension)
        {
            return extension == Path.GetExtension(filename);
        }

        /// <summary>
        /// Return true if the file has one of the given extensions
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static bool IsFileExtension(string filename, string[] extensions)
        {
            return extensions.Contains(Path.GetExtension(filename));
        }


        /// <summary>
        /// Return the list of files located in the given directory. null if error
        /// </summary>
        /// <param name="dir">Directory</param>
        /// <returns>List of files or null or throw an Exception</returns>
        public static List<string> GetFiles(string dir)
        {
            return GetFiles(dir, FileConstants.PatternAnyFile);
        }

        /// <summary>
        /// Return the list of files located in the given directory, following a specific pattern. null if error
        /// </summary>
        /// <param name="dir">Directory</param>
        /// <returns>List of files or null or throw an Exception</returns>
        public static List<string> GetFiles(string dir, string pattern)
        {
            return GetFiles(dir, pattern, false);
        }

        /// <summary>
        /// Return the list of files located in the given directory and possibly subdirectories, following a specific pattern.
        /// </summary>
        /// <param name="directory">Directory</param>
        /// <param name="pattern">Search pattern</param>
        /// <param name="allDirectories">Include the subdirectories</param>
        /// <returns>List of files or null or throw an Exception</returns>
        public static List<string> GetFiles(string directory, string pattern, bool allDirectories)
        {
            if (Directory.Exists(directory))
            {
                SearchOption searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                return new List<string>(Directory.GetFiles(directory, pattern, searchOption));
            }
            return null;
        }

        #endregion

        #region Methods - Dialog File / Folder 

        /// <summary>
        /// Return the file filter depending of the given value
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetFileFilter(FileFilters filter)
        {
            switch (filter)
            {
                case FileFilters.Vtf:
                    return FilterVtf;
                case FileFilters.None:
                default:
                    break;
            }
            return string.Empty;
        }

        /// <summary>
        /// Open the File Browser Dialog and return the path of the selected file, null if none
        /// </summary>
        /// <param name="filter">Filter for the file selection</param>
        /// <returns></returns>
        public static string OpenFileDialog(FileFilters filter)
        {
            return OpenFileDialog(GetFileFilter(filter));
        }

        private static string OpenFileDialog(string filter)
        {
            Microsoft.Win32.FileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter
            };
            bool? result = fileDialog.ShowDialog();
            return result == true ? fileDialog.FileName : null;
        }

        /// <summary>
        /// Open the File Browser Dialog and return the list of selected files, null if none
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string[] OpenFilesDialog(FileFilters filter)
        {
            return OpenFilesDialog(GetFileFilter(filter));
        }

        private static string[] OpenFilesDialog(string filter)
        {
            Microsoft.Win32.FileDialog fileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = filter,
                Multiselect = true
            };
            bool? result = fileDialog.ShowDialog();
            return result == true ? fileDialog.FileNames : null;
        }

        /// <summary>
        /// Open the File Browser Dialog and return the path of the selected folder, null if none
        /// </summary>
        /// <returns>Returns the selected folder</returns>
        public static string OpenDirectoryDialog()
        {
            Microsoft.Win32.FileDialog folderDialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                FileName = FolderSelection // Default name
            };
            bool? result = folderDialog.ShowDialog();
            return result == true ? Path.GetDirectoryName(folderDialog.FileName) : null;
        }

        /// <summary>
        /// Open the Save File Browser Dialog with the given settings
        /// </summary>
        /// <param name="fileName">Default filename</param>
        /// <param name="defaultExt">Default extension</param>
        /// <param name="filter">Files to filter</param>
        /// <param name="initialDirectory">Intitial directory</param>
        /// <returns>Return the path of the file to save, null if none</returns>
        public static string SaveFileDialog(string fileName, string defaultExt, string filter, string initialDirectory)
        {
            if (initialDirectory == null)
            {
                initialDirectory = string.Empty;
            }
            Microsoft.Win32.FileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = initialDirectory,
                Filter = filter,
                FileName = fileName,
                DefaultExt = defaultExt,
            };

            bool? result = saveDialog.ShowDialog();
            return result == true ? saveDialog.FileName : null;
        }

        #endregion

    }
}
