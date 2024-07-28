using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	partial class BitwiseT4
	{
		#region Bit pattern constants
		/// <summary>0101 0101</summary>
		const byte kMaskEvenBits = 0x55;
		/// <summary>1010 1010</summary>
		const byte kMaskOddBits = unchecked((byte)~kMaskEvenBits);

		/// <summary>0011 0011</summary>
		const byte kMaskConsecutivePairsLsb = 0x33;
		/// <summary>1100 1100</summary>
		const byte kMaskConsecutivePairsMsb = unchecked((byte)~kMaskConsecutivePairsLsb);

		/// <summary>0000 1111</summary>
		const byte kMaskNibbleLsb = 0x0F;
		/// <summary>1111 0000</summary>
		const byte kMaskNibbleMsb = unchecked((byte)~kMaskNibbleLsb);
		#endregion

		public abstract class BitFondleCodeGenerator
			: BitUtilCodeGenerator
		{
			/// <summary>Name of the input parameter of the method we're generating code for</summary>
			protected const string kBitsParamName = "bits";
			/// <summary>Name of the local variable we're using to fondle the bits of the input parameter</summary>
			protected const string kFondleVarName = "x";

			protected BitFondleCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			/// <summary>Declares and initializes the variable (of the underlying number type) used for bit fondling</summary>
			protected override void GeneratePrologue()
			{
				this.File.Write(this.Def.SizeOfInBits == PrimitiveDefinitions.kUInt64.SizeOfInBits
					                ? PrimitiveDefinitions.kUInt64.Keyword
					                : PrimitiveDefinitions.kUInt32.Keyword);

				this.File.WriteLine(" {0} = {1};", kFondleVarName, kBitsParamName);
			}

			/// <summary>Given a bit pattern mask at the byte level, build the mask suitable for the underlying number type</summary>
			/// <param name="byteMask"></param>
			/// <returns>hexadecimal literal code string suitable for masking integers of the underlying number type</returns>
			protected string BuildBitMaskForInteger(byte byteMask)
			{
				var sb = new System.Text.StringBuilder("0x");

				var mask_str = byteMask.ToString("X2", UtilT4.InvariantCultureInfo);
				for (int x = 0; x < this.Def.SizeOfInBytes; x++)
					sb.Append(mask_str);

				return sb.ToString();
			}
			/// <summary>Build a mask suitable for extracting the consecutive bit halves (LHS or RHS) of the underlying number type</summary>
			/// <param name="wordSize">Size of word in bytes</param>
			/// <param name="lhs">Are we masking for the left-hand-side or right-hand-side byte(s)?</param>
			/// <returns>hexadecimal literal code string suitable for masking integers of the underlying number type</returns>
			string BuildWordMaskForInteger(int wordSize, bool lhs)
			{
				var mask_str = new string('F', wordSize);
				var zero_str = new string('0', wordSize);

				// mask is for the left-hand-side/most-significant bits
				if (lhs)
					mask_str = mask_str + zero_str;
				else // rhs/lsb
					mask_str = zero_str + mask_str;

				var sb = new System.Text.StringBuilder("0x");
				for (int x = 0; x < this.Def.SizeOfInBytes; x += wordSize)
					sb.Append(mask_str);

				return sb.ToString();
			}
			protected string BuildWordMaskForIntegerMsb(int wordSize) { return this.BuildWordMaskForInteger(wordSize, lhs:true); }
			protected string BuildWordMaskForIntegerLsb(int wordSize) { return this.BuildWordMaskForInteger(wordSize, lhs:false); }
		};

		public class BitReverseCodeGenerator
			: BitFondleCodeGenerator
		{
			public BitReverseCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			protected override void GenerateXmlDoc()
			{
				this.File.WriteXmlDocSummary("Get the bit-reversed equivalent of an unsigned integer");
				this.File.WriteXmlDocParameter(kBitsParamName,
				                               "Integer to bit-reverse");
				this.File.WriteXmlDocReturns("");
			}
			protected override void GenerateMethodSignature()
			{
				string param_bits = string.Format(UtilT4.InvariantCultureInfo,
					"{0} {1}",
					this.Def.Keyword, kBitsParamName);

				this.File.WriteLine("[Contracts.Pure]");
				this.File.WriteLine("public static {0} {1}({2})",
				                    this.Def.Keyword,
				                    "BitReverse",
				                    param_bits);
			}
			protected override void GenerateEpilogue()
			{
				this.File.Write("return ");

				if (this.Def.SizeOfInBits < PrimitiveDefinitions.kUInt32.SizeOfInBits)
					this.File.Write("({0})", this.Def.Keyword);

				this.File.Write(kFondleVarName);
				this.File.WriteLine(";");
			}

			void GenerateOperationCode(string maskLHS, string maskRHS, int shiftAmount, string doc)
			{
				this.File.Write("{0} = (({0} & {1}) >> {3,2}) | (({0} & {2}) << {3,2});",
				                kFondleVarName,
				                maskLHS,
				                maskRHS,
				                shiftAmount);
				if (doc != null)
					this.File.WriteLine(" // {0}", doc);
				else
					this.File.WriteLine("");
			}
			void GenerateBitOperationCode(byte byteMaskLHS, byte byteMaskRHS, int shiftAmount, string doc = null)
			{
				this.GenerateOperationCode(this.BuildBitMaskForInteger(byteMaskLHS),
				                           this.BuildBitMaskForInteger(byteMaskRHS),
					shiftAmount, doc);
			}
			void GenerateWordOperationCode(int wordSize, string doc = null)
			{
				string mask_lhs = this.BuildWordMaskForIntegerMsb(wordSize);
				string mask_rhs = this.BuildWordMaskForIntegerLsb(wordSize);

				this.GenerateOperationCode(
					mask_lhs,
					mask_rhs,
					(wordSize * kBitsPerByte) / 2, doc);
			}
			protected override void GenerateCode()
			{
				this.GenerateBitOperationCode(kMaskOddBits, kMaskEvenBits, 1,
				                              "swap odd and even bits");
				this.GenerateBitOperationCode(kMaskConsecutivePairsMsb, kMaskConsecutivePairsLsb, 2,
				                              "swap consecutive pairs");
				this.GenerateBitOperationCode(kMaskNibbleMsb, kMaskNibbleLsb, 4,
				                              "swap nibbles");

				if (this.Def.SizeOfInBytes >= sizeof(ushort))
					this.GenerateWordOperationCode(sizeof(ushort),	"swap bytes");
				if (this.Def.SizeOfInBytes >= sizeof(uint))
					this.GenerateWordOperationCode(sizeof(uint),		"swap halves");
				if (this.Def.SizeOfInBytes >= sizeof(ulong))
					this.GenerateWordOperationCode(sizeof(ulong),	"swap words");

				this.File.NewLine();
			}
		};

		// based on http://www.df.lth.se/~john_e/gems/gem002d.html
		// UInt8  - ~57 bytes of IL generated (Debug AnyCPU)
		// UInt16 - ~84
		// UInt32 - ~87
		// UInt64 - ~164
		public class BitCountCodeGenerator
			: BitFondleCodeGenerator
		{
			public BitCountCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			protected override void GenerateXmlDoc()
			{
				this.File.WriteXmlDocSummary("Count the number of 'on' bits in an unsigned integer");
				this.File.WriteXmlDocParameter(kBitsParamName,
				                               "Integer whose bits to count");
				this.File.WriteXmlDocReturns("");
			}
			protected override void GenerateMethodSignature()
			{
				string param_bits = string.Format(UtilT4.InvariantCultureInfo,
					"{0} {1}",
					this.Def.Keyword, kBitsParamName);

				this.File.WriteLine("[Contracts.Pure]");
				this.File.WriteLine("public static {0} {1}({2})",
				                    PrimitiveDefinitions.kInt32.Keyword,
				                    "BitCount",
				                    param_bits);
			}
			protected override void GenerateEpilogue()
			{
				this.File.WriteLine("return ({0}){1};",
				                    PrimitiveDefinitions.kInt32.Keyword,
				                    kFondleVarName);
			}

			void GenerateOperationCode(string maskLHS, string maskRHS, int shiftAmount, string doc)
			{
				this.File.Write("{0} = (({0} & {1}) >> {3,2}) + ({0} & {2});",
				                kFondleVarName,
				                maskLHS,
				                maskRHS,
				                shiftAmount);
				if (doc != null)
					this.File.WriteLine(" // {0}", doc);
				else
					this.File.NewLine();
			}
			void GenerateBitOperationCode(byte byteMaskLHS, byte byteMaskRHS, int shiftAmount, string doc = null)
			{
				this.GenerateOperationCode(this.BuildBitMaskForInteger(byteMaskLHS),
				                           this.BuildBitMaskForInteger(byteMaskRHS),
					shiftAmount, doc);
			}
			void GenerateWordOperationCode(int wordSize, string doc = null)
			{
				string mask_lhs = this.BuildWordMaskForIntegerMsb(wordSize);
				string mask_rhs = this.BuildWordMaskForIntegerLsb(wordSize);

				this.GenerateOperationCode(
					mask_lhs,
					mask_rhs,
					(wordSize * kBitsPerByte) / 2, doc);
			}
			protected override void GenerateCode()
			{
				this.GenerateBitOperationCode(kMaskOddBits, kMaskEvenBits, 1);
				this.GenerateBitOperationCode(kMaskConsecutivePairsMsb, kMaskConsecutivePairsLsb, 2);
				this.GenerateBitOperationCode(kMaskNibbleMsb, kMaskNibbleLsb, 4);

				if (this.Def.SizeOfInBytes >= sizeof(ushort))
					this.GenerateWordOperationCode(sizeof(ushort));
				if (this.Def.SizeOfInBytes >= sizeof(uint))
					this.GenerateWordOperationCode(sizeof(uint));
				if (this.Def.SizeOfInBytes >= sizeof(ulong))
					this.GenerateWordOperationCode(sizeof(ulong));

				this.File.NewLine();
			}
		};

		// based on http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
		// UInt8  - ~44 bytes of IL generated (Debug AnyCPU)
		// UInt16 - ~64
		// UInt32 - ~65
		// UInt64 - ~86
		// #TODO: the generated code for 16-bit isn't working right...who's fault is this?
		public class FastBitCountCodeGenerator
			: BitCountCodeGenerator
		{
			public FastBitCountCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
				: base(ttFile, def)
			{
			}

			void GenerateCodeStep1()
			{
				// v = v - ((v >> 1) & (T)~(T)0/3);
				this.File.WriteLine("{0} =  {0} - (({0} >> 1) & {1});",
				                    kFondleVarName,
				                    this.BuildBitMaskForInteger(kMaskEvenBits));
			}
			void GenerateCodeStep2()
			{
				// v = (v & (T)~(T)0/15*3) + ((v >> 2) & (T)~(T)0/15*3);
				this.File.WriteLine("{0} = ({0} & {1}) + (({0} >> 2) & {1});",
				                    kFondleVarName,
				                    this.BuildBitMaskForInteger(kMaskConsecutivePairsLsb));
			}
			void GenerateCodeStep3()
			{
				// v = (v + (v >> 4)) & (T)~(T)0/255*15;
				this.File.WriteLine("{0} =  {0} + ({0} >> 4) & {1};",
				                    kFondleVarName,
				                    this.BuildBitMaskForInteger(kMaskNibbleLsb));
			}
			void GenerateCodeFinalCount()
			{
				// c = (T)(v * ((T)~(T)0/255)) >> (sizeof(T) - 1) * CHAR_BIT;
				this.File.WriteLine("{0} = ({0} * {1}) >> {2};",
				                    kFondleVarName,
				                    this.BuildBitMaskForInteger(0x01),
				                    this.Def.SizeOfInBits - kBitsPerByte);
			}
			protected override void GenerateCode()
			{
				this.GenerateCodeStep1();
				this.GenerateCodeStep2();
				this.GenerateCodeStep3();
				this.GenerateCodeFinalCount();

				this.File.NewLine();
			}
		}
	};
}
