using System;
using System.IO;
using System.Security.Cryptography;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	using Debug = Phoenix.Debug;

	static class PhxHash
	{
		public const int kSha1SizeOf = 20;

		// NOTE: data is written to the buffer in MSB order
		static byte[] gUInt64Buffer = new byte[sizeof(ulong)];

		static void BufferFillUnicode(char unicode)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer, 0, (ushort)unicode);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt16(gUInt64Buffer, 0);
			}
		}

		public static void UInt8(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			gUInt64Buffer[0] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(byte));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
		}
		public static void UInt16(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer, 0, (ushort)word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt16(gUInt64Buffer, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(ushort));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
		}
		public static void UInt32(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer, 0, word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt32(gUInt64Buffer, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(uint));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(uint), null, 0);
		}
		public static void UInt64(SHA1CryptoServiceProvider sha, ulong word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer, 0, word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt64(gUInt64Buffer, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer, 0, sizeof(ulong));
			else
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ulong), null, 0);
		}

		public static void Ascii(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				gUInt64Buffer[0] = (byte)(str[x] >> 0);
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
			}

			gUInt64Buffer[0] = 0;
			for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
			{
				BufferFillUnicode('\0');
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(byte), null, 0);
			}
		}
		public static void Unicode(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
			}

			BufferFillUnicode('\0');
			for (int x = 0, null_count = fixedLength - str.Length; x < null_count; x++)
			{
				sha.TransformBlock(gUInt64Buffer, 0, sizeof(ushort), null, 0);
			}
		}

		public static void Stream(SHA1CryptoServiceProvider sha
			, Stream inputStream
			, long inputOffset
			, long inputLength
			, bool isFinal = false)
		{
			const int k_read_block_size = 4096;

			Contract.Requires(inputStream != null);
			Contract.Requires(inputStream.CanSeek && inputStream.CanRead);
			Contract.Requires(inputOffset >= 0);
			Contract.Requires(inputLength > 0);

			var scratch_buffer = new byte[k_read_block_size];

			using (new IO.StreamPositionContext(inputStream))
			{
				inputStream.Seek(inputOffset, SeekOrigin.Begin);

				for (long input_bytes_read = 0; input_bytes_read < inputLength; )
				{
					long bytes_remaining = inputLength - input_bytes_read;
					int read_block_length = System.Math.Min((int)bytes_remaining, scratch_buffer.Length);

					Array.Clear(scratch_buffer, 0, scratch_buffer.Length);
					for (int actual_bytes_read = 0; actual_bytes_read < read_block_length; )
					{
						int sub_block_offset = actual_bytes_read;
						int sub_block_length = read_block_length - sub_block_offset;
						actual_bytes_read += inputStream.Read(scratch_buffer, sub_block_offset, sub_block_length);
					}

					sha.TransformBlock(
						scratch_buffer, 0, read_block_length,
						null, 0);
					input_bytes_read += read_block_length;
				}
			}

			if (isFinal)
			{
				sha.TransformFinalBlock(scratch_buffer, 0, 0);
			}
		}

		public const int kResultSize = 0x18;

		public static void Sha1Hash(string str, byte[] result)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(str));
			Contract.Requires<ArgumentNullException>(result != null);
			Contract.Requires(result.Length >= kResultSize);

			Array.Clear(result, 0, result.Length);

			byte[] str_bytes = System.Text.Encoding.ASCII.GetBytes(str);

			using (var sha = new SHA1CryptoServiceProvider())
			{
				byte[] result1;
				byte[] result_final;

				UInt32(sha, 0xA4800C14);
				//PhxHash.Ascii(sha, str);
				sha.TransformBlock(str_bytes, 0, str_bytes.Length, null, 0);
				UInt32(sha, 0x5AF4A9F1);
				UInt32(sha, 0xCA6884EC, true);
				result1 = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Result: {1}", str, Text.Util.ByteArrayToString(result1));

				sha.Initialize();
				UInt32(sha, 0xCB92EAEB);
				sha.TransformBlock(result1, 0, result1.Length, null, 0);
				UInt32(sha, 0x1D919BF8, true);
				result_final = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Final: {1}", str, Text.Util.ByteArrayToString(result_final));

				Array.Copy(result_final, result, result_final.Length);
			}
		}

		public static bool Sha1HashFile(string fileName, byte[] result, out long fileLength)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));
			Contract.Requires<ArgumentNullException>(result != null);
			Contract.Requires(result.Length >= kResultSize);

			fileLength = -1;

			try
			{
				if (!File.Exists(fileName))
					return false;

				byte[] result_final;

				using (var fs = File.OpenRead(fileName))
				using (var sha = new SHA1CryptoServiceProvider())
				{
					result_final = sha.ComputeHash(fs, 0, fs.Length);

					fileLength = fs.Length;
				}

				Array.Copy(result_final, result, result_final.Length);
			}
			catch (Exception ex)
			{
				Debug.Trace.Security.TraceInformation(ex.ToString());
				return false;
			}

			return true;
		}

		public static TigerHashBase CreateHaloWarsTigerHash()
		{
			Contract.Ensures(Contract.Result<TigerHashBase>() != null);

			var tiger = TigerHashBase.Create(TigerHash.kAlgorithmName);

			return tiger;
		}
	};
}