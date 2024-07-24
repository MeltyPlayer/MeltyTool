using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using StateFilterEnumerator = IReadOnlyBitSetEnumerators.StateFilterEnumerator;

	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public sealed class EnumBitSet<TEnum>
		: ICollection<TEnum>, System.Collections.ICollection
		, IComparable<EnumBitSet<TEnum>>, IEquatable<EnumBitSet<TEnum>>
		, IO.IEndianStreamSerializable
		where TEnum : struct, IComparable, IFormattable, IConvertible
	{
		static readonly Func<int, TEnum> FromInt32 = Reflection.EnumValue<TEnum>.FromInt32;
		static readonly Func<TEnum, int> ToInt32 = Reflection.EnumValue<TEnum>.ToInt32;

		static readonly int kBitSetLength = EnumBitEncoder32<TEnum>.kBitCount;

		readonly BitSet mBits;
		readonly TEnum mInvalidSentinelValue;

		/// <summary>Returns the "logical size" of the BitSet</summary>
		public int Length			{ get => kBitSetLength; }
		/// <summary>Number of bits set to true</summary>
		public int Cardinality		{ get => mBits.Cardinality; }
		/// <summary>Number of bits set to false</summary>
		public int CardinalityZeros	{ get => mBits.CardinalityZeros; }

		/// <summary>Member or value to use when an operation results in an invalid value (eg, NextSetBit)</summary>
		public TEnum InvalidSentinelValue { get => mInvalidSentinelValue; }

		#region Ctor
		static string CtorExceptionMsgTEnumIsFlags { get =>
			string.Format(Util.InvariantCultureInfo,
				"Tried to use a Flags enum in an EnumBitSet - {0}",
				Reflection.EnumUtil<TEnum>.EnumType.Name);
		}
		static string CtorExceptionMsgTEnumHasNone { get =>
			string.Format(Util.InvariantCultureInfo,
				"Tried to use a Enum with a NONE member in an EnumBitSet - {0}",
				Reflection.EnumUtil<TEnum>.EnumType.Name);
		}
		/// <summary></summary>
		/// <param name="invalidSentinelValue">Member or value to use when an operation results in an invalid value (eg, NextSetBit)</param>
		public EnumBitSet(TEnum invalidSentinelValue = default(TEnum))
		{
			Contract.Requires<ArgumentException>(!Reflection.EnumUtil<TEnum>.IsFlags, CtorExceptionMsgTEnumIsFlags);
			Contract.Requires<ArgumentException>(!EnumBitEncoder32<TEnum>.kHasNone, CtorExceptionMsgTEnumHasNone);

			mBits = new BitSet(kBitSetLength);
			mInvalidSentinelValue = invalidSentinelValue;
		}
		#endregion

		#region Access
		public bool this[TEnum bitIndex]
		{
			get {
				int actual_index = ToInt32(bitIndex);
				return mBits[actual_index];
			}
			set {
				int actual_index = ToInt32(bitIndex);
				mBits[actual_index] = value;
			}
		}

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		public bool Get(TEnum bitIndex)
		{
			int actual_index = ToInt32(bitIndex);
			return mBits[actual_index];
		}
		/// <summary>Set the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <param name="value">New value of the bit</param>
		public void Set(TEnum bitIndex, bool value)
		{
			int actual_index = ToInt32(bitIndex);
			mBits[actual_index] = value;
		}

		/// <summary>Flip the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		public void Toggle(TEnum bitIndex)
		{
			int actual_index = ToInt32(bitIndex);
			mBits.Toggle(actual_index);
		}

		public void SetAll(bool value) => mBits.SetAll(value);

		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		public int NextClearBitIndex(TEnum startBitIndex)
		{
			int actual_index = ToInt32(startBitIndex);
			return mBits.NextClearBitIndex(actual_index);
		}
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		public int NextSetBitIndex(TEnum startBitIndex)
		{
			int actual_index = ToInt32(startBitIndex);
			return mBits.NextSetBitIndex(actual_index);
		}

		/// <summary>Get the enum member of the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The enum member whose bit is 0, or <see cref="InvalidSentinelValue"/> there isn't another one</returns>
		public TEnum NextClearBit(TEnum startBitIndex)
		{
			int bit_index = NextClearBitIndex(startBitIndex);
			return bit_index.IsNotNone()
				? FromInt32(bit_index)
				: mInvalidSentinelValue;
		}
		/// <summary>Get the enum member of the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The enum member whose bit is 0, or <see cref="InvalidSentinelValue"/> there isn't another one</returns>
		public TEnum NextSetBit(TEnum startBitIndex)
		{
			int bit_index = NextSetBitIndex(startBitIndex);
			return bit_index.IsNotNone()
				? FromInt32(bit_index)
				: mInvalidSentinelValue;
		}

		/// <summary>Enumeration of enum members whose bits are 0 (clear)</summary>
		public EnumeratorWrapper<TEnum, EnumeratorBitState> ClearBitIndices { get =>
			new EnumeratorWrapper<TEnum, EnumeratorBitState>(
				new EnumeratorBitState(mBits.ClearBitIndices.GetEnumerator()));
		}
		/// <summary>Enumeration of enum members whose bits are 1 (set)</summary>
		public EnumeratorWrapper<TEnum, EnumeratorBitState> SetBitIndices { get =>
			new EnumeratorWrapper<TEnum, EnumeratorBitState>(
				new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator()));
		}

		public EnumeratorBitState GetEnumerator() => new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator());
		IEnumerator<TEnum> IEnumerable<TEnum>.GetEnumerator() => new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator());
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new EnumeratorBitState(mBits.SetBitIndices.GetEnumerator());
		#endregion

		#region Bit Operations
		/// <summary>Bit AND this set with another</summary>
		/// <param name="value">Set with the bits to AND with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> And(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.And(value.mBits);
			return this;
		}
		/// <summary>Clears all of the bits in this set whose corresponding bit is set in the specified BitSet</summary>
		/// <param name="value">set the BitSet with which to mask this BitSet</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> AndNot(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.AndNot(value.mBits);
			return this;
		}
		/// <summary>Bit OR this set with another</summary>
		/// <param name="value">Set with the bits to OR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Or(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.Or(value.mBits);
			return this;
		}
		/// <summary>Bit XOR this set with another</summary>
		/// <param name="value">Set with the bits to XOR with</param>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Xor(EnumBitSet<TEnum> value)
		{
			Contract.Requires<ArgumentNullException>(value != null);

			mBits.Xor(value.mBits);
			return this;
		}

		/// <summary>Inverts all bits in this set</summary>
		/// <returns>Returns the current instance</returns>
		public EnumBitSet<TEnum> Not()
		{
			mBits.Not();
			return this;
		}
		#endregion

		#region ICollection<TEnum> Members
		// We return mBits' since we're the only ones who can ever touch it
		public object SyncRoot { get => mBits.SyncRoot; }
		bool System.Collections.ICollection.IsSynchronized { get => false; }

		public void Add(TEnum item)					=> Set(item, true);
		public void Clear()							=> mBits.Clear();
		public bool Contains(TEnum item)			=> Get(item);

		void ICollection<TEnum>.CopyTo(TEnum[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(TEnum item)
		{
			bool existed = Get(item);
			Set(item, false);

			return existed;
		}

		/// <summary>returns <see cref="Cardinality"/></summary>
		int ICollection<TEnum>.Count				{ get => Cardinality; }
		/// <summary>returns <see cref="Cardinality"/></summary>
		int System.Collections.ICollection.Count	{ get => Cardinality; }
		bool ICollection<TEnum>.IsReadOnly			{ get => true; }

		public void CopyTo(Array array, int arrayIndex) => (mBits as System.Collections.ICollection).CopyTo(array, arrayIndex);
		#endregion

		public void CopyTo(bool[] array, int arrayIndex) => mBits.CopyTo(array, arrayIndex);

		public override bool Equals(object obj)
		{
			if (obj is EnumBitSet<TEnum> o)
			{
				return this.Equals(o);
			}

			return false;
		}

		public override int GetHashCode()				=> mBits.GetHashCode();
		#region IComparable<EnumBitSet<TEnum>> Members
		public int CompareTo(EnumBitSet<TEnum> other)	=> mBits.CompareTo(other.mBits);
		#endregion
		#region IEquatable<EnumBitSet<TEnum>> Members
		public bool Equals(EnumBitSet<TEnum> other)		=> mBits.Equals(other.mBits);
		#endregion

		public struct EnumeratorBitState
			: IEnumerator<TEnum>
		{
			StateFilterEnumerator mEnumerator;

			public EnumeratorBitState(StateFilterEnumerator bitStateEnumerator)
			{
				mEnumerator = bitStateEnumerator;
			}

			public TEnum Current		{ get => FromInt32(mEnumerator.Current); }
			object System.Collections.IEnumerator.Current { get => this.Current; }

			public bool MoveNext()		=> mEnumerator.MoveNext();
			public void Reset()			=> mEnumerator.Reset();

			public void Dispose()		=> mEnumerator.Dispose();
		};

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s) => mBits.Serialize(s);

		public void SerializeWords(IO.EndianStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat) => mBits.SerializeWords(s, streamedFormat);
		#endregion

		public void SerializeWords(IO.BitStream s, Shell.EndianFormat streamedFormat = Bits.kVectorWordFormat)
			=> mBits.SerializeWords(s, streamedFormat);
	};
}
