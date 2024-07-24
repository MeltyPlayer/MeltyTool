#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	/// <summary>Utility class for interacting with encoded 7-bit integers</summary>
	/// <remarks>Encoded 7-bit integers can only consume a max of 4 bytes</remarks>
	public static class Encoded7BitInt
	{
		/// <summary>Maximum value that can be stored in 1 encoded 7-bit integer</summary>
		/// <remarks>0x80 - 1</remarks>
		public const int kMaxValue1Bytes = 0x0000007F;
		/// <summary>Maximum value that can be stored in 2 encoded 7-bit integer</summary>
		/// <remarks>(0x80 &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue2Bytes = 0x00003FFF;
		/// <summary>Maximum value that can be stored in 3 encoded 7-bit integer</summary>
		/// <remarks>((0x80 &lt;&lt; 7) &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue3Bytes = 0x001FFFFF;
		/// <summary>Maximum value that can be stored in 4 encoded 7-bit integer</summary>
		/// <remarks>(((0x80 &lt;&lt; 7) &lt;&lt; 7) &lt;&lt; 7) - 1</remarks>
		public const int kMaxValue4Bytes = 0x0FFFFFFF;

		/// <summary>Calculate how many bytes it would take to encode a value into a 7-bit integer</summary>
		/// <param name="value">Value to encode</param>
		/// <returns>Number of bytes it would take to encode <paramref name="value"/></returns>
		public static int CalculateSize(int value)
		{
			Contract.Ensures(Contract.Result<int>() > 0);
			Contract.Ensures(Contract.Result<int>() < 5);

			int size = 0;
			for (uint num = (uint)value; num >= 0x80; size++)
				num >>= 7;

			return ++size;
		}
		/// <summary>Decode a value from a byte array</summary>
		/// <param name="buffer">The byte array containing the integer to decode</param>
		/// <param name="startIndex">The index of the first byte to decode</param>
		/// <param name="maxCount">The maximum amount to be decoded</param>
		/// <param name="endingIndex">The ending index after the value has been decoded, or -1 if this function fails</param>
		/// <returns>Decoded integer read from <paramref name="buffer"/> or -1 if this function fails</returns>
		public static int Read(byte[] buffer, int startIndex, int maxCount, out int endingIndex)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(buffer.Length > 1);
			Contract.Requires(startIndex > 0);
			Contract.Requires(maxCount > 0);
			endingIndex = TypeExtensions.kNone;

			int size = 0; // size (bytes) of the encoded int
			int count = 0;
			int shift = 0;
			byte b;
			do
			{
				// Check for a corrupted stream.  Access a max of 5 bytes.
				// In a future version, add a DataFormatException.
				if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
					return TypeExtensions.kNone;
				// Either a corrupted stream or the buffer is incomplete
				if (size >= maxCount)
					return TypeExtensions.kNone;

				b = buffer[startIndex + size++];
				count |= (b & 0x7F) << shift;
				shift += 7;
			} while ((b & 0x80) != 0);

			// either buffer is incomplete or
			// this isn't even data with a 7-bit integer.
			if ((size + count) > maxCount)
				return TypeExtensions.kNone;

			endingIndex = startIndex + size;

			return count;
		}
		/// <summary>Encode a value into a byte array</summary>
		/// <param name="buffer">The byte array to encode the 7-bit integer into</param>
		/// <param name="startIndex">The index of the first byte for the 7-bit integer</param>
		/// <param name="value">The value to encode into <paramref name="buffer"/></param>
		/// <returns>Index of the first byte after the encoded value in <paramref name="buffer"/></returns>
		public static int Write(byte[] buffer, int startIndex, int value)
		{
			// Write out an int 7 bits at a time.  The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			uint v = (uint)value;
			for (; v >= 0x80; v >>= 7, startIndex++)
				buffer[startIndex] = (byte)(v | 0x80);

			buffer[startIndex++] = (byte)v;

			return startIndex;
		}
	};
}
