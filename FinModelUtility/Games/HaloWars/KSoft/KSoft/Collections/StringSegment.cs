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
	using StringSegmentEnumerator = StringSegment.Enumerator;

	// #TODO how is this better or different compared to StringSegment Microsoft.Extensions.Primitives.dll?
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial struct StringSegment
		: IReadOnlyList<char>
		, IEquatable<StringSegment>
		, IList<char>
	{
		readonly string mData;
		public string Data { get { return this.mData; } }
		readonly int mOffset;
		public int Offset { get { return this.mOffset; } }
		readonly int mCount;
		public int Count { get { return this.mCount; } }

		#region Ctor
		public StringSegment(string data)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			this.mData = data;
			this.mOffset = 0;
			this.mCount = data.Length;
		}
		public StringSegment(string data, int offset, int count)
		{
			Contract.Requires<ArgumentNullException>(data != null);
			Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentException>(count < (data.Length - offset));

			this.mData = data;
			this.mOffset = offset;
			this.mCount = count;
		}
		#endregion

		void VerifyData()
		{
			if (this.mData == null)
				throw new InvalidOperationException("String data is null");
		}

		public int IndexOf(char value)
		{
			this.VerifyData();

			int index = this.mData.IndexOf(value, this.mOffset, this.mCount);
			if (index < 0)
				return TypeExtensions.kNone;

			return index - this.mOffset;
		}

		public bool Contains(char value)
		{
			this.VerifyData();

			return this.mData.IndexOf(value, this.mOffset, this.mCount) >= 0;
		}

		public void CopyTo(char[] array, int arrayIndex)
		{
			this.VerifyData();

			this.mData.CopyTo(this.mOffset, array, arrayIndex, this.mCount);
		}

		#region IReadOnlyList<char> Members
		public char this[int index] { get {
			this.VerifyData();

			return this.mData[this.mOffset + index];
		} }

		char IList<char>.this[int index] {
			get { return this[index]; }
			set { throw new NotImplementedException(); }
		}
		#endregion

		#region IEnumerable<char> Members
		public StringSegmentEnumerator GetEnumerator()
		{
			this.VerifyData();

			return new StringSegmentEnumerator(this);
		}
		IEnumerator<char> IEnumerable<char>.GetEnumerator()
		{ return this.GetEnumerator(); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{ return this.GetEnumerator(); }
		#endregion

		#region NotImplemented IList<char> Members
		void IList<char>.Insert(int index, char item) { throw new NotImplementedException(); }
		void IList<char>.RemoveAt(int index) { throw new NotImplementedException(); }
		#endregion

		#region NotImplemented ICollection<char> Members
		void ICollection<char>.Add(char item) { throw new NotImplementedException(); }
		void ICollection<char>.Clear() { throw new NotImplementedException(); }
		bool ICollection<char>.IsReadOnly { get { return true; } }
		bool ICollection<char>.Remove(char item) { throw new NotImplementedException(); }
		#endregion

		#region Equatable Members
		public bool Equals(StringSegment other)
		{
			return other.mData == this.mData && other.mOffset == this.mOffset && other.mCount == this.mCount;
		}
		public override bool Equals(object obj)
		{
			return obj is StringSegment && this.Equals((StringSegment)obj);
		}

		public static bool operator ==(StringSegment lhs, StringSegment rhs)
		{
			return lhs.Equals(rhs);
		}
		public static bool operator !=(StringSegment lhs, StringSegment rhs)
		{
			return !lhs.Equals(rhs);
		}
		#endregion

		public override int GetHashCode()
		{
			if (this.mData != null)
				return (this.mData.GetHashCode() ^ this.mOffset) ^ this.mCount;

			return 0;
		}
	};
}
