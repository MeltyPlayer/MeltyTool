using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	using BCost = Collections.BTypeValuesSingle;
	using BProtoObjectVeterancyList = Collections.BListExplicitIndex<BProtoObjectVeterancy>;

	partial class BDatabaseBase
	{
		HashSet<BCost> mPoolCosts;
		//HashSet<BPops> mPoolPops;
		HashSet<BProtoObjectVeterancyList> mPoolVeterancies;

		void InitializeValuePools()
		{
			mPoolCosts = new HashSet<BCost>();

			mPoolVeterancies = new HashSet<BProtoObjectVeterancyList>();
		}

		public bool InternTypeValues<T>(ref Collections.BTypeValuesBase<T> values)
		{
			return false;
		}
	};
}
