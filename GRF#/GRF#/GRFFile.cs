using System;
using System.IO;
using System.Text;
using Ionic.Zlib;

namespace GRFSharp
{
    public class GRFFile
    {

        #region local variables

        private string _filename;
        private int _compressedLength;
        private int _comressedLengthAligned;
        private int _uncompressedLength;
        private byte _flags;
        private int _offset;
        private int _cycle;
        private GRF _ownerGRF;
        private byte[] _uncompressedBody;

        //private FileInfo _fileInfo;
        //private FileStream _fileStream;
        #endregion

        #region public properties
        public string Name { get { return _filename; } }
        public string Extension { get { return new System.IO.FileInfo(_filename).Extension; } }
        public int CompressedLength { get { return _compressedLength; } }
        public int UncompressedLength { get { return _uncompressedLength; } }
        public int CompressedLengthAligned { get { return _comressedLengthAligned; } }
        public byte Flags { get { return _flags; } }
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
            byte flags,
            int offset,
            int cycle,
            GRF ownerGRF) // Constructor
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
            string filePath = folderPath + this.Name;
            byte[] thisData = this.Data;

            //if (!Directory.Exists(dirPath))
            //    Directory.CreateDirectory(dirPath);

            //BinaryWriter b = new BinaryWriter(File.OpenWrite(filePath));
            //b.Write(thisData);
            //s.Close();

            //if (!Directory.Exists(dirPath))
            //    Directory.CreateDirectory(dirPath);

            //Stream s =File.OpenWrite(filePath);
            //s.Write(thisData, 0, thisData.Length);
            //s.Close();

            //@Todo Improve writing algorithm to make it faster
            //BinaryWriter bw;
            //FileInfo _fileInfo = new FileInfo(filePath);

            //if (!_fileInfo.Exists)
            //{
            //    bw = new BinaryWriter(_fileInfo.Create());

            //}
            //else
            //{
            //    bw = new BinaryWriter(_fileInfo.Open(FileMode.Open));
            //}
            //bw.Write(thisData);
            //bw.Close();

            FileInfo _fileInfo = new FileInfo(filePath);
            FileStream _fileStream;

            if (!_fileInfo.Directory.Exists)
                _fileInfo.Directory.Create();

            if (!_fileInfo.Exists)
                _fileStream = _fileInfo.Create();
            else
                _fileStream = _fileInfo.Open(FileMode.Open);

            _fileStream.BeginWrite(thisData, 0, thisData.Length, (IAsyncResult ar) =>
            {
                _fileStream.Close();
            },null);
            //_fileStream.Write(thisData, 0, thisData.Length);
            //_fileStream.Close();
        }

        

        /// <summary>
        /// Write the file entry data on an steam.
        /// This also prepare the compressed buffer to be writen.
        /// </summary>
        /// <param name="bw">Stream to write the file entry.</param>
        public void Save(BinaryWriter bw)
        {
            byte[] name = Encoding.Default.GetBytes(_filename);
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
            if ((_flags & 1) != 0 && _uncompressedBody != null)
            {
                _offset = (int)bw.BaseStream.Position - 46;
                byte[] compressedBody = ZlibStream.CompressBuffer(_uncompressedBody);
                bw.Write(compressedBody, 0, compressedBody.Length);
                _uncompressedLength = _uncompressedBody.Length;
                _compressedLength = compressedBody.Length;
                _comressedLengthAligned = _compressedLength + (4 - ((_compressedLength - 1) % 4)) - 1;
                _flags = 1;
                _cycle = 0;
            }
            else
            {
                byte[] data = _ownerGRF.GetCompressedDataFromFile(this);
                _offset = (int)bw.BaseStream.Position - 46;
                bw.Write(data, 0, data.Length);
            }
        }
        #endregion
    }
}

