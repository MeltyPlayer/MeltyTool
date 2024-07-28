using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	using Adler32 = Security.Cryptography.Adler32;

	partial class CompressedStream
	{
		static class InMemoryRep
		{
			const int kOffsetSourceSize = 0;
			const int kOffsetSourceAdler = kOffsetSourceSize + sizeof(ulong);
			const int kOffsetCompressedSize = kOffsetSourceAdler + sizeof(uint);
			const int kOffsetCompressedAdler = kOffsetCompressedSize + sizeof(ulong);
			const int kOffsetMode = kOffsetCompressedAdler + sizeof(uint);

	  public static uint Checksum(ulong srcSize, uint srcAdler, ulong cmpSize, uint cmpAdler,
				uint mode = 0)
			{
				var bc = Adler32.BitComputer.New;

				bc.ComputeBE(srcSize);
				bc.ComputeBE(srcAdler);
				bc.ComputeBE(cmpSize);
				bc.ComputeBE(cmpAdler);
				bc.ComputeBE(mode);

				return bc.ComputeFinish();
			}
		};

		static readonly Header kBufferedHeader = new Header() {
			HeaderAdler32 = 0x00330004,
			StreamMode = (uint)Mode.Buffered,
			UncompressedSize = 0,	CompressedSize = 0,
			UncompressedAdler32 = 1,	CompressedAdler32 = 1,
		};

		public static void CompressFromStream(IO.EndianWriter blockStream, System.IO.Stream source,
			out uint streamAdler, out int streamSize)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Requires<ArgumentNullException>(source != null);

			using (var ms = new System.IO.MemoryStream((int)source.Length + Header.kSizeOf))
			using (var s = new IO.EndianStream(ms, Shell.EndianFormat.Big, permissions: FA.Write))
			using (var cs = new CompressedStream())
			{
				s.StreamMode = FA.Write;

				cs.InitializeFromStream(source);
				cs.Compress();

				cs.Serialize(s);

				ms.Position = 0;
				streamSize = (int)ms.Length;
				streamAdler = Adler32.Compute(ms, streamSize);

				ms.WriteTo(blockStream.BaseStream);
			}
		}
		public static byte[] DecompressFromStream(IO.EndianStream blockStream)
		{
			Contract.Requires<ArgumentNullException>(blockStream != null);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer;
			using (var cs = new CompressedStream())
			{
				cs.Serialize(blockStream);
				cs.Decompress();
				buffer = cs.UncompressedData;
			}

			return buffer;
		}
	};
}