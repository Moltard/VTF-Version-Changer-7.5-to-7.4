using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTFversionChanger.Tool.Utils
{
    internal static class FileConstants
    {

        #region Constants

        public const string ExtensionVtf = ".vtf";

        public const string PatternExtensionVtf = "*.vtf";
        public const string PatternAnyFile = ".*";

        #endregion

    }

    internal static class MessageConstants
    {

        #region Constants

        public const string MessageTitle = "VTF Version Changer";

        public const string MessageFileNotVtf = "The file is not .vtf";
        public const string MessageFileNotFound = "The file doesn't exist";
        public const string MessageFolderNotFound = "The folder doesn't exist";
        public const string MessageFolderNoVTF = "No VTF was found in the folder";

        public const string MessageVtfFolderSuccess = "Success\nSee logs for more details";
        public const string MessageVtfFolderCancel = "The conversion was canceled before the end";
        public const string MessageVtfFolderError = "Error during the conversion\nSee logs for more details";

        #endregion

    }


}
