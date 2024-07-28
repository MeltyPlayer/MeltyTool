
namespace KSoft.Phoenix
{
	static class SingleFixedPoint
	{
	const double kScaleToSingleMultiplier = 0.0001;//System.Math.Pow(10, -kExponent);
		const double kScaleFromSingleMultiplier = 10000;//System.Math.Pow(10, kExponent);

		const double kMax =   kScaleToSingleMultiplier * (double) Bitwise.Int24.MaxValue;
		const double kMin = -(kScaleToSingleMultiplier * (double)(Bitwise.Int24.MaxValue - 1));

		public static bool InRange(float value)
		{
			return value >= kMin && value <= kMax;
		}

		public static float ToSingle(uint value)
		{
			bool is_signed = Bitwise.Int24.IsSigned(value);
			value = Bitwise.Int24.GetNumber(value);

			float single = (float)(kScaleToSingleMultiplier * value);
			return is_signed ? -single : single;
		}
		public static uint FromSingle(float single)
		{
			bool is_signed = single < 0.0F;
			double d = System.Math.Abs(single);

			uint data = (uint)(d * kScaleFromSingleMultiplier);

			return Bitwise.Int24.SetSigned(data, is_signed);
		}
	};
}