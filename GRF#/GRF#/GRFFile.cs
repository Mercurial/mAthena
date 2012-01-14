using System;
using System.IO;
using System.Text;
using Ionic.Zlib;

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
        private byte[] _uncompressedBody;
        #endregion

        #region public properties
        public string Name { get { return _filename; } }
        public string Extension { get { return new System.IO.FileInfo(_filename).Extension; } }
        public int CompressedLength { get { return _compressedLength; } }
        public int UncompressedLength { get { return _uncompressedLength; } }
        public int CompressedLengthAligned { get { return _compressedLength; } }
        public char Flags { get { return _flags; } }
        public int Offset { get { return _offset; } }
        public int Cycle { get { return _cycle; } }
        public byte[] Data { get { return _ownerGRF.GetDataFromFile(this); } }
        public byte[] UncompressedBody { get { return _uncompressedBody; } set { _uncompressedBody = value; } }
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
            try
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
            catch (Exception ex)
            {

            }
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

        /// <summary>
        /// Write the file entry data on an steam.
        /// This also prepare the compressed buffer to be writed.
        /// </summary>
        /// <param name="bw">Stream to write the file entry.</param>
        public void Save(BinaryWriter bw)
        {
            byte[] name = Encoding.GetEncoding(949).GetBytes(_filename);
            bw.Write(name, 0, name.Length);
            bw.Write((byte)0);
            bw.Write((int)_compressedLength);
            bw.Write((int)_comressedLengthAligned);
            bw.Write((int)_uncompressedLength);
            bw.Write((byte)_flags);
            bw.Write((int)_offset);
        }

        public void SaveBody(BinaryWriter bw)
        {
            bw.Flush();
            _offset = (int)bw.BaseStream.Position - 46;

            if (_uncompressedBody != null)
            {
                byte[] compressedBody = ZlibStream.CompressBuffer(_uncompressedBody);

                bw.Write(compressedBody, 0, compressedBody.Length);

                _uncompressedLength = _uncompressedBody.Length;
                _compressedLength = compressedBody.Length;
                _comressedLengthAligned = _compressedLength + (4 - ((_compressedLength - 1) % 4)) - 1;
                _flags = (char)0;
                _cycle = 0;
            }
            else
            {
                // Store here to don't read twice
                byte[] data = _ownerGRF.GetOriginalDataFromFile(this);

                bw.Write(data, 0, data.Length);
            }
        }
    }
}

