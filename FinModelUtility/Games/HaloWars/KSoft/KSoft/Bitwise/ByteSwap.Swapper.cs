using System;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	partial class ByteSwap
	{
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct Swapper
		{
			readonly short[] kCodes;

			public Swapper(int sizeOf, params short[] codes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(sizeOf > 0);
				Contract.Requires<ArgumentNullException>(codes != null);

				this.kCodes = codes;
			}
			public Swapper(IByteSwappable definition)
			{
				Contract.Requires<ArgumentNullException>(definition != null);

				this.kCodes = definition.ByteSwapCodes;
			}

			public int SwapData(byte[] buffer, int startIndex = 0)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startIndex <= buffer.Length);

				return this.SwapData(buffer, startIndex, out int size_in_bytes, out int size_in_codes);
			}

			public int SwapData(byte[] buffer, int startIndex,
				out int sizeInBytes, out int sizeInCodes)
			{
				Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(buffer == null || startIndex <= buffer.Length);

				return this.SwapDataImpl(buffer, startIndex, out sizeInBytes, out sizeInCodes, 0);
			}

			private int SwapDataImpl(byte[] buffer, int startIndex
				, out int outSizeInBytes, out int outSizeInCodes
				, int codesStartIndex)
			{
				outSizeInBytes = outSizeInCodes = 0;
				int size_in_bytes = 0;
				int size_in_codes = 0;

				int codes_index = codesStartIndex;
				Contract.Assert(this.kCodes[codes_index] == (int)BsCode.ArrayStart);

				int array_count = this.kCodes[codes_index + 1]; // array count comes after ArrayStart

				bool buffer_is_valid = buffer != null;
				int buffer_index = startIndex;
				for (int elements_remaining = array_count; elements_remaining > 0; )
				{
					size_in_codes = 2;
					bool found_array_end = false;
					for (codes_index = codesStartIndex + size_in_codes; !found_array_end; )
					{
						var current_code = this.kCodes[codes_index];
						switch (current_code)
						{
							#region Word
							case (int)BsCode.Int16:
								if (buffer_is_valid)
								{
									SwapInt16(buffer, buffer_index);
									buffer_index += sizeof(short);
								}

								size_in_bytes += sizeof(short);
								size_in_codes++;
								codes_index++;
								break;
							#endregion

							#region DWord
							case (int)BsCode.Int32:
								if (buffer_is_valid)
								{
									SwapInt32(buffer, buffer_index);
									buffer_index += sizeof(int);
								}

								size_in_bytes += sizeof(int);
								size_in_codes++;
								codes_index++;
								break;
							#endregion

							#region QWord
							case (int)BsCode.Int64:
								if (buffer_is_valid)
								{
									SwapInt64(buffer, buffer_index);
									buffer_index += sizeof(long);
								}

								size_in_bytes += sizeof(long);
								size_in_codes++;
								codes_index++;
								break;
							#endregion

							case (int)BsCode.ArrayStart:
								int recursive_size_in_bytes, recursive_size_in_codes;

								this.SwapDataImpl(buffer, buffer_index,
								                  out recursive_size_in_bytes, out recursive_size_in_codes,
								                  codes_index);

								if (buffer_is_valid)
									buffer_index += recursive_size_in_bytes;

								codes_index += recursive_size_in_codes;
								size_in_codes += recursive_size_in_codes;
								size_in_bytes += recursive_size_in_bytes;
								break;

							case (int)BsCode.ArrayEnd:
								codes_index++;
								size_in_codes++;
								elements_remaining--;
								found_array_end = true;
								break;

							#region Skip (default)
							default:
								if (current_code < 0)
									throw new Debug.UnreachableException();

								if (buffer_is_valid)
									buffer_index += current_code;

								codes_index++;
								size_in_codes++;
								size_in_bytes += current_code;
								break;
							#endregion
						}
					}
				}

				outSizeInBytes = size_in_bytes;
				outSizeInCodes = size_in_codes;
				return buffer_is_valid
					? buffer_index
					: TypeExtensions.kNone;
			}
		};
	};
}
