using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public sealed class BTypeValuesSingle
		: BTypeValuesBase<float>
	{
		public BTypeValuesSingle(BTypeValuesParams<float> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}

		public bool HasNonZeroItems { get {
			for (int x = 0; x < this.Count; x++)
				if (this[x] != 0.0f)
					return true;

			return false;
		} }
	};
}