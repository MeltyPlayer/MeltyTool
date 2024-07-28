using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	public class BListExplicitIndexParams<T>
		: BListParams
	{
		public IComparer<T> kComparer = Comparer<T>.Default;

		/// <summary>Get the 'invalid' value for a value</summary>
		public Func<T> kTypeGetInvalid = () => default(T);

		public BListExplicitIndexParams() { }

		public BListExplicitIndexParams(int initialCapacity = TypeExtensions.kNone) : base()
		{
			this.Flags.Clear();
			if (initialCapacity > 0)
				this.InitialCapacity = initialCapacity;
		}
	};
}