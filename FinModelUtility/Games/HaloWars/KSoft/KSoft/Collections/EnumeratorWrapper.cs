using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KSoft
{
	/// <summary>Wraps an IEnumerator object so it can be used in an foreach expression</summary>
	/// <typeparam name="T"></typeparam>
	/// <remarks>
	/// Use this lightweight wrapper (it's a value type) when a type already has a provided IEnumerable
	/// implementation, but the type has other data/properties which can be enumerated with a foreach.
	/// </remarks>
	/// <see cref="Collections.BitSet.ClearBitIndices"/>
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct EnumeratorWrapper<T>
		: IEnumerable<T>
	{
		readonly IEnumerator<T> mEnumerator;

		public EnumeratorWrapper(IEnumerator<T> enumerator) {
			this.mEnumerator = enumerator; }
		public EnumeratorWrapper(IEnumerable<T> enumerable) {
			this.mEnumerator = enumerable.GetEnumerator(); }

		#region IEnumerable<T> Members
		public IEnumerator<T> GetEnumerator() { return this.mEnumerator; }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.mEnumerator; }
		#endregion
	};

	/// <summary>Wraps an IEnumerator value type so it can be used in an foreach expression</summary>
	/// <typeparam name="T">Type which is enumerated</typeparam>
	/// <typeparam name="TEnumerator">The value-type which implements IEnumerable</typeparam>
	/// <remarks>
	/// Use this lightweight wrapper (it's a value type) when a type already has a provided IEnumerable
	/// implementation, but the type has other data/properties which can be enumerated with a foreach.
	/// </remarks>
	/// <see cref="Collections.BitSet.ClearBitIndices"/>
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct EnumeratorWrapper<T, TEnumerator>
		: IEnumerable<T>
		where TEnumerator : struct, IEnumerator<T>
	{
		readonly TEnumerator mEnumerator;

		public EnumeratorWrapper(TEnumerator enumerator) {
			this.mEnumerator = enumerator; }

		public TEnumerator GetEnumerator() { return this.mEnumerator; }

		#region IEnumerable<T> Members
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return this.mEnumerator; }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.mEnumerator; }
		#endregion
	};
}
