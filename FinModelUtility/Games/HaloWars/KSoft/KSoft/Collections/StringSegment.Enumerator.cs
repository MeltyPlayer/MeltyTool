using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	partial struct StringSegment
	{
		[Serializable]
		public struct Enumerator
			: IEnumerator<char>
		{
			readonly string mData;
			readonly int mStart, mEnd;
			int mCurrent;

			public Enumerator(StringSegment segment)
			{
				this.mData = segment.mData;
				this.mStart = segment.mOffset;
				this.mEnd = this.mStart + segment.mCount;
				this.mCurrent = this.mStart - 1;
			}
			public void Dispose() { }

			#region IEnumerator<char> Members
			public char Current { get {
				if (this.mCurrent < this.mStart)	throw new InvalidOperationException("Enumeration has not started");
				if (this.mCurrent >= this.mEnd)	throw new InvalidOperationException("Enumeration already finished");

				return this.mData[this.mCurrent];
			} }
			object System.Collections.IEnumerator.Current { get { return this.Current; } }

			public bool MoveNext()
			{
				if (this.mCurrent < this.mEnd)
				{
					++this.mCurrent;
					return this.mCurrent < this.mEnd;
				}
				return false;
			}

			public void Reset()
			{
				this.mCurrent = this.mStart - 1;
			}
			#endregion
		};
	};
}