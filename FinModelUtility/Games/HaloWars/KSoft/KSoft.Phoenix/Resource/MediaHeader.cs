using System.Security.Cryptography;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	public sealed class MediaHeader
		: IO.IEndianStreamSerializable
	{
		static readonly Memory.Strings.StringStorage kNameStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString,
			Shell.EndianFormat.Big, 32);
		static readonly Memory.Strings.StringStorage kDescStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Unicode, Memory.Strings.StringStorageType.CString,
			Shell.EndianFormat.Big, 128);
		static readonly Memory.Strings.StringStorage kAuthorStorage = new Memory.Strings.StringStorage(
			Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageType.CString, 16);

		const byte kVersion = 2;
		internal const int kSizeOf = 0x1C0;

		const int kPaddingLength = 13;

		public ulong Id;
		public string Name;
		public string Description;
		public string Author;
		#region DateTime
		PhxSYSTEMTIME mDateTime;
		public PhxSYSTEMTIME DateTime { get { return this.mDateTime; } }
		#endregion
		public ulong AuthorXuid;
		public float Length;
		public short SessionId;
		public int GameType;
		public ulong DataCryptKey;
		public byte[] DataHash = new byte[PhxHash.kSha1SizeOf];
		public ulong DataSize;

		public byte[] Hash { get; private set; } = new byte[PhxHash.kSha1SizeOf];

		public void UpdateHash(SHA1CryptoServiceProvider sha)
		{
			PhxHash.UInt8(sha, kVersion);
			PhxHash.UInt64(sha, this.Id);
			PhxHash.Unicode(sha, this.Name, kNameStorage.FixedLength-1);
			PhxHash.Unicode(sha, this.Description, kDescStorage.FixedLength-1);
			PhxHash.Ascii(sha, this.Author, kAuthorStorage.FixedLength);
			this.DateTime.UpdateHash(sha);
			PhxHash.UInt64(sha, this.AuthorXuid);
			PhxHash.UInt32(sha, Bitwise.ByteSwap.SingleToUInt32(this.Length));
			PhxHash.UInt16(sha, (uint) this.SessionId);
			PhxHash.UInt32(sha, (uint) this.GameType);
			PhxHash.UInt64(sha, this.DataCryptKey);
			sha.TransformBlock(this.DataHash, 0, this.DataHash.Length, null, 0);
			PhxHash.UInt64(sha, this.DataSize, isFinal: true);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);
			s.Stream(ref this.Id);
			s.Stream(ref this.Name, kNameStorage);
			s.Stream(ref this.Description, kDescStorage);
			s.Stream(ref this.Author, kAuthorStorage);
			s.Stream(ref this.mDateTime);
			s.Stream(ref this.AuthorXuid);
			s.Stream(ref this.Length);
			s.Stream(ref this.SessionId);
			s.Stream(ref this.GameType);
			s.Stream(ref this.DataCryptKey);
			s.Stream(this.DataHash, 0, this.DataHash.Length);
			s.Stream(ref this.DataSize);
			s.Stream(this.Hash, 0, this.Hash.Length);
			s.Pad(kPaddingLength);
		}
		#endregion
	};
}
