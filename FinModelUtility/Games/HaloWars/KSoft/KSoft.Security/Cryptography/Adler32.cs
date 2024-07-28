using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public static partial class Adler32
	{
		// http://www.opensource.apple.com/source/xnu/xnu-1504.3.12/libkern/zlib/arm/adler32vec.s

		const uint kAdlerMod = 65521;
		const int kBlockMax = 5552;

		public static uint Compute(byte[] buffer, int offset, int length, uint adler32 = 1)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset+length <= buffer.Length);

			var computer = new BitComputer(adler32);
			computer.Compute(buffer, offset, length);
			adler32 = computer.ComputeFinish();
			return adler32;
		}
		public static uint Compute(byte[] buffer, uint adler32 = 1)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);

			return Compute(buffer, 0, buffer.Length, adler32);
		}

		public static uint Compute(System.IO.Stream stream, int length, uint adler32 = 1,
			bool restorePosition = false)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<InvalidOperationException>(stream.CanRead);
			Contract.Requires(!restorePosition || stream.CanSeek);

			long prev_position = restorePosition
				? stream.Position
				: -1;

			var computer = new BitComputer(adler32);

			int buffer_size = Math.Min(length, 1024);
			byte[] buffer = new byte[buffer_size];

			for (int bytes_remaining = length; bytes_remaining > 0; )
			{
				int num_bytes_to_read = Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = stream.Read(buffer, num_bytes_read, num_bytes_to_read);
					if (n == 0)
						break;

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
					computer.Compute(buffer, 0, num_bytes_read);
				else
					break;

				bytes_remaining -= num_bytes_read;
			}

			adler32 = computer.ComputeFinish();

			if (prev_position != -1)
				stream.Seek(prev_position, System.IO.SeekOrigin.Begin);

			return adler32;
		}


		static void ComputeUpdate(uint value, ref uint s1, ref uint s2)
		{
			s1 += value & 0xFFU;
			s2 += s1;
		}
		static uint ComputeFinish(uint s1, uint s2)
		{
			s1 %= kAdlerMod; s2 %= kAdlerMod;

			return ComputeResult(s1, s2);
		}
		static uint ComputeResult(uint s1, uint s2)
		{
			return (s2 << 16) | s1;
		}
		public static uint Compute(byte value, uint adler32 = 1)
		{
			uint s1 = adler32 & 0xFFFF, s2 = adler32 >> 16;

			ComputeUpdate(value, ref s1, ref s2);

			return ComputeFinish(s1, s2);
		}

		#region Compute 16-bits
		public static uint ComputeLE(ushort value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(ushort value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion

		#region Compute 32-bits
		public static uint ComputeLE(uint value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(uint value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion

		#region Compute 64-bits
		public static uint ComputeLE(ulong value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeLE(value);
			return bc.ComputeFinish();
		}
		public static uint ComputeBE(ulong value, uint adler32 = 1)
		{
			var bc = new BitComputer(adler32);
			bc.ComputeBE(value);
			return bc.ComputeFinish();
		}
		#endregion
	};
}