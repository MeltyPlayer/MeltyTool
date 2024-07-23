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
		public PhxSYSTEMTIME DateTime { get { return mDateTime; } }
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
			PhxHash.UInt64(sha, Id);
			PhxHash.Unicode(sha, Name, kNameStorage.FixedLength-1);
			PhxHash.Unicode(sha, Description, kDescStorage.FixedLength-1);
			PhxHash.Ascii(sha, Author, kAuthorStorage.FixedLength);
			DateTime.UpdateHash(sha);
			PhxHash.UInt64(sha, AuthorXuid);
			PhxHash.UInt32(sha, Bitwise.ByteSwap.SingleToUInt32(Length));
			PhxHash.UInt16(sha, (uint)SessionId);
			PhxHash.UInt32(sha, (uint)GameType);
			PhxHash.UInt64(sha, DataCryptKey);
			sha.TransformBlock(DataHash, 0, DataHash.Length, null, 0);
			PhxHash.UInt64(sha, DataSize, isFinal: true);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);
			s.Stream(ref Id);
			s.Stream(ref Name, kNameStorage);
			s.Stream(ref Description, kDescStorage);
			s.Stream(ref Author, kAuthorStorage);
			s.Stream(ref mDateTime);
			s.Stream(ref AuthorXuid);
			s.Stream(ref Length);
			s.Stream(ref SessionId);
			s.Stream(ref GameType);
			s.Stream(ref DataCryptKey);
			s.Stream(DataHash, 0, DataHash.Length);
			s.Stream(ref DataSize);
			s.Stream(Hash, 0, Hash.Length);
			s.Pad(kPaddingLength);
		}
		#endregion
	};
}
