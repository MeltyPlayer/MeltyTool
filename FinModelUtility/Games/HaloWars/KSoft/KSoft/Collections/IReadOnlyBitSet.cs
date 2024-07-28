using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, IReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	[Contracts.ContractClass(typeof(IReadOnlyBitSetContract))]
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public interface IReadOnlyBitSet
		: ICloneable
		, IReadOnlyCollection<bool>
		, IComparable<IReadOnlyBitSet>
		, IEquatable<IReadOnlyBitSet>
	{
		/// <summary>Returns the "logical size" of the BitSet</summary>
		/// <remarks>IE, the index of the highest addressable bit plus one</remarks>
		int Length { get; }

		/// <summary>Number of bits set to true</summary>
		int Cardinality { get; }
		/// <summary>Number of bits set to false</summary>
		int CardinalityZeros { get; }

		bool IsAllClear { get; }

		/// <summary>Versioning id used to sanity check enumerators</summary>
		int Version { get; }

		#region Access
		bool this[int bitIndex] { get; }
		/// <summary>Tests the states of a range of bits</summary>
		/// <param name="frombitIndex">bit index to start reading from (inclusive)</param>
		/// <param name="toBitIndex">bit index to stop reading at (exclusive)</param>
		/// <returns>True if any bits are set, false if they're all clear</returns>
		/// <remarks>If <paramref name="toBitIndex"/> == <paramref name="frombitIndex"/> this will always return false</remarks>
		bool this[int frombitIndex, int toBitIndex] { get; }

		/// <summary>Get the value of a specific bit</summary>
		/// <param name="bitIndex">Position of the bit</param>
		/// <returns><paramref name="bitIndex"/>'s value in the bit array</returns>
		[SuppressMessage("Microsoft.Design", "CA1716:IdentifiersShouldNotMatchKeywords")]
		bool Get(int bitIndex);

		int NextBitIndex(int startBitIndex, bool stateFilter);
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		int NextClearBitIndex(int startBitIndex = 0);
		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		int NextSetBitIndex(int startBitIndex = 0);

		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		StateFilterEnumeratorWrapper ClearBitIndices { get; }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		StateFilterEnumeratorWrapper SetBitIndices { get; }

		/// <summary>Test to see if any bit is on in a range of bits</summary>
		/// <param name="startBitIndex">Bit index to start testing at</param>
		/// <param name="bitCount">number of bits to test</param>
		/// <returns>True if ANY bit is on (set) in the range, false is they're all off (clear)</returns>
		bool TestBits(int startBitIndex, int bitCount);
		#endregion

		#region ISet-like interfaces
		/// <summary>Determines whether this set is a subset of a specified BitSet (this is included in other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set is a subset of other; otherwise, false</returns>
		/// <remarks>
		/// If <paramref name="other"/> contains the same bits as the current set, the current set is still considered a subset of other.
		///
		/// This method always returns <b>false</b> if the current set has elements that are not in <paramref name="other"/>.
		/// </remarks>
		bool IsSubsetOf(IReadOnlyBitSet other);
		/// <summary>Determines whether this set is a superset of a specified BitSet (this includes all of other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set is a superset of other; otherwise, false</returns>
		/// <remarks>
		/// If <paramref name="other"/> contains the same elements as the current set, the current set is still considered a superset of other.
		///
		/// This method always returns <b>false</b> if the current set has fewer elements than <paramref name="other"/>.
		/// </remarks>
		bool IsSupersetOf(IReadOnlyBitSet other);
		/// <summary>Determines whether this set overlaps with the specified BitSet (this has 1+ bits as other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set and other share at least one common element; otherwise, false</returns>
		/// <remarks>{0, 0}.Overlaps({0, 1}) would thus be true.</remarks>
		bool Overlaps(IReadOnlyBitSet other);
		/// <summary>Determines whether this set's TRUE-bits overlaps with the specified BitSet (this has 1+ bits as other)</summary>
		/// <param name="other">The BitSet to compare to the current set</param>
		/// <returns>true if the current set and other share at least one common TRUE-bit element; otherwise, false</returns>
		/// <remarks>{0, 0}.Overlaps({0, 1}) would thus be false.</remarks>
		bool OverlapsSansZeros(IReadOnlyBitSet other);
//		/// <summary>Determines whether this set and the specified BitSet contain the same elements</summary>
//		/// <param name="other">The BitSet to compare to the current set</param>
//		/// <returns>true if the current set is equal to other; otherwise, false</returns>
//		bool SetEquals(IReadOnlyBitSet other);
		#endregion
	};
	[Contracts.ContractClassFor(typeof(IReadOnlyBitSet))]
	abstract class IReadOnlyBitSetContract : IReadOnlyBitSet
	{
		public int Length { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }

		public int Cardinality { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }
		public int CardinalityZeros { get {
			Contract.Ensures(Contract.Result<int>() >= 0);

			throw new NotImplementedException();
		} }

		public bool IsAllClear { get { throw new NotImplementedException(); } }

		public int Version { get { throw new NotImplementedException(); } }

		#region Access
		public bool this[int bitIndex] { get {
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < this.Length);

			throw new NotImplementedException();
		} }
		public bool this[int frombitIndex, int toBitIndex] { get {
			Contract.Requires<ArgumentOutOfRangeException>(frombitIndex >= 0 && frombitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>(toBitIndex >= frombitIndex && toBitIndex <= this.Length);

			throw new NotImplementedException();
		} }

		public bool Get(int bitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bitIndex >= 0 && bitIndex < this.Length);

			throw new NotImplementedException();
		}

		public int NextBitIndex(int startBitIndex, bool stateFilter)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			throw new NotImplementedException();
		}
		public int NextClearBitIndex(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			throw new NotImplementedException();
		}
		public int NextSetBitIndex(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			throw new NotImplementedException();
		}

		public StateFilterEnumeratorWrapper ClearBitIndices { get { throw new NotImplementedException(); } }
		public StateFilterEnumeratorWrapper SetBitIndices { get { throw new NotImplementedException(); } }

		public bool TestBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			throw new NotImplementedException();
		}
		#endregion

		#region ISet-like interfaces
		public bool IsSubsetOf(IReadOnlyBitSet other)
		{
			Contract.Requires<ArgumentNullException>(other != null);

			throw new NotImplementedException();
		}
		public bool IsSupersetOf(IReadOnlyBitSet other)
		{
			Contract.Requires<ArgumentNullException>(other != null);

			throw new NotImplementedException();
		}
		public bool Overlaps(IReadOnlyBitSet other)
		{
			Contract.Requires<ArgumentNullException>(other != null);

			throw new NotImplementedException();
		}
		public bool OverlapsSansZeros(IReadOnlyBitSet other)
		{
			Contract.Requires<ArgumentNullException>(other != null);

			throw new NotImplementedException();
		}
		#endregion

		#region IReadOnlyCollection<bool> Members
		int IReadOnlyCollection<bool>.Count
		{
			get { throw new NotImplementedException(); }
		}

		IEnumerator<bool> IEnumerable<bool>.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region ICloneable Members
		object ICloneable.Clone()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IComparable<IReadOnlyBitSet> Members
		int IComparable<IReadOnlyBitSet>.CompareTo(IReadOnlyBitSet other)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region IEquatable<IReadOnlyBitSet> Members
		bool IEquatable<IReadOnlyBitSet>.Equals(IReadOnlyBitSet other)
		{
			throw new NotImplementedException();
		}
		#endregion
	};
}
