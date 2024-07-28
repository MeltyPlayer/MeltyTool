using System;
using System.IO;
using System.IO.Compression;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO.Compression
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
		"CA1062:ValidateArgumentsOfPublicMethods",
		Justification = "CodeContracts generally handle this already")]
	public static class ZLib
	{
		const int kSizeOfHeader = sizeof(ushort);

		static byte[] BufferFromStream(DeflateStream dec, int offset, int length, bool skipHeader)
		{
			byte[] result;

			// adjust for zlib header
			if (skipHeader)
				dec.BaseStream.Seek(offset + kSizeOfHeader, SeekOrigin.Begin);

			// decompress the data and fill in the result array
			result = new byte[length];
			dec.Read(result, 0, result.Length);

			return result;
		}

		public static byte[] BufferFromStream(MemoryStream ms,
			int offset = TypeExtensions.kNoneInt32, int length = TypeExtensions.kNoneInt32,
			bool skipHeader = true)
		{
			Contract.Requires<ArgumentNullException>(ms != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			if (offset.IsNone()) offset = 0;
			if (length.IsNone()) length = (int)ms.Length;

			using (var dec = new DeflateStream(ms, CompressionMode.Decompress, true))
			{
				return BufferFromStream(dec, offset, length, skipHeader);
			}
		}
		public static byte[] BufferFromBytes(byte[] bytes,
			int offset = TypeExtensions.kNoneInt32, int length = TypeExtensions.kNoneInt32,
			bool skipHeader = true)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			if (offset.IsNone()) offset = 0;
			if (length.IsNone()) length = bytes.Length;

			using (var ms = new MemoryStream(bytes))
			using (var dec = new DeflateStream(ms, CompressionMode.Decompress, false))
			{
				return BufferFromStream(dec, offset, length, skipHeader);
			}
		}


		public const int kNoCompression = ICSharpCode.SharpZipLib.Zip.Compression.Deflater.NO_COMPRESSION;
		public const int kBestCompression = ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION;

		public static byte[] LowLevelCompress(byte[] bytes, int level,
			out uint adler, byte[] compressedBytes,
			bool trimCompressedBytes = true, bool noZlibHeaderOrFooter = true)
		{
			int compressed_size;

			var zip = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(level, noZlibHeaderOrFooter);
			{
				zip.SetInput(bytes);
				zip.Finish();
				compressed_size = zip.Deflate(compressedBytes);
				adler = (uint)zip.Adler;

				if (trimCompressedBytes)
				{
					byte[] cmp_data = compressedBytes;
					Array.Resize(ref cmp_data, compressed_size);
					compressedBytes = cmp_data;
				}
			}

			return compressedBytes;
		}
		public static uint LowLevelDecompress(byte[] compressedBytes, byte[] uncompressedBytes,
			bool noHeader = true)
		{
			Contract.Requires<ArgumentNullException>(compressedBytes != null);
			Contract.Requires<ArgumentNullException>(uncompressedBytes != null);

			var zip = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater(noHeader);
			{
				zip.SetInput(compressedBytes);
				zip.Inflate(uncompressedBytes);
			}
			return (uint)zip.Adler;
		}

		public static byte[] LowLevelCompress(byte[] bytes, Shell.EndianFormat byteOrder)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] result = new byte[sizeof(int)];
			// Setup the decompressed size header
			byte[] size_bytes = BitConverter.GetBytes(bytes.Length);
			if (!byteOrder.IsSameAsRuntime())
				Bitwise.ByteSwap.SwapInt32(size_bytes, 0);
			Array.Copy(size_bytes, result, size_bytes.Length);

			var zip = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(
				ICSharpCode.SharpZipLib.Zip.Compression.Deflater.BEST_COMPRESSION, false);
			{
				zip.SetInput(bytes);
				zip.Finish();
				byte[] temp = new byte[bytes.Length];
				int compressed_size = zip.Deflate(temp);

				Contract.Assert(compressed_size <= bytes.Length);
				Array.Resize(ref result, sizeof(int) + compressed_size);
				Array.Copy(temp, 0, result, sizeof(int), compressed_size);
			}
			return result;
		}
		public static byte[] LowLevelDecompress(byte[] bytes, int uncompressedSize,
			int skipHeaderLength = sizeof(uint))
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Requires<ArgumentOutOfRangeException>(uncompressedSize >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(skipHeaderLength >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] result = new byte[uncompressedSize];
			var zip = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
			{
				zip.SetInput(bytes, skipHeaderLength, bytes.Length - skipHeaderLength); // skip the decompressed size header
				zip.Inflate(result);
			}
			return result;
		}
	};
}
