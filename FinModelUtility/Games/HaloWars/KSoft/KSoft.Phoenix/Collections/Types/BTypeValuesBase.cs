using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public abstract class BTypeValuesBase<T>
		: BListExplicitIndexBase<T>
	{
		internal BTypeValuesParams<T> TypeValuesParams { get { return this.Params as BTypeValuesParams<T>; } }

		protected BTypeValuesBase(BTypeValuesParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}