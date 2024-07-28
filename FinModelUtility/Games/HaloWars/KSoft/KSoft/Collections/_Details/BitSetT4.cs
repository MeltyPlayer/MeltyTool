using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using StateFilterEnumerator = IReadOnlyBitSetEnumerators.StateFilterEnumerator;

	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	partial class BitSet
	{
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		[Contracts.Pure]
		public int NextClearBitIndex(int startBitIndex = 0)
		{
			return this.NextBitIndex(startBitIndex, false);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		public StateFilterEnumeratorWrapper ClearBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, false));
		} }
		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		public StateFilterEnumeratorWrapper ClearBitIndicesStartingAt(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, false, startBitIndex));
		}

		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		[Contracts.Pure]
		public int NextSetBitIndex(int startBitIndex = 0)
		{
			return this.NextBitIndex(startBitIndex, true);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		public StateFilterEnumeratorWrapper SetBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, true));
		} }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		public StateFilterEnumeratorWrapper SetBitIndicesStartingAt(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, true, startBitIndex));
		}


		public void ClearBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = kVectorElementSectionBitMask(startBitIndex);
			var last_word_mask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			// target bits are only in one word...
			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Remove(ref this.mArray[from_word_index], mask);
				this.RecalculateCardinalityRound(from_word_index);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(from_word_index);
			Bitwise.Flags.Remove(ref this.mArray[from_word_index], from_word_mask);
			this.RecalculateCardinalityRound(from_word_index);

			// handle any words in between
			for (int x = from_word_index+1; x < last_word_index; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				this.mArray[x] = kWordAllBitsClear;
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(last_word_index);
			Bitwise.Flags.Remove(ref this.mArray[last_word_index], last_word_mask);
			this.RecalculateCardinalityRound(last_word_index);
		}

		public void SetBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = kVectorElementSectionBitMask(startBitIndex);
			var last_word_mask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			// target bits are only in one word...
			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Add(ref this.mArray[from_word_index], mask);
				this.RecalculateCardinalityRound(from_word_index);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(from_word_index);
			Bitwise.Flags.Add(ref this.mArray[from_word_index], from_word_mask);
			this.RecalculateCardinalityRound(from_word_index);

			// handle any words in between
			for (int x = from_word_index+1; x < last_word_index; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				this.mArray[x] = kWordAllBitsSet;
				this.Cardinality += kWordBitCount;
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(last_word_index);
			Bitwise.Flags.Add(ref this.mArray[last_word_index], last_word_mask);
			this.RecalculateCardinalityRound(last_word_index);
		}

		public void ToggleBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var from_word_mask = kVectorElementSectionBitMask(startBitIndex);
			var last_word_mask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			// target bits are only in one word...
			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(from_word_index);
				Bitwise.Flags.Toggle(ref this.mArray[from_word_index], mask);
				this.RecalculateCardinalityRound(from_word_index);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(from_word_index);
			Bitwise.Flags.Toggle(ref this.mArray[from_word_index], from_word_mask);
			this.RecalculateCardinalityRound(from_word_index);

			// handle any words in between
			for (int x = from_word_index+1; x < last_word_index; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				Bitwise.Flags.Toggle(ref this.mArray[x], this.mArray[x]);
				this.RecalculateCardinalityRound(x);
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(last_word_index);
			Bitwise.Flags.Toggle(ref this.mArray[last_word_index], last_word_mask);
			this.RecalculateCardinalityRound(last_word_index);
		}

		[Contracts.Pure]
		public bool TestBits(int startBitIndex, int bitCount)
		{
			if (bitCount <= 0)
				return false;

			var from_word_mask = kVectorElementSectionBitMask(startBitIndex);
			var last_word_mask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int last_bit_index = (startBitIndex+bitCount) - 1;
			var from_word_index = kVectorIndexInT(startBitIndex);
			var last_word_index = kVectorIndexInT(last_bit_index);

			// target bits are only in one word...
			if (from_word_index == last_word_index)
			{
				var mask = from_word_mask;// & last_word_mask;
				return Bitwise.Flags.TestAny(this.mArray[from_word_index], mask);
			}
			// or the target bits are in multiple words...

			// handle the first word
			if (Bitwise.Flags.TestAny(this.mArray[from_word_index], from_word_mask))
				return true;

			// handle any words in between
			for (int x = from_word_index+1; x < last_word_index; x++)
			{
				if (this.mArray[x] > kWordAllBitsClear)
					return true;
			}

			// handle the last word
			return Bitwise.Flags.TestAny(this.mArray[last_word_index], last_word_mask);
		}

	};
}