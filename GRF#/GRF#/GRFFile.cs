using System;
using System.IO;

namespace SAIB.SharpGRF
{
    public class GRFFile
    {

        #region local variables

        private string _filename;
        private int _compressedLength;
        private int _comressedLengthAligned;
        private int _uncompressedLength;
        private char _flags;
        private int _offset;
        private int _cycle;
        private SharpGRF _ownerGRF;
        #endregion

        #region public properties
        public string Name { get { return _filename; } }
        public int CompressedLength { get { return _compressedLength; } }
        public int UncompressedLength { get { return _uncompressedLength; } }
        public int CompressedLengthAligned { get { return _compressedLength; } }
        public char Flags { get { return _flags; } }
        public int Offset { get { return _offset; } }
        public int Cycle { get { return _cycle; } }
        public byte[] Data { get { return _ownerGRF.GetDataFromFile(this); } }
        #endregion

        #region constructor
        public GRFFile(string fileName,
            int compressedLength,
            int compressedLengthAligned,
            int uncompressedLength,
            char flags,
            int offset,
            int cycle,
            SharpGRF ownerGRF) // Constructor
        {
            _filename = fileName;
            _compressedLength = compressedLength;
            _comressedLengthAligned = compressedLengthAligned;
            _uncompressedLength = uncompressedLength;
            _flags = flags;
            _offset = offset;
            _cycle = cycle;
            _ownerGRF = ownerGRF;
        }
        #endregion

        #region public functions
        /// <summary>
        /// Writes this file to the disk
        /// </summary>
        /// <param name='path'>
        /// The folder path where to store the file
        /// </param>
        public void WriteToDisk(string folderPath)
        {
            FileInfo thisFileInfo = new FileInfo(folderPath + this.Name.Replace("\\", "//"));

            FileStream fileStream;
            if (!thisFileInfo.Directory.Exists)
                thisFileInfo.Directory.Create();

            if (!thisFileInfo.Exists)
            {
                fileStream = thisFileInfo.Create();

            }
            else
            {
                fileStream = thisFileInfo.Open(FileMode.Open);

            }

            fileStream.Write(this.Data, 0, this.Data.Length);
            fileStream.Close();


        }
        #endregion
    }
}

