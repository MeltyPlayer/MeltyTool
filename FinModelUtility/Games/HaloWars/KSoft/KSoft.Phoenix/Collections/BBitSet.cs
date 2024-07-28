using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using Phx = Phoenix.Phx;

	public sealed class BBitSet
	{
		BitSet mBits;

		/// <summary>Is this bitset void of any ON bits?</summary>
		public bool IsEmpty { get {
			return this.mBits == null || this.mBits.IsAllClear;
		} }
		/// <summary>Number of bits in the set, both ON and OFF</summary>
		public int Count { get {
			return this.IsEmpty ? 0 : this.mBits.Length;
		} }
		/// <summary>Number of bits in the set which are ON</summary>
		public int EnabledCount { get {
			return this.IsEmpty ? 0 : this.mBits.Cardinality;
		} }

		public BitSet RawBits { get { return this.mBits; } }

		/// <summary>Parameters that dictate the functionality of this list</summary>
		public BBitSetParams Params { get; private set; }

		public BBitSet(BBitSetParams @params, Phx.BDatabaseBase db = null)
		{
			Contract.Requires<ArgumentNullException>(@params != null);

			this.Params = @params;

			this.InitializeFromEnum(db);
		}

		public void Clear()
		{
			if (this.mBits != null)
				this.mBits.Clear();
		}

		internal void OptimizeStorage()
		{
			if (this.EnabledCount == 0)
				this.mBits = null;
		}

		internal void Set(int bitIndex, bool value = true)
		{
			if (this.mBits == null)
			{
				this.InitializeFromEnum(null);

				if (this.mBits == null)
					throw new InvalidOperationException("Can't use Set on BBitSet that requires BDatabase to initialize");
			}

			this.mBits.Set(bitIndex, value);
		}

		internal IProtoEnum InitializeFromEnum(Phx.BDatabaseBase db)
		{
			IProtoEnum penum = null;

			if (this.Params.kGetProtoEnum != null)
				penum = this.Params.kGetProtoEnum();
			else if (db != null)
				penum = this.Params.kGetProtoEnumFromDB(db);

			if (penum != null)
			{
				if (this.mBits == null)
					this.mBits = new BitSet(penum.MemberCount);
				else
				{
					this.mBits.Clear();

					if (this.mBits.Length != penum.MemberCount)
						this.mBits.Length = penum.MemberCount;
				}

				this.InitializeDefaultValues(penum);
			}

			return penum;
		}

		private void InitializeDefaultValues(IProtoEnum penum)
		{
			if (this.Params.kGetMemberDefaultValue == null)
				return;

			for (int x = 0; x < penum.MemberCount; x++)
			{
				bool bitDefault = this.Params.kGetMemberDefaultValue(x);
				if (bitDefault)
					this.mBits[x] = true;
			}
		}

		public bool this[int bit_index]
		{
			get { return this.IsEmpty ? false : this.mBits[bit_index]; }
			set
			{
				if (!this.IsEmpty)
					this.mBits[bit_index] = value;
			}
		}
	};
}