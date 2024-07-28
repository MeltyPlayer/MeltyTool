using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public static partial class IReadOnlyBitSetEnumerators
	{
		public partial struct StateEnumerator
		{
			public StateEnumerator(IReadOnlyBitSet bitset)
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
			}

			public bool MoveNext()
			{
				if (this.mBitIndex < this.mLastIndex)
				{
					this.mCurrent = this.mSet.Get(++this.mBitIndex);
					return true;
				}

				this.mBitIndex = this.mSet.Length;
				return false;
			}
		};

		public partial struct StateFilterEnumerator
		{
			public StateFilterEnumerator(IReadOnlyBitSet bitset, bool stateFilter, int startBitIndex = 0)
				: this(bitset, false)
			{
				Contract.Requires<ArgumentNullException>(bitset != null);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < bitset.Length || bitset.Length == 0);

				this.mStateFilter = stateFilter;
				this.mStartBitIndex = startBitIndex-1;
			}

			public bool MoveNext()
			{
				if (this.mBitIndex.IsNone())
					this.mBitIndex = this.mStartBitIndex;

				if (this.mBitIndex < this.mLastIndex)
				{
					this.mCurrent = this.mSet.NextBitIndex(++this.mBitIndex, this.mStateFilter);

					if (this.mCurrent >= 0)
					{
						this.mBitIndex = this.mCurrent;
						return true;
					}
				}

				this.mBitIndex = this.mSet.Length;
				return false;
			}
		};
	};
}