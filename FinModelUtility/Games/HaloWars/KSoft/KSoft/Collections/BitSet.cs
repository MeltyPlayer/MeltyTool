using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using KSoft.Bitwise;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

// the 'implementation word'
using TWord = System.UInt32;

namespace KSoft.Collections
{
	using StateEnumerator = IReadOnlyBitSetEnumerators.StateEnumerator;

	// http://docs.oracle.com/javase/7/docs/api/java/util/BitSet.html

	// NOTE: there are multiple places in this implementation where it ignores specially handling alignment-only bits.
	// Eg, if a BitSet has 33 bits in it, it would be aligned to 64 bits. If you then called SetAll(true) on it, it
	// would end up setting 64 bits to true. If you then set the Length to be 64, those previously alignment-only bits
	// would then retain their true state.
	// ...however, as of 2015, Length and all other places should now be void of this problem (with alignment only bits)

	[System.Diagnostics.DebuggerDisplay("Length = {Length}, Cardinality = {Cardinality}")]
	[Serializable, System.Runtime.InteropServices.ComVisible(true)]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public sealed partial class BitSet
		: ICollection<bool>, System.Collections.ICollection
		, IReadOnlyBitSet
		, IO.IEndianStreamSerializable
	{
		#region Constants
		static readonly int kWordBitMod;
		/// <summary>Number of bits in the implementation word</summary>
		const int kWordBitCount = sizeof(TWord) * Bits.kByteBitCount;

		const TWord kWordAllBitsClear = TWord.MinValue;
		const TWord kWordAllBitsSet = TWord.MaxValue;

		static readonly Bits.VectorLengthInT kVectorLengthInT;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementBitMask;
		static readonly Bits.VectorElementBitMask<TWord> kVectorElementSectionBitMask;
		static readonly Bits.VectorIndexInT kVectorIndexInT;
		static readonly Bits.VectorBitCursorInT kVectorBitCursorInT;

		static readonly Func<TWord, byte> kCountZerosForNextBit;

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static BitSet()
		{
			bool success = Bits.GetBitConstants(typeof(TWord),
				out int word_byte_count, out int word_bit_count, out int word_bit_shift, out kWordBitMod);
			Contract.Assert(success, "TWord is an invalid type for BitSet");

			kVectorLengthInT = Bits.GetVectorLengthInT<TWord>();
			Bits.GetVectorElementBitMaskInT(out kVectorElementBitMask);
			Bits.GetVectorElementSectionBitMaskInT(out kVectorElementSectionBitMask);
			kVectorIndexInT = Bits.GetVectorIndexInT<TWord>();
			kVectorBitCursorInT = Bits.GetVectorBitCursorInT<TWord>();

#pragma warning disable 0429 // Unreachable expression code detected
			// Big:    Bits go from MSB->LSB, so we want to count the 'left most' zeros
			// Little: Bits go from LSB->MSB, so we want to count the 'right most' zeros
			kCountZerosForNextBit = Bits.kVectorWordFormat == Shell.EndianFormat.Big
				? (Func<TWord,byte>)Bits.LeadingZerosCount   // Big Endian
				: (Func<TWord,byte>)Bits.TrailingZerosCount; // Little Endian
#pragma warning restore 0162
		}

		/// <summary>
		/// Get the mask needed for a caboose word for masking out its alignment-only (ie, unaddressable) bits
		/// </summary>
		/// <param name="bitLength"></param>
		/// <returns>Mask representing the non-alignment bits in the caboose word, or 0 if there are no alignment-only bits</returns>
		static TWord GetCabooseRetainedBitsMask(int bitLength)
		{
			// if there are no bits left over, then the bit length doesn't require any alignment-only bits
			if ((bitLength & kWordBitMod) == 0)
				return 0;

			TWord retained_bits_mask;
#pragma warning disable 0162 // comparing const values, could have 'unreachable' code
			if (Bits.kVectorWordFormat == Shell.EndianFormat.Big)
			{
				retained_bits_mask = kVectorElementBitMask(bitLength-1);
				// create a mask for all bits below the given length in a caboose word
				retained_bits_mask -= 1;
				// when bitvectors are written MSB->LSB, we have to invert the mask (which begins at the LSB)
				retained_bits_mask = ~retained_bits_mask;
			}
			else
			{
				retained_bits_mask = kVectorElementBitMask(bitLength);
				// create a mask for all bits below the given length in a caboose word
				retained_bits_mask -= 1;
			}
#pragma warning restore 0162

			return retained_bits_mask;
		}
		#endregion

		#region Instance data
		TWord[] mArray;
		int mLength;
		int mVersion;
		#endregion

		/// <summary>Size of a single implementation word, in bytes, used in the internal array to represent this bit set</summary>
		public int UnderlyingWordSize { get => sizeof(TWord); }
		/// <summary>Number of implementation words <b>used</b> in the internal array to represent this bit set</summary>
		/// <remarks>
		/// This differs from <see cref="LengthInWords"/> as the internal array can be larger than needed due to downsizing
		/// without calling <see cref="TrimExcess"/>
		/// </remarks>
		public int UnderlyingWordCount { get => this.mArray.Length; }

		/// <summary>Can <see cref="Length"/> be adjusted?</summary>
		public bool FixedLength { get; set; }
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>
		/// IE, the index of the highest addressable bit plus one
		///
		/// Note: when downsizing, the underlying storage's size stays the same, but the old bits will be zeroed and
		/// unaddressable. Call <see cref="TrimExcess"/> to optimize the underlying storage to the minimal size
		/// </remarks>
		public int Length {
			get { return this.mLength; }
			set {
				Contract.Requires<InvalidOperationException>(!this.FixedLength);
				Contract.Requires<ArgumentOutOfRangeException>(value >= 0);
				if (value == this.mLength)
					return;

				int value_in_words = kVectorLengthInT(value);
				#region resize mArray if needed
				if (value_in_words > this.mArray.Length)
				{
					var new_array = new TWord[value_in_words];
					Array.Copy(this.mArray, new_array, this.mArray.Length);
					this.mArray = new_array;
				}
				#endregion

				#region clear old bits if downsizing
				if (value < this.mLength)
				{
					kVectorBitCursorInT(value, out int index, out int bit_offset);

					// clear old, now unused, bits in the caboose word
					if (bit_offset != 0)
					{
						// create a mask for all bits below the new length in this word
						var retained_bits_mask = GetCabooseRetainedBitsMask(value);

						// keep the still-used bits in the vector, while masking out the old ones
						this.mArray[index] &= retained_bits_mask;
					}
					else // no caboose, 'hack' index so that the loop below clears it
						index--;

					for (int x = index + 1; x < this.mArray.Length; x++)
						this.mArray[x] = 0;

					// update the cardinality, if needed
					if (this.Cardinality > 0)
					{
						this.Cardinality = 0;
						for (int x = 0; x < value_in_words; x++)
							this.RecalculateCardinalityRound(x);
					}
				}
				#endregion

				this.mLength = value;
				this.mVersion++;
			}
		}
		/// <summary>Number of implementation words <b>needed</b> to represent this bit set</summary>
		/// <remarks>
		/// This differs from <see cref="UnderlyingWordCount"/> as it only considers the absolute least amounts of words needed
		/// and ignores any extra space that may have been accumulated from length downsizing without a call to <see cref="TrimExcess"/>
		/// </remarks>
		public int LengthInWords { get => kVectorLengthInT(this.mLength); }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality { get; private set; }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros { get => this.Length - this.Cardinality; }

		/// <summary>Are all the bits in this set currently false?</summary>
		public bool IsAllClear { get => this.Cardinality == 0; }

		int IReadOnlyBitSet.Version { get => this.mVersion; }

		#region Ctor
		#region InitializeArray
		void InitializeArrayWithDefault(int length, bool defaultValue, out int outBitLength)
		{
			outBitLength = length;

			this.mArray = new TWord[kVectorLengthInT(length)];

			this.SetAllInternal(defaultValue);

			// the above method doesn't modify anything besides the raw bits,
			if (defaultValue)
				this.Cardinality = outBitLength;
		}
		void InitializeArrayFromBytes(byte[] bytes, int index, int length, out int outBitLength)
		{
			outBitLength = length * Bits.kByteBitCount;

			this.mArray = new TWord[kVectorLengthInT(outBitLength)];

			Buffer.BlockCopy(bytes, index, this.mArray, 0, length);

			this.RecalculateCardinality();
		}
		void InitializeArrayFromBools(bool[] values, int index, int length, out int outBitLength)
		{
			outBitLength = length;

			this.mArray = new TWord[kVectorLengthInT(outBitLength)];

			for (int x = 0; x < length; x++)
				if (values[index + x])
				{
					this.mArray[kVectorIndexInT(x)] |= kVectorElementBitMask(x);
					this.Cardinality++;
				}
		}
		#endregion

		/// <summary>Creates an empty, growable bit-set</summary>
		public BitSet()
			: this(0, defaultValue:false, fixedLength:false)
		{
		}

		public BitSet(int length, bool defaultValue = false, bool fixedLength = true)
		{
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);

			this.mVersion = 0;
			this.InitializeArrayWithDefault(length, defaultValue, out this.mLength);
			this.FixedLength = fixedLength;
		}

		public BitSet(byte[] bytes, int index, int length, bool fixedLength = true)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < bytes.Length);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index+length) <= bytes.Length);

			this.mVersion = 0;
			this.InitializeArrayFromBytes(bytes, index, length, out this.mLength);
			this.FixedLength = fixedLength;
		}
		[SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		public BitSet(params byte[] bytes) : this(bytes, 0, bytes.Length, true)
		{
		}

		public BitSet(bool[] values, int index, int length, bool fixedLength = true)
		{
			Contract.Requires<ArgumentNullException>(values != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < values.Length);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((index + length) <= values.Length);

			this.mVersion = 0;
			this.InitializeArrayFromBools(values, index, length, out this.mLength);
			this.FixedLength = fixedLength;
		}
		[SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		public BitSet(params bool[] values) : this(values, 0, values.Length, true)
		{
		}

		public BitSet(BitSet set)
		{
			Contract.Requires<ArgumentNullException>(set != null);

			this.mArray = new TWord[set.mArray.Length];
			Array.Copy(set.mArray, this.mArray, this.mArray.Length);

			this.mLength = set.mLength;
			this.Cardinality = set.Cardinality;
			this.mVersion = set.mVersion;
			this.FixedLength = set.FixedLength;
		}
		public object Clone()	=> new BitSet(this);
		#endregion

		#region RecalculateCardinality
		/// <summary>Update <see cref="Cardinality"/> for an individual word in the underlying array</summary>
		/// <param name="wordIndex"></param>
		void RecalculateCardinalityRound(int wordIndex)		=> this.Cardinality += Bits.BitCount(this.mArray[wordIndex]);

		/// <summary>Undo a previous <see cref="RecalculateCardinalityRound"/> for an individual word in the underlying array</summary>
		/// <param name="wordIndex"></param>
		void RecalculateCardinalityUndoRound(int wordIndex)	=> this.Cardinality -= Bits.BitCount(this.mArray[wordIndex]);

		void RecalculateCardinalityFinishRounds(int startWordIndex)
		{
			for (int x = startWordIndex, word_count = this.LengthInWords; x < word_count; x++)
				this.RecalculateCardinalityRound(x);
		}
		void RecalculateCardinality()
		{
			this.Cardinality = 0;
			for (int x = 0, word_count = this.LengthInWords; x < word_count; x++)
				this.RecalculateCardinalityRound(x);
		}
		#endregion

		#region ClearAlignmentOnlyBits
		void ClearAlignmentOnlyBits()
		{
			int caboose_word_index = this.LengthInWords - 1;

			if (caboose_word_index < 0)
				return;

			var retained_bits_mask = GetCabooseRetainedBitsMask(this.Length);

			if (retained_bits_mask == 0)
				return;

			this.mArray[caboose_word_index] &= retained_bits_mask;
		}
		void ClearAlignmentOnlyBitsForBitOperation(IReadOnlyBitSet value)
		{
			// if the operation value is longer, it could possibly contain more addressable bits in its caboose word,
			// causing this set's caboose word to have those bits be non-zero in the Bit Operation. So zero them out.
			if (value.Length <= this.Length)
				return;

			var retained_bits_mask = GetCabooseRetainedBitsMask(this.Length);

			if (retained_bits_mask == 0)
				return;

			int last_word_index = this.LengthInWords - 1;
			// the Bit Operations below update Cardinality as each word is touched
			// so we'll need to 'undo' the last word's round before we mask out the alignment-only bits, then recalc
			this.RecalculateCardinalityUndoRound(last_word_index);
			this.mArray[last_word_index] &= retained_bits_mask;
			this.RecalculateCardinalityRound(last_word_index);
		}
		#endregion

		#region Access
		public bool this[int bitIndex] {
			get {
				// REMINDER: Contract for bitIndex already specified by IReadOnlyBitSet's contract

				return this.GetInternal(bitIndex);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < this.Length);

				this.SetInternal(bitIndex, value);
			}
		}
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		public bool this[int frombitIndex, int toBitIndex] {
			get {
				// REMINDER: Contracts already specified by IReadOnlyBitSet's contract

				int bitCount = toBitIndex - frombitIndex;
				return bitCount > 0 && this.TestBits(frombitIndex, bitCount);
			}
			set {
				Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < this.Length);
				Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= this.Length);

				// handle the cases of the set already being all 1's or 0's
				if (value && this.Cardinality == this.Length)
					return;
				if (!value && this.CardinalityZeros == this.Length)
					return;

				int bitCount = toBitIndex - frombitIndex;
				if (bitCount == 0)
					return;

				if (value)
					this.SetBits(frombitIndex, bitCount);
				else
					this.ClearBits(frombitIndex, bitCount);
			}
		}

		bool GetInternal(int bitIndex, out int wordIndex, out TWord bitmask)
		{
			wordIndex = kVectorIndexInT(bitIndex);
			bitmask = kVectorElementBitMask(bitIndex);

			return Flags.Test(this.mArray[wordIndex], bitmask);
		}
		/// <summary>Get the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		bool GetInternal(int bitIndex) => this.GetInternal(bitIndex, out int index, out TWord bitmask);

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		public bool Get(int bitIndex) => this.GetInternal(bitIndex);

		void SetInternal(int wordIndex, TWord bitmask, bool value)
		{
			if (value)
			{
				Flags.Add(ref this.mArray[wordIndex], bitmask);
				++this.Cardinality;
			}
			else
			{
				Flags.Remove(ref this.mArray[wordIndex], bitmask);
				--this.Cardinality;
			}

			this.mVersion++;
		}
		/// <summary>Set the value of a specific bit, without performing and bounds checking on the bit index</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		void SetInternal(int bitIndex, bool value)
		{
			// #REVIEW: is it really worth checking that we're not setting a bit to the same state?
			// Yes, currently, as SetInternal updates Cardinality

			bool old_value = this.GetInternal(bitIndex, out int index, out TWord bitmask);

			if (old_value != value)
				this.SetInternal(index, bitmask, value);
		}
		/// <summary>Set the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		public void Set(int bitIndex, bool value)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < this.Length);

			this.SetInternal(bitIndex, value);
		}

		/// <summary>Flip the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns>The bit's new value</returns>
		public bool Toggle(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < this.Length);

			bool old_value = this.GetInternal(bitIndex, out int index, out TWord bitmask);
			bool new_value = !old_value;

			this.SetInternal(index, bitmask, new_value);

			return new_value;
		}

		void SetAllInternal(bool value)
		{
			int word_count = this.LengthInWords;

			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			var fill_value = value
				? kWordAllBitsSet
				: kWordAllBitsClear;
			for (int x = 0; x < word_count; x++)
				this.mArray[x] = fill_value;

			if (value) // so if any exist, zero them out
				this.ClearAlignmentOnlyBits();

			// intentionally don't update Cardinality or mVersion here
		}
		public void SetAll(bool value)
		{
			this.SetAllInternal(value);

			this.Cardinality = value
				? this.Length
				: 0;

			this.mVersion++;
		}

		public int NextBitIndex(int startBitIndex, bool stateFilter)
		{
			kVectorBitCursorInT(startBitIndex, out int index, out int bit_offset);

			// get a mask for the the bits that start at bit_offset, thus ignoring bits that came before startBitIndex
			var bitmask = kVectorElementSectionBitMask(bit_offset);

			int result_bit_index = TypeExtensions.kNone;
			var word = this.mArray[index];
			for (	word = (stateFilter == false ? ~word : word) & bitmask;
					result_bit_index.IsNone();
					word =  stateFilter == false ? ~this.mArray[index] : this.mArray[index])
			{
				// word will be 0 if it contains bits that are NOT stateFilter, thus we want to ignore such elements.
				// count the number of zeros (representing bits in the undesired state) leading up to the bit with
				// the desired state, then add the the index in which it appears at within the overall BitSet
				if (word != 0)
					result_bit_index = kCountZerosForNextBit(word) + (index * kWordBitCount);

				// I perform the increment and loop condition here to keep the for() statement simple
				if (++index == this.mArray.Length)
					break;
			}

			// If we didn't find a next bit, result will be -1 and thus less than Length, which is desired behavior
			// else, the result is a valid index of the next bit with the desired state
			return result_bit_index < this.Length
				? result_bit_index
				: TypeExtensions.kNone;
		}

		public StateEnumerator GetEnumerator() => new StateEnumerator(this);
		IEnumerator<bool> IEnumerable<bool>.GetEnumerator() => new StateEnumerator(this);
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new StateEnumerator(this);
		#endregion

		#region Bit Operations
		/// <summary>Bit AND this set with another</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet And(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			if (!ReferenceEquals(value, this) && value.Length > 0)
			{
				this.Cardinality = 0;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					this.mArray[word_index] &= value.mArray[word_index];
					this.RecalculateCardinalityRound(word_index);
				}

				// NOTE: we don't do ClearAlignmentOnlyBitsForBitOperation here as a larger BitSet won't introduce
				// new TRUE-bits in a And() operation

				this.RecalculateCardinalityFinishRounds(word_index);

				this.mVersion++;
			}
			return this;
		}
		/// <summary>Clears all of the bits in this set whose corresponding bit is set in the specified BitSet</summary>
		/// <param name="value">BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public BitSet AndNot(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			// we're clearing with ourself, just clear all the bits
			if (ReferenceEquals(value, this))
			{
				this.Clear(); // will increment mVersion
			}
			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this will be cleared.
			else if (value.Cardinality > 0)
			{
				this.Cardinality = 0;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					this.mArray[word_index] &= ~value.mArray[word_index];
					this.RecalculateCardinalityRound(word_index);
				}

				this.ClearAlignmentOnlyBitsForBitOperation(value);

				this.RecalculateCardinalityFinishRounds(word_index);

				this.mVersion++;
			}

			return this;
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet Or(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this would get modified.
			if (!ReferenceEquals(value, this) && value.Cardinality > 0)
			{
				this.Cardinality = 0;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					this.mArray[word_index] |= value.mArray[word_index];
					this.RecalculateCardinalityRound(word_index);
				}

				this.ClearAlignmentOnlyBitsForBitOperation(value);

				this.RecalculateCardinalityFinishRounds(word_index);

				this.mVersion++;
			}
			return this;
		}
		/// <summary>Bit XOR this set with another</summary>
		/// <param name="value">Set with the bits to XOR with</param>
		/// <returns>Returns the current instance</returns>
		public BitSet Xor(BitSet value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			// we're clearing with ourself, just clear all the bits
			if (ReferenceEquals(value, this))
			{
				this.Clear(); // will increment mVersion
			}
			// test Cardinality, not Length, to optimally handle empty and all-zeros bitsets.
			// if value is all-zeros, no bits in this would get modified.
			else if (value.Cardinality > 0)
			{
				this.Cardinality = 0;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				int word_index;
				for (word_index = 0; word_index < length_in_words; word_index++)
				{
					this.mArray[word_index] ^= value.mArray[word_index];
					this.RecalculateCardinalityRound(word_index);
				}

				this.ClearAlignmentOnlyBitsForBitOperation(value);

				this.RecalculateCardinalityFinishRounds(word_index);

				this.mVersion++;
			}

			return this;
		}

		/// <summary>Inverts all bits in this set</summary>
		/// <returns>Returns the current instance</returns>
		public BitSet Not()
		{
			// NOTE: if the array is auto-aligned, this will end up setting alignment-only data
			for (int x = 0, word_count = this.LengthInWords; x < word_count; x++)
				this.mArray[x] = (TWord)~this.mArray[x];
			// so reset that data
			this.ClearAlignmentOnlyBits();

			// invert the Cardinality as what was once one is now none!
			this.Cardinality = this.CardinalityZeros;

			this.mVersion++;
			return this;
		}
		#endregion

		/// <summary>Compare the words of this set with another</summary>
		/// <param name="other">The other set, that is equal or greater in length, to compare bits with</param>
		/// <param name="bitsCount">Number of bits to compare</param>
		/// <returns></returns>
		bool BitwiseEquals(BitSet other, int bitsCount)
		{
			if (ReferenceEquals(other, this))
				return true;

			int word_index = 0;
			int word_count = kVectorLengthInT(bitsCount) - 1;
			Contract.Assume((word_index+word_count) <= this.LengthInWords);
			Contract.Assume((word_index+word_count) <= other.LengthInWords);

			for (; word_index < word_count; word_index++, bitsCount -= kWordBitCount)
			{
				if (this.mArray[word_index] != other.mArray[word_index])
					return false;
			}

			var last_word_mask = GetCabooseRetainedBitsMask(bitsCount);

			return
				( this.mArray[word_index] & last_word_mask) ==
				(other.mArray[word_index] & last_word_mask);
		}
		#region ISet-like interfaces
		/// <summary>This set is included in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSubsetOf(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			// THIS is a subset of OTHER if it contains the same set bits as OTHER
			// If THIS is larger, then OTHER couldn't contain the bits of THIS
			return this.Length <= other.Length &&
				// verify all our bits exist in OTHER
				((BitSet)other).BitwiseEquals(this, this.Length);
		}
		/// <summary>This set includes all of other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSupersetOf(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			// THIS is a superset of OTHER if it contains the same set bits as OTHER
			// If THIS is shorter, then THIS couldn't contain the bits of OTHER
			return this.Length >= other.Length &&
				// verify all of OTHER's bits exist in THIS
				this.BitwiseEquals((BitSet)other, other.Length);
		}
		/// <summary>This set's bits match 1+ bits in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Overlaps(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			if (ReferenceEquals(other, this))
				return true;

			if (this.Length != 0)
			{
				// NOTE: this algorithm doesn't play nice with auto-aligned arrays where a Bit Operation
				// has tweaked alignment-only data
				var value = (BitSet)other;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				for (int x = 0; x < length_in_words; x++)
				{
					var lhs = this.mArray[x];
					var rhs = value.mArray[x];
					// if each array has similar bits active only.
					// negate to test for matching FALSE-bits
					if ((lhs & rhs) != 0 || (~lhs & ~rhs) != 0)
						return true;
				}
			}

			return false;
		}
		/// <summary>This set's TRUE-bits match 1+ bits in other</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool OverlapsSansZeros(IReadOnlyBitSet other)
		{
			Contract.Assert(other is BitSet, "Only implemented to work with BitSet");

			if (ReferenceEquals(other, this))
				return true;

			if (this.Length != 0)
			{
				// NOTE: this algorithm doesn't play nice with auto-aligned arrays where a Bit Operation
				// has tweaked alignment-only data
				var value = (BitSet)other;
				int length_in_words = Math.Min(this.LengthInWords, value.LengthInWords);
				for (int x = 0; x < length_in_words; x++)
				{
					var lhs = this.mArray[x];
					var rhs = value.mArray[x];
					// if each array has similar bits active only. FALSE-bits are ignored
					if ((lhs & rhs) != 0)
						return true;
				}
			}

			return false;
		}
		#endregion

		#region ICollection<bool> Members
		[NonSerialized] object mSyncRoot;
		public object SyncRoot { get {
			if (this.mSyncRoot == null)
				System.Threading.Interlocked.CompareExchange(ref this.mSyncRoot, new object(), null);
			return this.mSyncRoot;
		} }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int ICollection<bool>.Count					{ get => this.Cardinality; }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int IReadOnlyCollection<bool>.Count			{ get => this.Cardinality; }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int System.Collections.ICollection.Count	{ get => this.Cardinality; }
		public bool IsReadOnly						{ get => false; }
		bool System.Collections.ICollection.IsSynchronized { get => false; }

		void ICollection<bool>.Add(bool item)		=> throw new NotSupportedException();
		bool ICollection<bool>.Contains(bool item)	=> throw new NotSupportedException();
		bool ICollection<bool>.Remove(bool item)	=> throw new NotSupportedException();
		#endregion

		/// <summary>Resizes the underlying storage to the minimal size needed to represent the current <see cref="Length"/></summary>
		public void TrimExcess()
		{
			int length_in_words = this.LengthInWords;

			if (this.mArray.Length > length_in_words)
				Array.Resize(ref this.mArray, length_in_words);
		}

		/// <summary>Set all the bits to zero; doesn't modify <see cref="Length"/></summary>
		[System.Diagnostics.DebuggerStepThrough]
		public void Clear() => this.SetAll(false);

		#region CopyTo
		public void CopyTo(bool[] array, int arrayIndex)
		{
			foreach(var bit in this)
				array[arrayIndex++] = bit;
		}

		void System.Collections.ICollection.CopyTo(Array array, int arrayIndex)
		{
			// #TODO: verify 'array' lengths
			if (array is TWord[])
				Array.Copy(this.mArray, arrayIndex, array, 0, this.LengthInWords);
			else if (array is byte[])
				Buffer.BlockCopy(this.mArray, arrayIndex, array, 0, this.LengthInWords * sizeof(TWord));
			else if (array is bool[])
				this.CopyTo((bool[])array, arrayIndex);
			else
				throw new ArgumentException(string.Format(Util.InvariantCultureInfo, "Array type unsupported {0}", array.GetType()));
		}
		#endregion

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash*23 + this.Length.GetHashCode();
				hash = hash*23 + this.Cardinality.GetHashCode();
				return hash;
			}
		}
		#region IComparable<IReadOnlyBitSet> Members
		public int CompareTo(IReadOnlyBitSet other)
		{
			if (this.Length == other.Length)
				return this.Cardinality - other.Cardinality;

			return this.Length - other.Length;
		}
		#endregion
		#region IEquatable<IReadOnlyBitSet> Members
		public bool Equals(IReadOnlyBitSet other)
		{
			// #TODO: this also needs to check BitwiseEquals
			return this.Length == other.Length && this.Cardinality == other.Cardinality;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			// #TODO: needs to write like byte[] in order to be TWord agnostic
			throw new NotImplementedException();
		}

		public void SerializeWords(IO.EndianStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			bool byte_swap = streamedFormat != Bits.kVectorWordFormat;

			for (int x = 0, word_count = this.LengthInWords; x < word_count; x++)
			{
				if (byte_swap) Bits.BitReverse(ref this.mArray[x]);
				s.Stream(ref this.mArray[x]);
				if (byte_swap) Bits.BitReverse(ref this.mArray[x]);
			}

			if (s.IsReading)
				this.RecalculateCardinality();
		}
		#endregion

		public void SerializeWords(IO.BitStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
		{
			bool byte_swap = streamedFormat != Bits.kVectorWordFormat;

			for (int x = 0, word_count = this.LengthInWords; x < word_count; x++)
			{
				if (byte_swap) Bits.BitReverse(ref this.mArray[x]);
				s.Stream(ref this.mArray[x]);
				if (byte_swap) Bits.BitReverse(ref this.mArray[x]);
			}

			if (s.IsReading)
				this.RecalculateCardinality();
		}

		#region Enum interfaces
		private void ValidateBit<TEnum>(TEnum bit, int bitIndex)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (bitIndex < 0 || bitIndex >= this.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(bit), bit,
					"Enum member is out of range for indexing");
			}
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool Test<TEnum>(TEnum bit)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			this.ValidateBit(bit, bitIndex);

			return this[bitIndex];
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public BitSet Set<TEnum>(TEnum bit, bool value = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			int bitIndex = bit.ToInt32(null);
			this.ValidateBit(bit, bitIndex);

			this.Set(bitIndex, value);
			return this;
		}

		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public string ToString<TEnum>(TEnum maxCount
			, string valueSeperator = TypeExtensions.kDefaultArrayValueSeperator
			, bool stateFilter = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (this.Cardinality == 0)
				return "";

			int maxCountValue = maxCount.ToInt32(null);
			if (maxCountValue < 0 || maxCountValue >= this.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(maxCount), string.Format(Util.InvariantCultureInfo,
					"{0}/{1} is invalid",
					maxCount, maxCountValue));
			}

			if (valueSeperator == null)
				valueSeperator = "";

			var enumType = typeof(TEnum);
			var enumMembers = (TEnum[])Enum.GetValues(enumType);

			// Find the member which represents bit-0
			int memberIndex = 0;
			while (memberIndex < enumMembers.Length && memberIndex < maxCountValue && enumMembers[memberIndex].ToInt32(null) != 0)
				memberIndex++;

			var sb = new System.Text.StringBuilder();
			var bitsInDesiredState = stateFilter
				? this.SetBitIndices
				: this.ClearBitIndices;
			foreach (int bitIndex in bitsInDesiredState)
			{
				if (bitIndex >= maxCountValue)
					break;

				if (sb.Length > 0)
					sb.Append(valueSeperator);

				sb.Append(enumMembers[memberIndex+bitIndex].ToString());
			}

			return sb.ToString();
		}

		/// <summary>Interprets the provided separated strings as Enum members and sets their corresponding bits</summary>
		/// <returns>True if all strings were parsed successfully, false if there were some strings that failed to parse</returns>
		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool TryParseFlags<TEnum>(string line
			, string valueSeperator = TypeExtensions.kDefaultArrayValueSeperator
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			// LINQ stmt allows there to be whitespace around the commas
			return this.TryParseFlags<TEnum>(
				Util.Trim(System.Text.RegularExpressions.Regex.Split(line, valueSeperator)),
				errorsOutput);
		}

		/// <summary>Interprets the provided strings as Enum members and sets their corresponding bits</summary>
		/// <returns>True if all strings were parsed successfully, false if there were some strings that failed to parse</returns>
		/// <typeparam name="TEnum">Members should be bit indices, not literal flag values</typeparam>
		public bool TryParseFlags<TEnum>(IEnumerable<string> collection
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (collection == null)
			{
				return false;
			}

			bool success = true;
			foreach (string flagStr in collection)
			{
				var parsed = this.TryParseFlag<TEnum>(flagStr, errorsOutput);
				if (parsed.HasValue == false)
					continue;
				else if (parsed.Value == false)
					success = false;
			}

			return success;
		}

		private bool? TryParseFlag<TEnum>(string flagStr
			, ICollection<string> errorsOutput = null)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			const bool ignore_case = true;

			// Enum.TryParse will call Trim on the value anyway, so don't add yet another allocation when we can check for whitespace
			if (string.IsNullOrWhiteSpace(flagStr))
				return null;

			if (!Enum.TryParse<TEnum>(flagStr, ignore_case, out TEnum flag))
			{
				if (errorsOutput != null)
				{
					errorsOutput.AddFormat("Couldn't parse '{0}' as a {1} flag",
						flagStr, typeof(TEnum));
				}
				return false;
			}

			int bitIndex = flag.ToInt32(null);
			if (bitIndex < 0 || bitIndex > this.Length)
			{
				if (errorsOutput != null)
				{
					errorsOutput.AddFormat("Member '{0}'={1} in enum {2} can't be used as a bit index",
						flag, bitIndex, typeof(TEnum));
				}
				return false;
			}

			this.Set(bitIndex, true);
			return true;
		}
		#endregion
	};
}
