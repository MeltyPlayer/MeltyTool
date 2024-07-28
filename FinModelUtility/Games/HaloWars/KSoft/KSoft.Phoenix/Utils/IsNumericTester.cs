
namespace KSoft.Phoenix
{
	public struct IsNumericTester
	{
		public bool AllowExponential;
		public int StartOffset;

		public int ReturnFailOffset;
		public int ReturnIntegralDigits;
		public int ReturnFractionalDigits;
		public int ReturnSignificantDigits;

		private void InitializeReturns()
		{
			this.ReturnFailOffset = TypeExtensions.kNone;
			this.ReturnIntegralDigits = this.ReturnFractionalDigits = this.ReturnSignificantDigits = 0;
		}

		private enum Phase
		{
			// D is the Fortran-style exponent character
			// [whitespace] [sign] [digits] [.digits] [ {d | D | e | E}[sign]digits]
			//  0            1      2        34          4              5    6

			/** <summary>0</summary> */ Whitespace,
			/** <summary>1</summary> */ Sign,
			/** <summary>2</summary> */ Digits,
			/** <summary>3</summary> */ FractionalSign,
			/** <summary>4</summary> */ FractionalDigits,
			/** <summary>5</summary> */ ExponentSign,
			/** <summary>6</summary> */ ExponentDigits,
			/** <summary>7</summary> */ TrailingWhiteSpace,

			kNumberOf
		};

		public bool Test(string str)
		{
			this.InitializeReturns();

			if (str == null)
				return this.TestFailed(0);

			bool found_digits = false;

			bool found_first_non_zero_digit = false;
			var cur_phase = Phase.Whitespace;

			for (int cur_pos = this.StartOffset; cur_pos < str.Length; cur_pos++)
			{
				char c = str[cur_pos];

				bool c_is_digit = char.IsDigit(c);
				if (c_is_digit)
					found_digits = true;

				for (bool next_char = true; next_char; )
				{
					next_char = false;

					switch (cur_phase)
					{
						case Phase.Whitespace: {
							if (IsIgnoredWhitespace(c))
								next_char = true;
							else
								cur_phase = Phase.Sign;
						} break;

						case Phase.Sign: {
							if (IsDigitSign(c))
								next_char = true;

							cur_phase = Phase.Digits;
						} break;

						case Phase.Digits: {
							if (c_is_digit)
							{
								this.ReturnIntegralDigits++;

								if (c != '0')
									found_first_non_zero_digit = true;

								if (found_first_non_zero_digit)
									this.ReturnSignificantDigits++;

								next_char = true;
							}
							else
								cur_phase = Phase.FractionalSign;
						} break;

						case Phase.FractionalSign: {
							if(c == '.')
							{
								next_char = true;
								cur_phase = Phase.FractionalDigits;
							}
							else if (IsExponentCharacter(c))
							{
								if (!this.AllowExponential)
									return this.TestFailed(cur_pos);

								next_char = true;
								cur_phase = Phase.ExponentSign;
							}
							else
							{
								return this.TestFailed(cur_pos);
							}
						} break;

						case Phase.FractionalDigits: {
							if (c_is_digit)
							{
								this.ReturnFractionalDigits++;

								if (c != '0')
									found_first_non_zero_digit = true;

								if (found_first_non_zero_digit)
									this.ReturnSignificantDigits++;

								next_char = true;
							}
							else if (IsExponentCharacter(c))
							{
								if (!this.AllowExponential)
									return this.TestFailed(cur_pos);

								next_char = true;
								cur_phase = Phase.ExponentSign;
							}
							else if (IsIgnoredWhitespace(c))
							{
								next_char = true;
								cur_phase = Phase.TrailingWhiteSpace;
							}
							else
							{
								return this.TestFailed(cur_pos);
							}
						} break;

						case Phase.ExponentSign: {
							if (IsDigitSign(c))
								next_char = true;

							cur_phase = Phase.ExponentDigits;
						} break;

						case Phase.ExponentDigits: {
							if (c_is_digit)
							{
								next_char = true;
							}
							else if (IsIgnoredWhitespace(c))
							{
								next_char = true;
								cur_phase = Phase.TrailingWhiteSpace;
							}
							else
							{
								this.TestFailed(cur_pos);
							}
						} break;

						case Phase.TrailingWhiteSpace: {
							if (IsIgnoredWhitespace(c))
								next_char = true;
							else
								return this.TestFailed(cur_pos);
						} break;
					}
				}
			}

			return found_digits;
		}

		private bool TestFailed(int curPos)
		{
			this.ReturnFailOffset = curPos;
			return false;
		}

		private static bool IsIgnoredWhitespace(char c)
		{
			return c == ' ' || c == '\t';
		}
		private static bool IsDigitSign(char c)
		{
			return c == '-' || c == '+';
		}
		private static bool IsExponentCharacter(char c)
		{
			return
				c == 'e' || c == 'd' ||
				c == 'E' || c == 'D';
		}
	};
}