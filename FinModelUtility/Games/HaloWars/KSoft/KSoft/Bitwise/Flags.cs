using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Bitwise
{
	/// <summary>Static class for manipulating integer bit-vectors</summary>
	public static partial class Flags
	{
	};

	/// <summary>Encases a 32-bit bit-vector into a reference object, with bit-manipulator methods</summary>
	/// <remarks>
	/// By using a reference object to store the underlying value, we can declare properties in
	/// classes which use this type and then do things like:
	/// <code>obj.Flags.Reset(0);</code>
	/// With [Flags] retaining the new value. If this were a value type object, the above code
	/// wouldn't affect [obj]'s [Flags] property because [Flags] would be RETURNING a value, not a reference
	/// </remarks>
	public sealed class Flags32
	{
		uint mValue;

		/// <summary>Reset the internal value with another</summary>
		/// <param name="value"></param>
		public void Reset(uint value)				{
			this.mValue = value; }
		/// <summary>Reset the internal value with an array of bits</summary>
		/// <param name="values">Array of bit values</param>
		public void Reset(params uint[] values)
		{
			Contract.Requires(values != null);

			foreach (uint f in values)
				this.Add(f);
		}

		/// <summary>Implicitly cast an <see cref="Flags32"/> to its unsigned integer representation</summary>
		public uint ToUInt32()
		{
			return this.mValue;
		}
		/// <summary>Implicitly cast an <see cref="Flags32"/> to its unsigned integer representation</summary>
		/// <param name="value"></param>
		public static implicit operator uint(Flags32 value)
		{
			Contract.Requires(value != null);

			return value.ToUInt32();
		}

		/// <summary>Construct a flags value from a 32-bit bit-vector</summary>
		/// <param name="value"></param>
		public Flags32(uint value)					{
			this.Reset(value); }
		/// <summary>Construct a flags value from an array of 32-bit bit-vectors</summary>
		/// <param name="values"></param>
		public Flags32(params uint[] values)
		{
			Contract.Requires(values != null);

			this.Reset(values);
		}

		/// <summary>Tests if this.Value has <paramref name="flag"/></summary>
		/// <param name="flag">flag to test</param>
		/// <returns>True if <paramref name="flag"/> is set</returns>
		[Contracts.Pure]
		public bool Test(uint flag)					{ return (this.mValue & flag) == flag; }

		#region Add
		/// <summary>Adds <paramref name="flags"/> from this.Value</summary>
		/// <param name="flags">flags to add</param>
		public void Add(uint flags)					{
			this.mValue |= flags; }

		/// <summary>Adds <paramref name="flags"/> when <paramref name="cond"/> is true</summary>
		/// <param name="cond">condition to use to determine when to set flags</param>
		/// <param name="flags">flags to add</param>
		/// <returns><paramref name="cond"/>'s value</returns>
		public bool Add(bool cond, uint flags)
		{
			if (cond)
				this.Add(flags);

			return cond;
		}
		#endregion

		#region Remove
		/// <summary>Removes <paramref name="flags"/> from this.Value</summary>
		/// <param name="flags">flags to remove</param>
		public void Remove(uint flags)				{
			this.mValue &= ~flags; }

		/// <summary>Removes <paramref name="flags"/> when <paramref name="cond"/> is true</summary>
		/// <param name="cond">condition to use to determine when to remove flags</param>
		/// <param name="flags">flags to remove</param>
		/// <returns><paramref name="cond"/>'s value</returns>
		public bool Remove(bool cond, uint flags)
		{
			if (cond)
				this.Remove(flags);

			return cond;
		}
		#endregion


		#region Overrides
		/// <summary>Tests whether or not <paramref name="obj"/> is equal to this</summary>
		/// <param name="obj">The other object</param>
		/// <returns>Returns true if <paramref name="obj"/> is a <see cref="Flags32"/> object and if it's value is the same as this</returns>
		public override bool Equals(object obj)	{ return obj is Flags32 && ((Flags32)obj).mValue == this.mValue; }
		/// <summary><see cref="uint.GetHashCode()"/></summary>
		/// <returns></returns>
		/// <remarks>Beware: this uses the underlying flags value's hash code</remarks>
		public override int GetHashCode()		{ return this.mValue.GetHashCode(); }
		/// <summary><see cref="uint.ToString()"/></summary>
		/// <returns>Returns a 8 character hexadecimal value in a string</returns>
		public override string ToString()
		{
			return this.mValue.ToString("X8", Util.InvariantCultureInfo);
		}
		#endregion
	};
}
