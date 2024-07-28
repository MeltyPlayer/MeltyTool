using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	/// <summary>
	/// Our base interface for lists of comparable objects, whose elements occupy an explicit index
	/// </summary>
	/// <typeparam name="T">Comparble object's type</typeparam>
	public abstract class BListExplicitIndexBase<T>
		: BListBase<T>
	{
		internal BListExplicitIndexParams<T> ExplicitIndexParams { get { return this.Params as BListExplicitIndexParams<T>; } }

		protected BListExplicitIndexBase(BListExplicitIndexParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}

		/// <summary>
		/// If the new count is greater than <see cref="Count"/>, adds new elements up-to <paramref name="new_count"/>,
		/// using the "invalid value" defined in the list params
		/// </summary>
		/// <param name="newCount"></param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="newCount"/> is less than <see cref="Count"/></exception>
		internal void ResizeCount(int newCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(newCount >= this.Count,
				"For resizing to a smaller Count, use Capacity.");

			var eip = this.ExplicitIndexParams;

			for (int x = this.Count; x < newCount; x++)
				this.AddItem(eip.kTypeGetInvalid());
		}

		public override void Clear()
		{
			int original_count = this.Count;
			base.Clear();

			this.ResizeCount(original_count);
		}

		internal void InitializeItem(int index)
		{
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);

			var eip = this.ExplicitIndexParams;

			if (index >= this.Count)
			{
				// expand the list up-to the requested index
				for (int x = this.Count; x <= index; x++)
					this.AddItem(eip.kTypeGetInvalid());
			}
			else
				base[index] = eip.kTypeGetInvalid();
		}
	};
}