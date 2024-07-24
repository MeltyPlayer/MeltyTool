using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	partial class Flags
	{
		#region Test
		#region 8-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(byte value, byte flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(sbyte value, sbyte flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if any bits in <paramref name="flag"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool TestAny(byte value, byte flag)
		{
			return (value & flag) != 0;
		}

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(byte value, params byte[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if(!Test(value, i))
					return false;

			return true;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(byte value, params byte[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if (Test(value, i))
					return true;

			return false;
		}
		#endregion
		#region 16-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(ushort value, ushort flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(short value, short flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if any bits in <paramref name="flag"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool TestAny(ushort value, ushort flag)
		{
			return (value & flag) != 0;
		}

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(ushort value, params ushort[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if(!Test(value, i))
					return false;

			return true;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(ushort value, params ushort[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if (Test(value, i))
					return true;

			return false;
		}
		#endregion
		#region 32-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(uint value, uint flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(int value, int flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if any bits in <paramref name="flag"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool TestAny(uint value, uint flag)
		{
			return (value & flag) != 0;
		}

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(uint value, params uint[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if(!Test(value, i))
					return false;

			return true;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(uint value, params uint[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if (Test(value, i))
					return true;

			return false;
		}
		#endregion
		#region 64-bit
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(ulong value, ulong flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if <paramref name="flag"/> is active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) == <paramref name="flag"/></returns>
		[Contracts.Pure]
		public static bool Test(long value, long flag)
		{
			return (value & flag) == flag;
		}
		/// <summary>Returns true if any bits in <paramref name="flag"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flag">Value to test for</param>
		/// <returns>(<paramref name="value"/> &amp; <paramref name="flag"/>) != 0</returns>
		[Contracts.Pure]
		public static bool TestAny(ulong value, ulong flag)
		{
			return (value & flag) != 0;
		}

		/// <summary>Returns true if all the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value">Value to test in</param>
		/// <param name="flags">Values to test for</param>
		/// <returns>Returns true if ALL the flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool Test(ulong value, params ulong[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if(!Test(value, i))
					return false;

			return true;
		}

		/// <summary>Returns true if any one of the flags in <paramref name="flags"/> are active in <paramref name="value"/></summary>
		/// <param name="value"></param>
		/// <param name="flags"></param>
		/// <returns>Returns true if any (one, some, or all) flag values in <paramref name="flags"/> are set in <paramref name="value"/></returns>
		[Contracts.Pure]
		public static bool TestAny(ulong value, params ulong[] flags)
		{
			Contract.Requires(flags != null);

			foreach (var i in flags)
				if (Test(value, i))
					return true;

			return false;
		}
		#endregion
		#endregion

		#region Add
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		public static uint Add(uint lhs, uint rhs)
		{
			return lhs | rhs;
		}
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector reference</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		public static void Add(ref uint lhs, uint rhs)
		{
			lhs |= rhs;
		}

		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		public static ulong Add(ulong lhs, ulong rhs)
		{
			return lhs | rhs;
		}
		/// <summary>Adds <paramref name="rhs"/> to <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector reference</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to add to <paramref name="lhs"/></param>
		public static void Add(ref ulong lhs, ulong rhs)
		{
			lhs |= rhs;
		}

		#endregion

		#region Remove
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> AND-EQUALS ~<paramref name="rhs"/></returns>
		public static uint Remove(uint lhs, uint rhs)
		{
			return lhs & ~rhs;
		}
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		public static void Remove(ref uint lhs, uint rhs)
		{
			lhs &= ~rhs;
		}

		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> AND-EQUALS ~<paramref name="rhs"/></returns>
		public static ulong Remove(ulong lhs, ulong rhs)
		{
			return lhs & ~rhs;
		}
		/// <summary>Removes <paramref name="rhs"/> from <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to remove from <paramref name="lhs"/></param>
		public static void Remove(ref ulong lhs, ulong rhs)
		{
			lhs &= ~rhs;
		}

		#endregion

		#region Toggle
		/// <summary>Complements <paramref name="rhs"/> bits in <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to complement in <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> XOR-EQUALS <paramref name="rhs"/></returns>
		public static uint Toggle(uint lhs, uint rhs)
		{
			return lhs ^ rhs;
		}
		/// <summary>Complements <paramref name="rhs"/> bits in <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to complement in <paramref name="lhs"/></param>
		public static void Toggle(ref uint lhs, uint rhs)
		{
			lhs ^= rhs;
		}

		/// <summary>Complements <paramref name="rhs"/> bits in <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to complement in <paramref name="lhs"/></param>
		/// <returns><paramref name="lhs"/> XOR-EQUALS <paramref name="rhs"/></returns>
		public static ulong Toggle(ulong lhs, ulong rhs)
		{
			return lhs ^ rhs;
		}
		/// <summary>Complements <paramref name="rhs"/> bits in <paramref name="lhs"/></summary>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to complement in <paramref name="lhs"/></param>
		public static void Toggle(ref ulong lhs, ulong rhs)
		{
			lhs ^= rhs;
		}

		#endregion

		#region Modify
		#region 8-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		///
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static byte Modify(bool addOrRemove, byte lhs, byte rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs :
				lhs &= (byte)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref byte lhs, byte rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (byte)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 16-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		///
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static ushort Modify(bool addOrRemove, ushort lhs, ushort rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs :
				lhs &= (ushort)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref ushort lhs, ushort rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (ushort)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 32-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		///
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static uint Modify(bool addOrRemove, uint lhs, uint rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs :
				lhs &= (uint)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref uint lhs, uint rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (uint)~rhs;

			return addOrRemove;
		}
		#endregion
		#region 64-bit
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns>
		/// If <paramref name="addOrRemove"/> is True:
		/// <paramref name="lhs"/> |= <paramref name="rhs"/>
		///
		/// Else:
		/// <paramref name="lhs"/> &amp;= <paramref name="rhs"/>
		/// </returns>
		public static ulong Modify(bool addOrRemove, ulong lhs, ulong rhs)
		{
			return (addOrRemove == true ?
				lhs |= rhs :
				lhs &= (ulong)~rhs);
		}
		/// <summary>Modify <paramref name="lhs"/> with <paramref name="rhs"/></summary>
		/// <param name="addOrRemove">True to add <paramref name="rhs"/>, false to remove</param>
		/// <param name="lhs">Existing bit-vector</param>
		/// <param name="rhs">Other bit-vector whose bits we wish to modify on <paramref name="lhs"/></param>
		/// <returns><paramref name="addOrRemove"/></returns>
		public static bool Modify(bool addOrRemove, ref ulong lhs, ulong rhs)
		{
			if (addOrRemove == true)
				lhs |= rhs;
			else
				lhs &= (ulong)~rhs;

			return addOrRemove;
		}
		#endregion
		#endregion
	};
}
