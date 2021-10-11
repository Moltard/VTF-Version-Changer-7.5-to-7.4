using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTFversionChanger.Models;

namespace VTFversionChanger.Tool
{
    internal static class ConvertVtfFile
    {

        /// <summary>
        /// Convert a given vtf file from 7.5 to 7.4<br></br>
        /// Original function by antim0118
        /// </summary>
        /// <param name="vtfFile">VTF file to convert</param>
        /// <exception cref="WrongVTFIdException">If the VTF file id was unexpected</exception>
        /// <returns><c>true</c> if the VTF was converted and <c>false</c> if the VTF was not converted (vtf version is not 7.5).<br/></returns>
        public static bool ConvertVTFVersion(string vtfFile)
        {
            using (FileStream FS = File.OpenRead(vtfFile))
            using (BinaryReader BR = new BinaryReader(FS))
            {
                int id = BR.ReadInt32();
                if (id != 0x465456)
                {
                    throw new WrongVTFIdException($"Warning - File signature doesn't match 'VTF': {vtfFile}");
                }
                int majorVersion = BR.ReadInt32(); // == 7 anyway
                int minorVersion = BR.ReadInt32();
                if (minorVersion != 5)
                {
                    return false; // Skip vtfs that are not 7.5
                }
            }
            using (FileStream FS = File.OpenWrite(vtfFile))
            using (BinaryWriter BW = new BinaryWriter(FS))
            {
                _ = BW.Seek(8, SeekOrigin.Begin); // skip id and major version
                BW.Write(4); // write minor version to 4
            }
            return true;
        }

    }

    /// <summary>
    /// Exception used when a VTF id is not what is expected (0x465456)
    /// </summary>
    [Serializable]
    internal class WrongVTFIdException : Exception
    {
        public WrongVTFIdException() { }

        public WrongVTFIdException(string str) : base(str) { }

    }


}
