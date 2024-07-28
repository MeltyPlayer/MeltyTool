using System;
using System.Collections.Generic;

namespace KSoft.Memory.Strings
{
	partial class StringMemoryPool
	{
		/// <summary>
		/// Enumerates the strings in a <see cref="StringMemoryPool"/> in key-value pairs
		/// with the <b>address</b> being the <i>key</i> and the <b>string</b> being the <i>value</i>
		/// </summary>
		protected struct KeyValueEnumerator : IEnumerator<KeyValuePair<Values.PtrHandle, string>>
		{
			const int kBlankIndexState = -2;

			StringMemoryPool mPool;
			int mCurrentIndex;

			public KeyValueEnumerator(StringMemoryPool pool)
			{
				this.mPool = pool;
				this.mCurrentIndex = kBlankIndexState;
				this.mCurrent = new KeyValuePair<Values.PtrHandle, string>(Values.PtrHandle.Null32, null);
			}

			#region IEnumerator<T> Members
			KeyValuePair<Values.PtrHandle, string> mCurrent;
			/// <summary>Get the current element in the enumeration</summary>
			public KeyValuePair<Values.PtrHandle, string> Current { get { return this.mCurrent; } }
			#endregion

			#region IDisposable Members
			void IDisposable.Dispose() { }
			#endregion

			#region IEnumerator Members
			/// <summary>Get the current element in the enumeration</summary>
			object System.Collections.IEnumerator.Current { get { return this.mCurrent; } }

			/// <summary>Advances the enumerator to the next address\string pair</summary>
			/// <returns></returns>
			public bool MoveNext()
			{
				// for supporting state Resets
				if (this.mCurrentIndex == kBlankIndexState)
					this.mCurrentIndex = 0;

				if (this.mCurrentIndex >= 0 && this.mCurrentIndex < this.mPool.Count)
				{
					this.mCurrent = new KeyValuePair<Values.PtrHandle, string>(this.mPool.mReferences[this.mCurrentIndex], this.mPool.mPool[this.mCurrentIndex]);

					this.mCurrentIndex++;
				}
				// when we've past the end of the pool
				else
					this.mCurrentIndex = TypeExtensions.kNone;

				return this.mCurrentIndex >= 0 && this.mCurrentIndex < this.mPool.Count;
			}

			public void Reset() {
				this.mCurrentIndex = kBlankIndexState; }
			#endregion
		};
	};
}