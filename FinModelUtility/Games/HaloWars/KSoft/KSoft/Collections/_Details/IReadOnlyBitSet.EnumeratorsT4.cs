using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Collections
{
	static partial class IReadOnlyBitSetEnumerators
	{
		[Serializable]
		partial struct StateEnumerator
			: IEnumerator< bool >
		{
			readonly IReadOnlyBitSet mSet;
			readonly int mLastIndex;
			readonly int mVersion;
			int mBitIndex;
			bool mCurrent;

			StateEnumerator(IReadOnlyBitSet bitset,
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				bool dummy)
				: this()
			{
				this.mSet = bitset;
				this.mLastIndex = bitset.Length - 1;
				this.mVersion = bitset.Version;
				this.mBitIndex = TypeExtensions.kNone;
			}

			void VerifyVersion()
			{
				if (this.mVersion != this.mSet.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public bool Current { get {
				if (this.mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (this.mBitIndex > this.mLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return this.mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get => this.Current; }

			public void Reset()
			{
				this.VerifyVersion();
				this.mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }
		};

		[Serializable]
		partial struct StateFilterEnumerator
			: IEnumerator< int >
		{
			readonly IReadOnlyBitSet mSet;
			readonly int mLastIndex;
			readonly int mVersion;
			int mBitIndex;
			int mCurrent;
			// defined here to avoid: CS0282: There is no defined ordering between fields in multiple declarations of partial class or struct
			readonly bool mStateFilter;
			readonly int mStartBitIndex;

			StateFilterEnumerator(IReadOnlyBitSet bitset,
				[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
				bool dummy)
				: this()
			{
				this.mSet = bitset;
				this.mLastIndex = bitset.Length - 1;
				this.mVersion = bitset.Version;
				this.mBitIndex = TypeExtensions.kNone;
			}

			void VerifyVersion()
			{
				if (this.mVersion != this.mSet.Version)
					throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			public int Current { get {
				if (this.mBitIndex.IsNone())			throw new InvalidOperationException("Enumeration has not started");
				if (this.mBitIndex > this.mLastIndex)		throw new InvalidOperationException("Enumeration already finished");

				return this.mCurrent;
			} }
			object System.Collections.IEnumerator.Current { get => this.Current; }

			public void Reset()
			{
				this.VerifyVersion();
				this.mBitIndex = TypeExtensions.kNone;
			}

			public void Dispose()	{ }
		};

	};
}
