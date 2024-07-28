using System;
using System.Collections.Generic;
using System.Linq;
using Debug = System.Diagnostics.Debug;
using TextTemplating = Microsoft.VisualStudio.TextTemplating;

namespace KSoft.T4.Bitwise
{
	public enum BitOperation
	{
		Clear,
		Set,
		Toggle,
		Test,

		kFirst = Clear,
		kLast = Test
	};

	public static partial class BitwiseT4
	{
		public const int kBitsPerByte = sizeof(byte) * 8;

		// Get the keyword used to define general constants for integer types (bit count, etc)
		public static string GetConstantKeyword(this NumberCodeDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			switch (def.Code)
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
					return "Byte";

				case TypeCode.UInt16:
				case TypeCode.Int16:
					return "Int16";

				case TypeCode.UInt32:
				case TypeCode.Int32:
					return "Int32";

				case TypeCode.UInt64:
				case TypeCode.Int64:
					return "Int64";

				default:
					throw new InvalidOperationException(def.Code.ToString());
			}
		}
		public static string GetVectorsSuffix(this NumberCodeDefinition def)
		{
			if (def == null)
				throw new ArgumentNullException(nameof(def));

			if (!def.IsByte)
				return GetConstantKeyword(def);

			return "Bytes";
		}

		#region Bitstream related
		/// <summary>	The integer type to use for bitstream cache operations. </summary>
		public static NumberCodeDefinition BitStreamCacheWord { get {
			return PrimitiveDefinitions.kUInt32;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleIntegerTypes { get {
			yield return PrimitiveDefinitions.kChar;

			foreach (var num_type in PrimitiveDefinitions.Numbers)
				if (num_type.IsInteger)
					yield return num_type;
		} }
		public static IEnumerable<PrimitiveCodeDefinition> BitStreambleNonIntegerTypes { get {
			yield return PrimitiveDefinitions.kBool;

			yield return PrimitiveDefinitions.kSingle;
			yield return PrimitiveDefinitions.kDouble;
		} }
		#endregion

		#region BitOperation extensions
		public static string FlagsMethod(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.Clear:	return "Bitwise.Flags.Remove";
			case BitOperation.Set:		return "Bitwise.Flags.Add";
			case BitOperation.Toggle:	return "Bitwise.Flags.Toggle";
			case BitOperation.Test:		return "Bitwise.Flags.TestAny";

			default: throw new InvalidOperationException(op.ToString());
			}
		}
		public static string FlagsMethodBitsPrefix(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.Clear:
			case BitOperation.Set:
			case BitOperation.Toggle:
				return "ref";

			default:
				return "";
			}
		}
		public static string ResultType(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.Test:
				return "bool";

			default:
				return "void";
			}
		}
		public static string ResultDefault(this BitOperation op)
		{
			switch (op)
			{
			case BitOperation.Test:
				return "false";

			default:
				return "";
			}
		}
		public static bool IsNotPure(this BitOperation op)
		{
			return op != BitOperation.Test;
		}
		public static bool RequiresCardinalityReUpdate(this BitOperation op)
		{
			return op != BitOperation.Test;
		}
		#endregion

		#region BittableTypes
		static readonly IReadOnlyList<NumberCodeDefinition> kBittableTypes_Unsigned = new List<NumberCodeDefinition> {
			PrimitiveDefinitions.kByte,
			PrimitiveDefinitions.kUInt16,
			PrimitiveDefinitions.kUInt32,
			PrimitiveDefinitions.kUInt64,
		};
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public static IReadOnlyList<NumberCodeDefinition> BittableTypes_Unsigned { get {
			return kBittableTypes_Unsigned;
		} }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public static IEnumerable<NumberCodeDefinition> BittableTypes_MajorWords { get {
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kUInt64;
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypes { get {
			return PrimitiveDefinitions.Numbers.Where(num_type => num_type.IsInteger);
		} }

		public static IEnumerable<NumberCodeDefinition> BittableTypesInt32 { get {
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kInt32;
		} }
		public static IEnumerable<NumberCodeDefinition> BittableTypesInt32And64 { get {
			yield return PrimitiveDefinitions.kUInt32;
			yield return PrimitiveDefinitions.kInt32;
			yield return PrimitiveDefinitions.kUInt64;
			yield return PrimitiveDefinitions.kInt64;
		} }
		#endregion

		public class IntegerByteAccessCodeGenerator
		{
			static readonly string kByteKeyword = NumberCodeDefinition.TypeCodeToKeyword(TypeCode.Byte);

			readonly TextTemplating.TextTransformation mFile;
			//readonly NumberCodeDefinition mDef;
			readonly int mSizeOfInBits;
			readonly int mSizeOfInBytes;
			readonly string mByteName;

			readonly string mBufferName;
			readonly string mOffsetName; // NOTE: offset variable must be mutable!

			internal IntegerByteAccessCodeGenerator(TextTemplating.TextTransformation ttFile,
				NumberCodeDefinition def,
				string byteName, string bufferName, string offsetName = null,
				int bitCount = -1)
			{
				this.mFile = ttFile;
				//mDef = def;
				this.mSizeOfInBits = bitCount == -1
					? def.SizeOfInBits
					: bitCount;
				this.mSizeOfInBytes = bitCount == -1
					? def.SizeOfInBytes
					: bitCount / kBitsPerByte;
				this.mByteName = byteName;

				this.mBufferName = bufferName;
				this.mOffsetName = offsetName;
			}

			void GenerateByteDeclarationsCode()
			{
				this.mFile.Write("{0} ", kByteKeyword);
				for (int x = 0; x < this.mSizeOfInBytes; x++)
				{
					this.mFile.Write("{0}{1}", this.mByteName, x);

					if (x < (this.mSizeOfInBytes - 1))
						this.mFile.Write(", ");
				}

				this.mFile.WriteLine(";");
			}
			public void GenerateByteDeclarations()
			{
				// indent to method code body's indention level
				using (this.mFile.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateByteDeclarationsCode();
				}
			}

			void GenerateBytesFromBufferCode()
			{
				Debug.Assert(this.mOffsetName != null, "generator not setup for read/write from/to a buffer");

				for (int x = 0; x < this.mSizeOfInBytes; x++, this.mFile.WriteLine(";"))
				{
					this.mFile.Write("{0}{1} = ", this.mByteName, x);
					this.mFile.Write("{0}[{1}++]", this.mBufferName, this.mOffsetName);
				}
			}
			public void GenerateBytesFromBuffer()
			{
				// indent to method code body's indention level
				using (this.mFile.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateBytesFromBufferCode();
				}
			}

			void GenerateBytesFromValueCode(string valueName, bool littleEndian)
			{
				int bit_offset = !littleEndian
					? this.mSizeOfInBits
					: 0 - kBitsPerByte;
				int bit_adjustment = !littleEndian
					? -kBitsPerByte
					: +kBitsPerByte;

				for (int x = 0; x < this.mSizeOfInBytes; x++, this.mFile.WriteLine(";"))
				{
					this.mFile.Write("{0}{1} = ", this.mByteName, x);
					this.mFile.Write("({0})({1} >> {2,2})", kByteKeyword, valueName, bit_offset += bit_adjustment);
				}
			}
			public void GenerateBytesFromValue(string valueName, bool littleEndian = true)
			{
				// indent to method code body's indention level, plus one (assumed we are in a if-statement)
				using (this.mFile.EnterCodeBlock(indentCount: 3+1))
				{
					this.GenerateBytesFromValueCode(valueName, littleEndian);
				}
			}

			void GenerateWriteBytesToBufferCode(bool useSwapFormat)
			{
				const string k_swap_format =		"{0}[--{1}] = ";
				const string k_replacement_format =	"{0}[{1}++] = ";

				Debug.Assert(this.mOffsetName != null);

				string format = useSwapFormat
					? k_swap_format
					: k_replacement_format;

				for (int x = 0; x < this.mSizeOfInBytes; x++, this.mFile.WriteLine(";"))
				{
					this.mFile.Write(format, this.mBufferName, this.mOffsetName);
					this.mFile.Write("{0}{1}", this.mByteName, x);
				}
			}
			public void GenerateWriteBytesToBuffer(bool useSwapFormat = true)
			{
				// indent to method code body's indention level
				using (this.mFile.EnterCodeBlock(indentCount: 3))
				{
					this.GenerateWriteBytesToBufferCode(useSwapFormat);
				}
			}
		};

		public abstract class BitUtilCodeGenerator
		{
			protected TextTemplating.TextTransformation File { get; private set; }
			protected NumberCodeDefinition Def { get; private set; }

			protected BitUtilCodeGenerator(TextTemplating.TextTransformation ttFile, NumberCodeDefinition def)
			{
				this.File = ttFile;
				this.Def = def;
			}

			public void Generate()
			{
				using (this.File.EnterCodeBlock())
				using (this.File.EnterCodeBlock())
				{
					this.GenerateXmlDoc();
					this.GenerateMethodSignature();

					using (this.File.EnterCodeBlock(TextTransformationCodeBlockType.Brackets))
					{
						this.GeneratePrologue();
						this.GenerateCode();
						this.GenerateEpilogue();
					}
				}
			}

			protected abstract void GenerateXmlDoc();
			protected abstract void GenerateMethodSignature();
			protected abstract void GeneratePrologue();
			protected abstract void GenerateEpilogue();
			protected abstract void GenerateCode();
		};
	};
}
