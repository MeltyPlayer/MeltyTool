using System;
using System.Collections.Generic;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	partial class BitwiseT4
	{
		public sealed class ByteSwapableIntegerDefinition
		{
			readonly NumberCodeDefinition mCodeDef;
			readonly int mSizeOfInBits;
			readonly int mSizeOfInBytes;

			internal ByteSwapableIntegerDefinition(NumberCodeDefinition codeDef, int bitCount)
			{
				mCodeDef = codeDef;
				mSizeOfInBits = bitCount;
				mSizeOfInBytes = bitCount / kBitsPerByte;
			}
			internal ByteSwapableIntegerDefinition(NumberCodeDefinition codeDef)
				: this(codeDef, codeDef.SizeOfInBits)
			{
			}

			public NumberCodeDefinition CodeDefinition { get { return mCodeDef; } }
			public int SizeOfInBits { get { return mSizeOfInBits; } }
			public int SizeOfInBytes { get { return mSizeOfInBytes; } }

			/// <summary>Can this integer not be represented via a one of .NET's System.Int types?</summary>
			public bool IsUnnaturalWord { get { return SizeOfInBits != mCodeDef.SizeOfInBits; } }

			public string Keyword { get { return mCodeDef.Keyword; } }
			public string SignedKeyword { get { return mCodeDef.SignedKeyword; } }
			public TypeCode Code { get { return mCodeDef.Code; } }
			public TypeCode SignedCode { get { return mCodeDef.SignedCode; } }

			public string ToStringHexFormat { get { return mCodeDef.ToStringHexFormat; } }
			public bool BitOperatorsImplicitlyUpCast { get { return mCodeDef.BitOperatorsImplicitlyUpCast; } }

			public string GetConstantKeyword()
			{
				return IsUnnaturalWord
					? "Int" + SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: mCodeDef.GetConstantKeyword();
			}

			public NumberCodeDefinition TryGetSignedDefinition() { return mCodeDef.TryGetSignedDefinition(); }

			public string WordTypeNameUnsigned { get {
				return IsUnnaturalWord
					? "UInt" + SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: Code.ToString();
			} }
			public string WordTypeNameSigned { get {
				return IsUnnaturalWord
					? "Int" + SizeOfInBits.ToString(UtilT4.InvariantCultureInfo)
					: SignedCode.ToString();
			} }
			public string GetOverloadSuffixForUnnaturalWord(bool isSigned)
			{
				if (!IsUnnaturalWord)
					return "";

				return isSigned
					? WordTypeNameSigned
					: WordTypeNameUnsigned;
			}

			public string SizeOfCode { get {
				return IsUnnaturalWord
					? "kSizeOf" + GetConstantKeyword()
					: string.Format(UtilT4.InvariantCultureInfo, "sizeof({0})", Keyword);
			} }

			public IntegerByteSwapCodeGenerator NewByteSwapCodeGenerator(TextTemplating.TextTransformation ttFile,
				string valueName = "value")
			{
				return new IntegerByteSwapCodeGenerator(ttFile, this, valueName);
			}

			public IntegerByteAccessCodeGenerator NewByteAccessCodeGenerator(TextTemplating.TextTransformation ttFile,
				string byteName = "b", string bufferName = "buffer", string offsetName = "offset")
			{
				return new IntegerByteAccessCodeGenerator(ttFile, mCodeDef, byteName, bufferName, offsetName, mSizeOfInBits);
			}
		};

		public static IEnumerable<ByteSwapableIntegerDefinition> ByteSwapableIntegers { get {
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.kUInt16);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.kUInt32);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.kUInt64);

			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.kUInt32, 24);
			yield return new ByteSwapableIntegerDefinition(PrimitiveDefinitions.kUInt64, 40);
		} }

		public class IntegerByteSwapCodeGenerator
		{
			TextTemplating.TextTransformation mFile;
			ByteSwapableIntegerDefinition mDef;

			readonly string mValueName;

			public IntegerByteSwapCodeGenerator(TextTemplating.TextTransformation ttFile,
				ByteSwapableIntegerDefinition def, string valueName)
			{
				mFile = ttFile;
				mDef = def;

				mValueName = valueName;
			}

			void GeneratePrologue(bool cast, bool isSigned)
			{
				// smaller integers are promoted to larger types when bit operated on
				// this is for casting the final result back into the smaller integer type
				if (cast)
				{
					var def = isSigned
						? mDef.TryGetSignedDefinition()
						: mDef.CodeDefinition;

					mFile.WriteLine("({0})( ", def.Keyword);
					mFile.PushIndent(TextTransformationCodeBlockBookmark.kIndent);
				}
			}
			void GenerateEpilogue(bool cast)
			{
				// close the cast operation group
				if (cast)
				{
					mFile.PopIndent();
					mFile.Write(")");
				}
			}
			void GenerateStep(bool isSigned, int shift, string mask, bool lastOperation = false)
			{
				// mask is not added when this is the last op in signed expression as the mask will be an unsigned value
				// this isn't the case for unnatural-words, which should consume fewer bits than the MSB/sign-bit
				bool mask_op = mDef.IsUnnaturalWord || !isSigned || !lastOperation;
				if (mask_op)
					mFile.Write("(");
				else
					mFile.Write(" "); // add a space to keep aligned with statements prefixed with '('

				// start the shift operation group
				mFile.Write("({0}", mValueName);

				// LHS with positive numbers, RHS with negative
				if (shift > 0)
					mFile.Write(" << ");
				else
				{
					mFile.Write(" >> ");
					shift = -shift;
				}
				mFile.Write("{0,2}", shift);

				// close the shift operation group
				mFile.Write(")");

				// add the mask operation
				if (mask_op)
				{
					mFile.Write(" & ");
					mFile.Write("0x{0})", mask);
				}

				// not the last operation so OR this with the next operation
				if (!lastOperation)
					mFile.Write(" | ");
			}

			void GenerateCode(bool isSigned)
			{
				// amount to increase 'shift' after each step
				const int k_step_shift_inc = kBitsPerByte * 2;

				// do we need to cast the result?
				bool cast = mDef.BitOperatorsImplicitlyUpCast;
				string hex_format = mDef.ToStringHexFormat;

				GeneratePrologue(cast, isSigned);

				// GenerateStep's 'shift' is negative when the operation is a RHS (>>), and positive for LHS (<<)
				int shift = (0-mDef.SizeOfInBits) + kBitsPerByte;
				// our byte mask for each step
				ulong mask = byte.MaxValue;
				// While 'shift' is negative, we're generating steps to swap the MSB bytes to the LSBs half.
				// Once 'shift' becomes positive, we're generating steps to swap the LSB bytes to the MSBs half.
				for(int x = mDef.SizeOfInBytes-1; x >= 0; x--, shift += k_step_shift_inc, mask <<= kBitsPerByte)
				{
					GenerateStep(isSigned, shift, mask.ToString(hex_format, UtilT4.InvariantCultureInfo), x == 0);
					mFile.NewLine();
				}

				GenerateEpilogue(cast);
			}
			public void Generate(bool isSigned = false)
			{
				// indent to method code body's indention level, plus one (l-value statement should be on the line before)
				using (mFile.EnterCodeBlock(indentCount: 3+1))
				{
					GenerateCode(isSigned);

					mFile.EndStmt();
				}
			}
		};
	};
}
