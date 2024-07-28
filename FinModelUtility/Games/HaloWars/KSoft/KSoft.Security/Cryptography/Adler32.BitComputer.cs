using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	partial class Adler32
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BitComputer
		{
			uint s1, s2;

			public BitComputer(uint adler32)
			{
				this.s1 = adler32 & 0xFFFF;
				this.s2 = adler32 >> 16;
			}

			public static BitComputer New { get { return new BitComputer(1); } }

			public uint ComputeFinish()
			{
				return Adler32.ComputeFinish(this.s1, this.s2);
			}

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset + length <= buffer.Length);

				int buflen = length;
				for (int blocklen; buflen > 0; buflen -= blocklen)
				{
					blocklen = buflen < kBlockMax
						? buflen
						: kBlockMax;

					int x;
					for (x = 0; x + 7 < blocklen; x += 8, offset += 8)
					{
						this.s1 += buffer[offset + 0];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 1];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 2];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 3];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 4];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 5];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 6];
						this.s2 += this.s1;
						this.s1 += buffer[offset + 7];
						this.s2 += this.s1;
					}

					for (; x < blocklen; x++, offset++)
					{
						this.s1 += buffer[offset];
						this.s2 += this.s1;
					}

					this.s1 %= kAdlerMod;
					this.s2 %= kAdlerMod;
				}
			}

			#region Compute 16-bits
			public void ComputeLE(ushort value)
			{
				ComputeUpdate((value & 0x00FFU) >> 0, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0xFF00U) >> 8, ref this.s1, ref this.s2);
			}
			public void ComputeBE(ushort value)
			{
				ComputeUpdate((value & 0xFF00U) >> 8, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x00FFU) >> 0, ref this.s1, ref this.s2);
			}
			#endregion

			#region Compute 32-bits
			public void ComputeLE(uint value)
			{
				ComputeUpdate((value & 0x000000FFU) >> 0, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0xFF000000U) >> 24, ref this.s1, ref this.s2);
			}
			public void ComputeBE(uint value)
			{
				ComputeUpdate((value & 0xFF000000U) >> 24, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.s1, ref this.s2);
				ComputeUpdate((value & 0x000000FFU) >> 0, ref this.s1, ref this.s2);
			}
			#endregion

			#region Compute 64-bits
			public void ComputeLE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = lo;
				this.ComputeLE(_value);

				_value = hi;
				this.ComputeLE(_value);
			}
			public void ComputeBE(ulong value)
			{
				uint lo = Bits.GetLowBits(value);
				uint hi = Bits.GetHighBits(value);
				uint _value;

				_value = hi;
				this.ComputeBE(_value);

				_value = lo;
				this.ComputeBE(_value);
			}
			#endregion
		};
	};
}
