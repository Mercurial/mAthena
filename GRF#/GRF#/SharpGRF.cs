using System;
using System.IO;
using Ionic.Zlib;
using System.Collections.Generic;
using System.Text;

namespace SAIB.SharpGRF
{


    public class SharpGRF
    {

        #region local variables
        private string _filePathToGRF;
        private List<GRFFile> _GRFFiles = new List<GRFFile>();

        private int _compressedLength;
        private int _uncompressedLength;

        private byte[] _bodyBytes;

        private int _fileCount = 0;

        const int sizeOfUint = sizeof(uint);
        const int sizeOfInt = sizeof(int);
        const int sizeOfChar = sizeof(char);

        private string _signature;
        private string _encryptionKey;
        private int _fileTableOffset;
        private int _version;
        private int _m1;
        private int _m2;
        private bool _isOpen = false;

        Stream _grfStream;

        #endregion

        #region public properties
        public List<GRFFile> Files { get { return _GRFFiles; } }
        public int FileCount { get { return _fileCount; } }
        public bool IsOpen { get { return _isOpen; } }
        public int Version
        {
            get
            {
                return this._version;
            }
        }

        public int FileTableOffset
        {
            get
            {
                return this._fileTableOffset;
            }
        }

        public string Signature
        {
            get
            {
                return this._signature;
            }
        }


        public string EncryptionKey
        {
            get
            {
                return this._encryptionKey;
            }
        }

        public int M2
        {
            get
            {
                return this._m2;
            }
        }

        public int M1
        {
            get
            {
                return this._m1;
            }
        }
        #endregion


        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SAIB.SharpGRF.SharpGRF"/> class.
        /// </summary>
        public SharpGRF() // Constructor
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SAIB.SharpGRF.SharpGRF"/> class.
        /// </summary>
        /// <param name='filePathToGRF'>
        /// File path to the GRF file.
        /// </param>
        public SharpGRF(string filePathToGRF) // Constructor
        {
            _filePathToGRF = filePathToGRF;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Open the GRF File to start reading.
        /// </summary>
        public void Open()
        {
            string signature, encryptionKey;
            int tableOffset, version, m1, m2;
            _GRFFiles.Clear();
            _grfStream = new FileStream(_filePathToGRF, FileMode.Open);
            //Read GRF File Header -> Signature
            byte[] signatureByte = new byte[15];
            _grfStream.Read(signatureByte, 0, 15);
            signature = System.Text.Encoding.ASCII.GetString(signatureByte);

            // Read GRF File Header -> Encryption Key
            byte[] allowencryptionBytes = new byte[15];
            _grfStream.Read(allowencryptionBytes, 0, 15);
            encryptionKey = System.Text.Encoding.ASCII.GetString(allowencryptionBytes);


            byte[] tableOffsetBytes = new byte[sizeOfUint];
            _grfStream.Read(tableOffsetBytes, 0, sizeOfUint);
            tableOffset = BitConverter.ToInt32(tableOffsetBytes, 0);

            byte[] skipBytes = new byte[sizeOfUint];
            _grfStream.Read(skipBytes, 0, sizeOfUint);
            m1 = BitConverter.ToInt32(skipBytes, 0);

            byte[] fileCountBytes = new byte[sizeOfUint];
            _grfStream.Read(fileCountBytes, 0, sizeOfUint);
            m2 = BitConverter.ToInt32(fileCountBytes, 0);

            byte[] versionBytes = new byte[sizeOfUint];
            _grfStream.Read(versionBytes, 0, sizeOfUint);
            version = BitConverter.ToInt32(versionBytes, 0);

            this._signature = signature;
            this._encryptionKey = encryptionKey;
            this._fileTableOffset = tableOffset;
            this._m1 = m1;
            this._m2 = m2;
            this._version = version;

            _grfStream.Seek(_fileTableOffset, SeekOrigin.Current);

            byte[] compressedLengthBytes = new byte[sizeOfUint];
            _grfStream.Read(compressedLengthBytes, 0, sizeOfUint);
            _compressedLength = BitConverter.ToInt32(compressedLengthBytes, 0);

            byte[] uncompressedLengthBytes = new byte[sizeOfUint];
            _grfStream.Read(uncompressedLengthBytes, 0, sizeOfUint);
            _uncompressedLength = BitConverter.ToInt32(uncompressedLengthBytes, 0);

            byte[] compressedBodyBytes = new byte[_compressedLength];
            _grfStream.Read(compressedBodyBytes, 0, _compressedLength);

            _bodyBytes = ZlibStream.UncompressBuffer(compressedBodyBytes);

            _fileCount = m2 - m1 - 7;
            int offset = 0;

            for (int x = 0; x < _fileCount; x++)
            {
                string fileName = string.Empty;
                char currentChar;


                while ((currentChar = (char)_bodyBytes[offset++]) != 0)
                    fileName += currentChar.ToString();

                int fileCompressedLength = 0,
                    fileCompressedLengthAligned = 0,
                    fileUncompressedLength = 0,
                    fileOffset = 0,
                    fileCycle = 0;

                char fileFlags = (char)0;

                //@Todo algorithm needs improvement
                byte[] fileCompressedLengthBytes = new byte[sizeOfInt];
                Array.Copy(_bodyBytes, offset, fileCompressedLengthBytes, 0, sizeOfInt);
                fileCompressedLength = BitConverter.ToInt32(fileCompressedLengthBytes, 0);
                offset += sizeOfInt;

                byte[] fileCompressedLengthAlignedBytes = new byte[sizeOfInt];
                Array.Copy(_bodyBytes, offset, fileCompressedLengthAlignedBytes, 0, sizeOfInt);
                fileCompressedLengthAligned = BitConverter.ToInt32(fileCompressedLengthAlignedBytes, 0);
                offset += sizeOfInt;

                byte[] fileUnCompressedLengthCharBits = new byte[sizeOfInt];
                Array.Copy(_bodyBytes, offset, fileUnCompressedLengthCharBits, 0, sizeOfInt);
                fileUncompressedLength = BitConverter.ToInt32(fileUnCompressedLengthCharBits, 0);
                offset += sizeOfInt;

                byte[] fileFlagsBytes = new byte[1];
                Array.Copy(_bodyBytes, offset, fileFlagsBytes, 0, 1);
                fileFlags = (char)fileFlagsBytes[0];
                offset++;

                byte[] fileOffsetBytes = new byte[sizeOfInt];
                Array.Copy(_bodyBytes, offset, fileOffsetBytes, 0, sizeOfInt);
                fileOffset = BitConverter.ToInt32(fileOffsetBytes, 0);
                offset += sizeOfInt;


                if (fileFlags == 3)
                {
                    int lop, srccount, srclen = fileCompressedLength;

                    for (lop = 10, srccount = 1; srclen >= lop; lop *= 10, srccount++)
                        fileCycle = srccount;
                }


                GRFFile newGRFFile = new GRFFile(
                    System.Text.Encoding.GetEncoding("EUC-KR").GetString(System.Text.Encoding.Default.GetBytes(fileName)),
                    fileCompressedLength,
                    fileCompressedLengthAligned,
                    fileUncompressedLength,
                    fileFlags,
                    fileOffset,
                    fileCycle,
                    this);

                _GRFFiles.Add(newGRFFile);

            }
            _isOpen = true;
        }

        /// <summary>
        ///  Open the GRF File to start reading. (Overload)
        /// </summary>
        /// <param name='filePath'>
        /// Path the the grf file to be opened
        /// </param>
        public void Open(string filePath)
        {
            _filePathToGRF = filePath;
            Open();
        }

        /// <summary>
        /// Closes the grf so it can be used again
        /// </summary>
        public void Close()
        {
            _grfStream.Close();
            _isOpen = false;
        }

        /// <summary>
        /// Gets the data of the file in the grf.
        /// </summary>
        /// <returns>
        /// byte[] the data in bytes
        /// </returns>
        /// <param name='file'>
        /// (GRFFile) The file to get
        /// </param>
        public byte[] GetDataFromFile(GRFFile file)
        {
            byte[] compressedBody = new byte[file.CompressedLength];

            _grfStream.Seek(46 + file.Offset, SeekOrigin.Begin);
            _grfStream.Read(compressedBody, 0, file.CompressedLengthAligned);

            if ((file.Flags == 3) || (file.Flags == 5))
            {
                compressedBody = DES.Decode(compressedBody, file.CompressedLengthAligned, file.Cycle);
            }

            return ZlibStream.UncompressBuffer(compressedBody);
        }
        #endregion
    }
}

