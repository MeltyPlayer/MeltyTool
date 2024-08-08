using System;
#if CONTRACTS_FULL_SHIM

#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	/// <summary>Stack friendly bit encoder for dealing with handle generation or reading</summary>
	/// <remarks>Bits are written from the LSB to the MSB</remarks>
	[System.Diagnostics.DebuggerDisplay("Bits = {m64}, BitIndex = {mBitIndex}")]
	public partial struct HandleBitEncoder
		: IEquatable<HandleBitEncoder>
	{
		IntegerUnion mBits;
		int mBitIndex;

	/// <summary>How many bits have actually been consumed by the handle data</summary>
	public int UsedBitCount => this.mBitIndex;

		/// <summary>Get the entire handle's value represented in 32-bits</summary>
		/// <returns></returns>
		public uint GetCombinedHandle()
		{
			uint hi = Bits.GetHighBits(this.mBits.u64);

			// this order allows a user to XOR again with GetHandle32 to get
			// the upper 32-bit values of m64
			return hi ^ this.mBits.u32;
		}

	/// <summary>Clear the internal state of the encoder</summary>
	public void Reset()
		{
			this.mBits = new IntegerUnion();
			this.mBitIndex = 0;
		}

		#region Overrides
		public override bool Equals(object obj)
		{
			if (obj is HandleBitEncoder o)
			{
				return this.Equals(o);
			}

			return false;
		}
		public bool Equals(HandleBitEncoder other) => this.mBitIndex == other.mBitIndex &&
		                                              this.mBits.u64 == other.mBits.u64;
		public static bool operator ==(HandleBitEncoder x, HandleBitEncoder y) => x.Equals(y);
		public static bool operator !=(HandleBitEncoder x, HandleBitEncoder y) => !x.Equals(y);

		public override int GetHashCode() => (int) this.GetCombinedHandle();

		/// <summary>"[{<see cref="GetHandle64()"/>} @ {CurrentBitIndex}]</summary>
		/// <returns></returns>
		/// <remarks>Handle value is formatted to a 16-character hex string</remarks>
		public override string ToString() =>
			string.Format(Util.InvariantCultureInfo,
				"[{0} @ {1}]",
				this.mBits.u64.ToString("X16", Util.InvariantCultureInfo),
				this.mBitIndex.ToString(Util.InvariantCultureInfo));
		#endregion
	};
}
