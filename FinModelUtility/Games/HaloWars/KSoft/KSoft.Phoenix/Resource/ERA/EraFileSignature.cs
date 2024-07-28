using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using SHA1CryptoServiceProvider = System.Security.Cryptography.SHA1CryptoServiceProvider;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	/*public*/ sealed class EraFileSignature
		: IO.IEndianStreamSerializable
	{
		const uint kSignature = 0x05ABDBD8;
		const uint kSignatureMarker = 0xAAC94350;
		const byte kDefaultSizeBit = 0x13;

		const int kNonSignatureBytesSize = sizeof(uint) + sizeof(byte) + sizeof(uint);

		const uint kSha1Salt = 0xA7F95F9C;

		public byte SizeBit = kDefaultSizeBit;
		public byte[] SignatureData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			int sig_data_length = reading || this.SignatureData == null
				? 0
				: this.SignatureData.Length;
			int size = reading
				? 0
				: kNonSignatureBytesSize + sig_data_length;

			s.StreamSignature(kSignature);
			s.Stream(ref size);
			if (size < kNonSignatureBytesSize)
				throw new System.IO.InvalidDataException(size.ToString("X8"));
			s.Pad64();

			s.StreamSignature(kSignatureMarker);
			s.Stream(ref this.SizeBit);
			if (reading)
			{
				Array.Resize(ref this.SignatureData, size - kNonSignatureBytesSize);
				sig_data_length = this.SignatureData.Length;
			}
			if (sig_data_length > 0)
			{
				s.Stream(this.SignatureData);
			}
			s.StreamSignature(kSignatureMarker);
		}
		#endregion

		internal static byte[] ComputeSignatureDigest(System.IO.Stream chunksStream
			, long chunksOffset
			, long chunksLength
			, ECF.EcfHeader header)
		{
			Contract.Requires(chunksStream != null);
			Contract.Requires(chunksStream.CanSeek && chunksStream.CanRead);
			Contract.Requires(chunksOffset >= 0);
			Contract.Requires(chunksLength > 0);

			using (var sha = new SHA1CryptoServiceProvider())
			{
				PhxHash.UInt32(sha, kSha1Salt);
				PhxHash.UInt32(sha, (uint)header.HeaderSize);
				PhxHash.UInt32(sha, (uint)header.ChunkCount);
				PhxHash.UInt32(sha, (uint)header.ExtraDataSize);
				PhxHash.UInt32(sha, (uint)header.TotalSize);

				PhxHash.Stream(sha,
					chunksStream, chunksOffset, chunksLength,
					isFinal: true);

				return sha.Hash;
			}
		}
	};
}