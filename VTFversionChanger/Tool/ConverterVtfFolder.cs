using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTFversionChanger.Models;

namespace VTFversionChanger.Tool
{

    internal class ConverterVtfFolder
    {

        /// <summary>
        /// Delegate used to convert a given VTF file<br/>
        /// Case 1: Convert and log<br/>
        /// Case 2: Convert and dont log
        /// </summary>
        /// <param name="vtfFile">VTF file to convert</param>
        private delegate void ConvertProcess(string vtfFile);

        public ProgressBar VTFProgressBar { get; private set; }

        public LogText Log { get; private set; }

        public List<string> VtfFilesList { get; private set; }

        public bool Verbose { get; private set; }

        public bool IsStopped { get; private set; }

        public bool StopOnError { get; private set; }

        public ConverterVtfFolder(ProgressBar vtfProgressBar, LogText log, List<string> vtfFilesList, bool verbose, bool stopOnError)
        {
            VTFProgressBar = vtfProgressBar;
            Log = log;
            VtfFilesList = vtfFilesList;
            Verbose = verbose;
            StopOnError = stopOnError;
            IsStopped = false;
        }

        /// <summary>
        /// Stop the conversion process while it's running
        /// </summary>
        public void Stop()
        {
            IsStopped = true;
        }

        /// <summary>
        /// Start the conversion of all VTF
        /// </summary>
        /// <exception cref="ErrorConversionVTFException">If an unexpected error happens while processing a file and if the user selected the 'Stop Conversion on Error' option</exception>
        /// <exception cref="CancelConversionVTFException">If the user cancel the process while it's running</exception>
        /// <returns></returns>
        public void Start()
        {
            VTFProgressBar.Reset();
            // If Verbose, the process will log every converted file
            ConvertProcess convert = Verbose ? new ConvertProcess(ConvertAndLog) : new ConvertProcess(ConvertNoLog);

            int vtfCount = VtfFilesList.Count;
            int i = 0;
            while (!IsStopped && i < vtfCount)
            {
                string vtfFile = VtfFilesList[i];
                try
                {
                    convert(vtfFile);
                }
                catch (WrongVTFIdException ve)
                {
                    Log.AppendLine(ve.Message);
                }
                catch (Exception e)
                {
                    Log.AppendLine($"Error while processing {vtfFile}\n\t{e.Message}");
                    if (StopOnError)
                    {
                        Log.AppendLine($"Aborting the conversion\n");
                        Log.AppendLine($"Total files processed: {i}/{vtfCount}");
                        throw new ErrorConversionVTFException();
                    }
                }
                i++;
                VTFProgressBar.CurrentProgress = (int)(100f / vtfCount * i);
            }
            if (IsStopped)
            {
                Log.AppendLine("The conversion was cancelled by the user\n");
                Log.AppendLine($"Total files processed: {i}/{vtfCount}");
                throw new CancelConversionVTFException();
            }
            Log.AppendLine();
            Log.AppendLine($"Total files processed: {i}/{vtfCount}");
        }

        /// <summary>
        /// Function used with delegate, to convert a given VTF and log it
        /// </summary>
        /// <param name="vtfFile"></param>
        private void ConvertAndLog(string vtfFile)
        {
            if (ConvertVtfFile.ConvertVTFVersion(vtfFile))
            {
                Log.AppendLine($"Converted {vtfFile}");
            }
            else
            {
                Log.AppendLine($"{vtfFile} is not 7.5");
            }
        }

        /// <summary>
        /// Function used with delegate, to only convert a given VTF
        /// </summary>
        /// <param name="vtfFile"></param>
        private void ConvertNoLog(string vtfFile)
        {
            _ = ConvertVtfFile.ConvertVTFVersion(vtfFile);
        }

    }

    /// <summary>
    /// Exception used when an unexpected error happened during the VTF conversion
    /// </summary>
    [Serializable]
    internal class ErrorConversionVTFException : Exception
    {
        public ErrorConversionVTFException() { }

        public ErrorConversionVTFException(string str) : base(str) { }
    }

    /// <summary>
    /// Exception used when the user cancel the VTF conversion
    /// </summary>
    [Serializable]
    internal class CancelConversionVTFException : Exception
    {
        public CancelConversionVTFException() { }

        public CancelConversionVTFException(string str) : base(str) { }
    }




}
