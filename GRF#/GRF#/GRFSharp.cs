using System;
using System.IO;
using Ionic.Zlib;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace GRFSharp
{
    #region Event Delegates
    public delegate void ExtractCompleteEventHandler(object sender, GRFFileExtractEventArg e);
    #endregion
    public class GRF
    {
        #region Local variables
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
        private byte[] _encryptionKey;
        private int _fileTableOffset;
        private int _version;
        private int _m1;
        private int _m2;
        private bool _isOpen = false;

        Stream _grfStream;

        #endregion

        #region Public Events
        public event ExtractCompleteEventHandler ExtractComplete;
        #endregion

        #region Protected Events
        protected virtual void OnExtractComplete(GRFFileExtractEventArg e)
        {
            if (ExtractComplete != null)
                ExtractComplete(this, e);
        }
        #endregion

        #region Public properties
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


        public byte[] EncryptionKey
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
        public GRF() // Constructor
        {
            _signature = "Master of Magic";
            _encryptionKey = new byte[14];
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SAIB.SharpGRF.SharpGRF"/> class.
        /// </summary>
        /// <param name='filePathToGRF'>
        /// File path to the GRF file.
        /// </param>
        public GRF(string filePathToGRF) // Constructor
        {
            _filePathToGRF = filePathToGRF;

            _signature = "Master of Magic";
            _encryptionKey = new byte[14];
        }
        #endregion

        #region Public Functions

        /// <summary>
        ///  Save the GRF file.
        /// </summary>
        public void Save()
        {
            // Write to temporary file
            string tempfile = Path.GetTempFileName();
            FileStream fs = new FileStream(tempfile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            byte[] signatureByte = new byte[Math.Max(_signature.Length, 15)];
            Encoding.ASCII.GetBytes(_signature).CopyTo(signatureByte, 0);
            bw.Write(signatureByte, 0, 15);
            bw.Write((byte)0);
            bw.Write(_encryptionKey, 0, 14);

            bw.Write((int)0); // will be updated later
            bw.Write((int)_m1);
            bw.Write((int)_GRFFiles.Count + _m1 + 7);
            bw.Write((int)0x200); // We always save as 2.0

            foreach (GRFFile file in _GRFFiles)
            {
                file.SaveBody(bw);
            }

            bw.Flush();

            int fileTablePos = (int)fs.Position;

            MemoryStream bodyStream = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(bodyStream);

            foreach (GRFFile file in _GRFFiles)
            {
                file.Save(bw2);
            }

            bw2.Flush();
            //byte[] compressedBody = new byte[_uncompressedLength + 100];
            //int size = compressedBody.Length;
            //ZLib.compress(compressedBody, ref size, bodyStream.GetBuffer(), (int)bodyStream.Length);
            byte[] compressedBody = ZlibStream.CompressBuffer(bodyStream.GetBuffer());

            bw.Write((int)compressedBody.Length);
            bw.Write((int)bodyStream.Length);
            bw.Write(compressedBody, 0, compressedBody.Length);
            bw2.Close();

            // Update file table offset
            bw.BaseStream.Seek(30, SeekOrigin.Begin);
            bw.Write((int)fileTablePos - 46);

            bw.Close();

            if (_grfStream != null)
                _grfStream.Close();

            File.Copy(tempfile, _filePathToGRF, true);

            Open();
        }

        public void SaveAs(string filepath)
        {
            // Write to temporary file
            string tempfile = Path.GetTempFileName();
            FileStream fs = new FileStream(tempfile, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            byte[] signatureByte = new byte[Math.Max(_signature.Length, 15)];
            Encoding.ASCII.GetBytes(_signature).CopyTo(signatureByte, 0);
            bw.Write(signatureByte, 0, 15);
            bw.Write((byte)0);
            bw.Write(_encryptionKey, 0, 14);

            bw.Write((int)0); // will be updated later
            bw.Write((int)_m1);
            bw.Write((int)_GRFFiles.Count + _m1 + 7);
            bw.Write((int)0x200); // We always save as 2.0

            foreach (GRFFile file in _GRFFiles)
            {
                file.SaveBody(bw);
            }

            bw.Flush();

            int fileTablePos = (int)fs.Position;

            MemoryStream bodyStream = new MemoryStream();
            BinaryWriter bw2 = new BinaryWriter(bodyStream);

            foreach (GRFFile file in _GRFFiles)
            {
                file.Save(bw2);
            }

            bw2.Flush();
            //byte[] compressedBody = new byte[_uncompressedLength + 100];
            //int size = compressedBody.Length;
            //ZLib.compress(compressedBody, ref size, bodyStream.GetBuffer(), (int)bodyStream.Length);
            byte[] compressedBody = ZlibStream.CompressBuffer(bodyStream.GetBuffer());

            bw.Write((int)compressedBody.Length);
            bw.Write((int)bodyStream.Length);
            bw.Write(compressedBody, 0, compressedBody.Length);
            bw2.Close();

            // Update file table offset
            bw.BaseStream.Seek(30, SeekOrigin.Begin);
            bw.Write((int)fileTablePos - 46);

            bw.Close();

            if (_grfStream != null)
                _grfStream.Close();

            File.Copy(tempfile, filepath, true);

            _filePathToGRF = filepath;
            Open();
        }

        /// <summary>
        /// Open the GRF File to start reading.
        /// </summary>
        public void Open()
        {
            string signature;
            int tableOffset, version, m1, m2;
            _GRFFiles.Clear();
            _grfStream = new FileStream(_filePathToGRF, FileMode.Open);
            BinaryReader br = new BinaryReader(_grfStream);

            //Read GRF File Header -> Signature
            byte[] signatureByte = new byte[15];
            _grfStream.Read(signatureByte, 0, 15);
            signature = System.Text.Encoding.ASCII.GetString(signatureByte);
            br.ReadByte();

            // Read GRF File Header -> Encryption Key
            byte[] encryptionKey = new byte[14];
            _grfStream.Read(encryptionKey, 0, 14);

            tableOffset = br.ReadInt32();
            m1 = br.ReadInt32();
            m2 = br.ReadInt32();
            version = br.ReadInt32();

            this._signature = signature;
            this._encryptionKey = encryptionKey;
            this._fileTableOffset = tableOffset;
            this._m1 = m1;
            this._m2 = m2;
            this._version = version;

            _grfStream.Seek(_fileTableOffset, SeekOrigin.Current);

            _compressedLength = br.ReadInt32();
            _uncompressedLength = br.ReadInt32();

            byte[] compressedBodyBytes = new byte[_compressedLength];
            _grfStream.Read(compressedBodyBytes, 0, _compressedLength);

            _bodyBytes = ZlibStream.UncompressBuffer(compressedBodyBytes);

            _fileCount = m2 - m1 - 7;

            MemoryStream bodyStream = new MemoryStream(_bodyBytes);
            BinaryReader bodyReader = new BinaryReader(bodyStream);

            for (int x = 0; x < _fileCount; x++)
            {
                string fileName = string.Empty;
                char currentChar;

                while ((currentChar = (char)bodyReader.ReadByte()) != 0)
                    fileName += currentChar;

                int fileCompressedLength = 0,
                    fileCompressedLengthAligned = 0,
                    fileUncompressedLength = 0,
                    fileOffset = 0,
                    fileCycle = 0;

                byte fileFlags = 0;

                fileCompressedLength = bodyReader.ReadInt32();
                fileCompressedLengthAligned = bodyReader.ReadInt32();
                fileUncompressedLength = bodyReader.ReadInt32();
                fileFlags = bodyReader.ReadByte();
                fileOffset = bodyReader.ReadInt32();

                if (fileFlags == 3)
                {
                    int lop, srccount, srclen = fileCompressedLength;

                    for (lop = 10, srccount = 1; srclen >= lop; lop *= 10, srccount++)
                        fileCycle = srccount;
                }

                if (fileFlags == 2) // Do not add folders 
                    return;

                GRFFile newGRFFile = new GRFFile(
                    System.Text.Encoding.Default.GetString(System.Text.Encoding.Default.GetBytes(fileName)),
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
                //@Todo fix DES.Decode()
                //compressedBody = DES.Decode(compressedBody, file.CompressedLengthAligned, file.Cycle);
                return new byte[file.CompressedLength];
            }

            return ZlibStream.UncompressBuffer(compressedBody);
        }

        /// <summary>
        /// Gets the byte[] data of the file in the grf. (Uncompressed)
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] GetCompressedDataFromFile(GRFFile file)
        {
            byte[] compressedBody = new byte[file.CompressedLength];
            _grfStream.Seek(46 + file.Offset, SeekOrigin.Begin);
            _grfStream.Read(compressedBody, 0, file.CompressedLengthAligned);
            return compressedBody;
        }


        /// <summary>
        /// Add a file inside the grf.
        /// </summary>
        /// <param name="filename">The name of the file to be added.</param>
        /// <param name="data">The data of the file to be added.</param>
        public void AddFile(string inputFilePath,string outputFilePath)
        {
            int i = 0;
            byte[] data = File.ReadAllBytes(inputFilePath);

            foreach (GRFFile file in _GRFFiles)
            {
                if (file.Name.ToLower() == outputFilePath.ToLower())
                {
                    _GRFFiles[i].UncompressedBody = data;
                    return;
                }

                i++;
            }

            GRFFile f = new GRFFile(outputFilePath, 0, 0, 0, 1, 0, 0, this);
            f.UncompressedBody = data;
            _GRFFiles.Add(f);
        }

        /// <summary>
        /// Delete a file in the grf.
        /// </summary>
        /// <param name="filename">The file name to delete.</param>
        public void DeleteFile(string filename)
        {
            foreach (GRFFile file in _GRFFiles)
            {
                if (file.Name.ToLower() == filename.ToLower())
                {
                    _GRFFiles.Remove(file);
                    return;
                }
            }
        }



        /// <summary>
        /// Extracts a file from the grf to the specified path.
        /// </summary>
        /// <param name="file">The file inside the grf</param>
        /// <param name="path">The path where to extract the file</param>
        public void ExtractFileToPath(GRFFile file,string path)
        {
            if (file.Flags == 1)
            {
                file.WriteToDisk(path);
                OnExtractComplete(new GRFFileExtractEventArg(file));
            }
        }
        #endregion

    }
}

