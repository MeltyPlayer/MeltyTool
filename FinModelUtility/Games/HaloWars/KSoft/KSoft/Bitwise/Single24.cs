using System.Diagnostics.CodeAnalysis;

namespace KSoft.Bitwise
{
	[SuppressMessage("Microsoft.Design", "CA1823:AvoidUnusedPrivateFields")]
	public static class Single24
	{
		public const float MinValue = kMin;
		public const float MaxValue = kMax;

		#region Single bit definitions
		static class Single32
		{
			internal const int kMantissaBitIndex = 0;
			internal const int kMantissaBitCount = 23;
			// 0x007FFFFF, (1 << 23) - 1
			internal const uint kMantissaBitMask = (1U<<kMantissaBitCount)-1;

			internal const int kExponentBitIndex = kMantissaBitIndex + kMantissaBitCount;
			internal const int kExponentBitCount = 8;
			// 0x7F800000, ((1 << 8) - 1) << 23
			internal const uint kExponentBitMask = ((1U<<kExponentBitCount)-1) << kExponentBitIndex;

			internal const int kSignBitIndex = kExponentBitIndex + kExponentBitCount;
			internal const int kSignBitCount = 1;
			// 0x80000000, 1 << 31
			internal const uint kSignBitMask = 1U << kSignBitIndex;

			internal const uint kSignBit = kSignBitMask;

			internal const int kExponentBias = (1 << (kExponentBitCount-1)) - 1;
		};
		#endregion

		#region Single24 bit definitions
		const int kMantissaBitIndex = 0;
		const int kMantissaBitCount = 17;
		// 0x0001FFFF, (1 << 17) - 1
		const uint kMantissaBitMask = (1U << kMantissaBitCount) - 1;

		const int kExponentBitIndex = kMantissaBitIndex + kMantissaBitCount;
		const int kExponentBitCount = 6;
		// 0x007E0000, ((1 << 6) - 1) << 17
		const uint kExponentBitMask = ((1U << kExponentBitCount) - 1) << kExponentBitIndex;

		const int kSignBitIndex = kExponentBitIndex + kExponentBitCount;

	// 0x00800000, 1 << 23
	const uint kSignBitMask = 1U << kSignBitIndex;

		const uint kSignBit = kSignBitMask;

		const int kExponentBias = (1 << (kExponentBitCount - 1)) - 1;
		const int kExponentBiasDiff = Single32.kExponentBias - kExponentBias;

		const int kMantissaBitDiff = Single32.kMantissaBitCount - kMantissaBitCount;
	#endregion

	#region Min\Max
	// min\max values for a signed single
	internal const uint kMinInt = 0xFFFFFF;
		internal const uint kMaxInt = 0x7FFFFF;
		const float kMin = -8.589902E+09F;
		const float kMax = 8.589902E+09F;
		#endregion

		public static bool InRange(float value)
		{
			return value >= kMin && value <= kMax;
		}

		public static uint FromSingle(float singleValue)
		{
			uint data = ByteSwap.SingleToUInt32(singleValue);
			uint mantissa = (data & Single32.kMantissaBitMask) >> Single32.kMantissaBitIndex;
			uint exponent = (data & Single32.kExponentBitMask) >> Single32.kExponentBitIndex;
			uint sign = (data & Single32.kSignBitMask) >> Single32.kSignBitIndex;
			uint v = 0;

			if (exponent == 0) v = kSignBit;
			else
			{
				sign = sign == 1 ? kSignBit : 0U;

				exponent -= kExponentBiasDiff;
				exponent <<= kExponentBitIndex;

				mantissa >>= kMantissaBitDiff;
				mantissa <<= kMantissaBitIndex;

				v = exponent | mantissa;
				v |= sign;
			}

			return v;
		}
		public static float ToSingle(uint data)
		{
			uint mantissa = (data & kMantissaBitMask) >> kMantissaBitIndex;
			uint exponent = (data & kExponentBitMask) >> kExponentBitIndex;
			uint sign = (data & kSignBitMask) >> kSignBitIndex;
			uint v = 0;

			if (exponent == 0) v = Single32.kSignBit;
			else
			{
				sign = sign == 1 ? Single32.kSignBit : 0;
				exponent += kExponentBiasDiff;
				exponent <<= kExponentBitIndex;

				v = exponent | mantissa;
				v <<= kMantissaBitDiff;
				v |= sign;
			}

			return ByteSwap.SingleFromUInt32(v);
		}
	};
}
