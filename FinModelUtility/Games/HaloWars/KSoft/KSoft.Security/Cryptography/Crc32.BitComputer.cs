using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	partial class Crc32
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct BitComputer
		{
			Definition mDefinition;
			uint mCrc;

			public BitComputer(Definition definition)
			{
				Contract.Requires(definition != null);

				this.mDefinition = definition;
				this.mCrc = this.mDefinition.InitialValue;
			}
			public BitComputer(Definition definition, uint initialValue)
			{
				Contract.Requires(definition != null);

				this.mDefinition = definition;
				this.mCrc = initialValue;
			}

			public void ComputeBegin()
			{
				this.mCrc ^= this.mDefinition.XorIn;
			}

			public uint ComputeFinish()
			{
				this.mCrc ^= this.mDefinition.XorOut;
				return this.mCrc;
			}

			public void Compute(byte[] buffer, int offset, int length)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0 && length >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(offset+length <= buffer.Length);

				for (int x = 0; x < length; x++)
					this.mDefinition.ComputeUpdate(buffer[offset+x], ref this.mCrc);
			}

			public void Compute(byte value)
			{
				this.mDefinition.ComputeUpdate(value, ref this.mCrc);
			}

			#region Compute 16-bits
			public void ComputeLE(ushort value)
			{
				this.mDefinition.ComputeUpdate((value & 0x00FFU) >> 0, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0xFF00U) >> 8, ref this.mCrc);
			}
			public void ComputeBE(ushort value)
			{
				this.mDefinition.ComputeUpdate((value & 0xFF00U) >> 8, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x00FFU) >> 0, ref this.mCrc);
			}
			#endregion

			#region Compute 32-bits
			public void ComputeLE(uint value)
			{
				this.mDefinition.ComputeUpdate((value & 0x000000FFU) >> 0, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0xFF000000U) >> 24, ref this.mCrc);
			}
			public void ComputeBE(uint value)
			{
				this.mDefinition.ComputeUpdate((value & 0xFF000000U) >> 24, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x00FF0000U) >> 16, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x0000FF00U) >> 8, ref this.mCrc);
				this.mDefinition.ComputeUpdate((value & 0x000000FFU) >> 0, ref this.mCrc);
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
