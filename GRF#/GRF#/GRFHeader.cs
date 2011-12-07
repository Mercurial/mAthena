using System;

namespace SAIB.SharpGRF
{
	/// <summary>
	/// GRF header.
	/// </summary>
	public class GRFHeader
	{
		private string _signature;
		private string _encryptionKey;
		private int _fileTableOffset;
		private int _version;
		private int _m1;
		private int _m2;
		
		public int Version {
			get {
				return this._version;
			}
		}

		public int FileTableOffset {
			get {
				return this._fileTableOffset;
			}
		}

		public string Signature {
			get {
				return this._signature;
			}
		}
		

		public string EncryptionKey {
			get {
				return this._encryptionKey;
			}
		}

		public int M2 {
			get {
				return this._m2;
			}
		}

		public int M1 {
			get {
				return this._m1;
			}
		}
		
		public GRFHeader(string signature, string encryptionKey , int fileTableOffset , int skip, int count, int version)
		{
			_signature = signature;
			_encryptionKey = encryptionKey;
			_fileTableOffset = fileTableOffset;
			_m1 = skip;
			_m2 = count;
			_version = version;
		}
	}
}

