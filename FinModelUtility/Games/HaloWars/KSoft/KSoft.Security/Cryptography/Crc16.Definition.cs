#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public static partial class Crc16
	{
		public sealed class Definition
		{
			readonly ushort mPolynomial;
			readonly ushort[] mCrcTable;
			readonly ushort mInitialValue;
			readonly ushort mXorIn;
			readonly ushort mXorOut;

			public ushort Polynomial { get { return this.mPolynomial; } }
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
			public ushort[] CrcTable { get { return this.mCrcTable; } }
			public ushort InitialValue { get { return this.mInitialValue; } }
			public ushort XorIn { get { return this.mXorIn; } }
			public ushort XorOut { get { return this.mXorOut; } }

			static ushort[] BuildCrcTable(uint polynomial)
			{
				var crc_table = new ushort[kCrcTableSize];

				for (uint index = 0; index < crc_table.Length; index++)
				{
					uint crc = index << 8;
					for (uint j = 0; j < 8; j++)
					{
						if ((crc & 0x8000) != 0)
							crc = (crc << 1) ^ polynomial;
						else
							crc <<= 1;
					}
					crc_table[index] = (ushort)crc;
				}

				Contract.Assert(crc_table[1] != 0);
				Contract.Assert(crc_table[crc_table.Length-1] != 0);

				return crc_table;
			}

			public Definition(ushort polynomial = kDefaultPolynomial, ushort initialValue = ushort.MaxValue, ushort xorIn = 0, ushort xorOut = 0, params ushort[] crcTable)
			{
				Contract.Requires(crcTable.IsNullOrEmpty() || crcTable.Length == kCrcTableSize);

				this.mPolynomial = polynomial;
				this.mInitialValue = initialValue;
				this.mXorIn = xorIn;
				this.mXorOut = xorOut;

				this.mCrcTable = crcTable.IsNullOrEmpty()
					? BuildCrcTable(this.Polynomial)
					: crcTable;
			}

			public ushort ComputeUpdate(ushort crc, uint value)
			{
				value &= 0xFF;
				ushort a = (ushort) (crc << 8);
				ushort b = (ushort)((crc >> 8) & 0x00FFFFFF); // don't include the top most byte in case there was somehow any carry
				ushort c = this.CrcTable[(b ^ value) & 0xFF];
				return (ushort)(a ^ c);
			}

			public void ComputeUpdate(uint value, ref ushort crc)
			{
				crc = this.ComputeUpdate(crc, value);
			}

			internal ushort HashCore(ushort crc, byte[] array, int startIndex, int count)
			{
				for (int index = startIndex; count != 0; --count, ++index)
				{
					crc = this.ComputeUpdate(crc, array[index]);
				}

				return crc;
			}
			public ushort Crc(ref ushort crc, byte[] buffer, int size)
			{
				if (crc == 0)
					crc = this.InitialValue;

				crc ^= this.XorIn;

				crc = this.HashCore(crc, buffer, 0, size);

				crc ^= this.XorOut;

				return crc;
			}
		};
	};
}
