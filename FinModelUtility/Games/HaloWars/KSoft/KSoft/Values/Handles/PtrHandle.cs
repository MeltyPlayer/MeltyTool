using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Values
{
	public enum PtrHandleType : byte
	{
		/// <summary>Pointer is absolute, no fix-ups are needed</summary>
		Absolute,
		/// <summary>Pointer is relative to a base address</summary>
		/// <remarks>a.k.a., a virtual address</remarks>
		Relative,

		[Obsolete(EnumBitEncoderBase.kObsoleteMsg, true)]
		kNumberOf
	};

	/// <summary>Wrapper structure for handling either a 32-bit or 64-bit pointer (address)</summary>
	/// <remarks>If you use the parameterless ctor, the pointer will be implicitly 32-bit</remarks>
	[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = kSizeOf)]
//	[System.ComponentModel.TypeConverter(typeof(PtrHandleConverter))]
	[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
	public struct PtrHandle
		: IO.IEndianStreamable
		, IComparer<PtrHandle>, IComparable<PtrHandle>
		, IEquatable<PtrHandle>, IEqualityComparer<PtrHandle>
		, System.Collections.IComparer, IComparable
	{
		#region Constants
		/// <summary>Size needed for extra pointer info, padded to the nearest 64-bits</summary>
		const int kSizeOfInfo = sizeof(bool) + sizeof(byte) +
			sizeof(short) + // unused
			sizeof(uint);

		/// <summary>Size of the largest address entity</summary>
		public const int kSizeOf = sizeof(ulong) + kSizeOfInfo;

		/// <summary>Constant value representing a null 32-bit address</summary>
		public static PtrHandle Null32 { get { return new PtrHandle(uint.MinValue); } }
		/// <summary>Constant value representing a null 64-bit address</summary>
		public static PtrHandle Null64 { get { return new PtrHandle(ulong.MinValue); } }

		/// <summary>Constant value representing a 32-bit INVALID_HANDLE (-1)?</summary>
		public static PtrHandle InvalidHandle32 { get { return new PtrHandle(uint.MaxValue); } }
		/// <summary>Constant value representing a 64-bit INVALID_HANDLE (-1)?</summary>
		public static PtrHandle InvalidHandle64 { get { return new PtrHandle(ulong.MaxValue); } }

		/// <summary>
		/// Result returned from a Comparison operation when the 'lhs' field
		/// doesn't match the 'rhs' field's address size
		/// </summary>
		public const int kComparisonDifferentSize = -2;
		/// <summary>Result returned from a comparison operation when 'lhs &lt; rhs'</summary>
		public const int kComparisonLess = -1;
		/// <summary>Result returned from a comparison operation when 'lhs == rhs'</summary>
		public const int kComparisonEqual = 0;
		/// <summary>Result returned from a comparison operation when 'lhs &gt; rhs'</summary>
		public const int kComparisonGreater = 1;
		#endregion

		#region Fields
		/// <summary>Address as a 64-bit integer</summary>
		[System.Runtime.InteropServices.FieldOffset(0)]
		ulong Handle;
		/// <summary>Extra Pointer info field space</summary>
		[System.Runtime.InteropServices.FieldOffset(8)]
		[SuppressMessage("Style", "IDE0044:Delegate invocation can be simplified.")]
		ulong Info;

		/// <summary>Mutable 32-bit Address</summary>
		[System.Runtime.InteropServices.FieldOffset(0)]
		public uint u32;
		/// <summary>Mutable 64-bit Address</summary>
		[System.Runtime.InteropServices.FieldOffset(0)]
		public ulong u64;

		/// <summary>Is this Address 64-bit?</summary>
		[System.Runtime.InteropServices.FieldOffset(8)]
		public readonly bool Is64bit;
		/// <summary>Extra user-specified data</summary>
		/// <remarks>Not used for any internal calculations (ie, won't affect any output)</remarks>
		[System.Runtime.InteropServices.FieldOffset(9)]
		public PtrHandleType Type;

		//[System.Runtime.InteropServices.FieldOffset(10)]

		/// <summary>Extra user-specified data</summary>
		/// <remarks>Not used for any internal calculations (ie, won't affect any output)</remarks>
		[System.Runtime.InteropServices.FieldOffset(12)]
		public uint UserData;
		#endregion

		/// <summary>The size of this pointer</summary>
		public Shell.ProcessorSize Size { get { return !this.Is64bit ? Shell.ProcessorSize.x32 : Shell.ProcessorSize.x64; } }
		/// <summary>Is this pointer not referencing anything?</summary>
		public bool IsNull { get { return this.Handle == 0; } }
		public bool IsNotNull { get { return this.Handle != 0; } }
		/// <summary>Is this pointer the same as the Win32 API INVALID_HANDLE (or -1) value?</summary>
		public bool IsInvalidHandle { get { return this.Handle == ulong.MaxValue || this.u32 == uint.MaxValue; } }
		public bool IsNotInvalidHandle { get { return this.Handle != ulong.MaxValue || this.u32 != uint.MaxValue; } }

		#region Ctor
		PtrHandle(bool is64bit, ulong handle)
		{
			this.Handle = this.Info = this.u64 = 0;
			this.u32 = this.UserData = 0;
			this.Type = PtrHandleType.Absolute;

			this.Handle = handle;
			this.Is64bit = is64bit;
		}

		/// <summary>Construct an address based on the traits (but not the actual value) of another <see cref="PtrHandle"/></summary>
		/// <param name="baseInfo">Handle with the traints\info to inherit from</param>
		/// <param name="address">Value to use as this handle's value</param>
		public PtrHandle(PtrHandle baseInfo, ulong address) : this(baseInfo.Is64bit, address)
		{
			this.Type = baseInfo.Type;
		}

		/// <summary>Construct an address which is implicity casted based on the size</summary>
		/// <param name="addressSize">Real size of <paramref name="address"/></param>
		/// <param name="address">Starting address value</param>
		public PtrHandle(Shell.ProcessorSize addressSize, ulong address) : this(addressSize == Shell.ProcessorSize.x64, address)
		{
		}

		/// <summary>Construct an address with an explicit size, but no set value (default to NULL)</summary>
		/// <param name="addressSize">This pointer's Address size</param>
		public PtrHandle(Shell.ProcessorSize addressSize)
		{
			this.Handle = this.Info = this.u64 = 0;
			this.u32 = this.UserData = 0;
			this.Type = PtrHandleType.Absolute;

			this.Is64bit = addressSize == Shell.ProcessorSize.x64;
		}

		/// <summary>Construct a 32-bit address</summary>
		/// <param name="address">Starting address value</param>
		public PtrHandle(uint address) : this(Shell.ProcessorSize.x32)	{
			this.u32 = address; }
		/// <summary>Construct a 64-bit address</summary>
		/// <param name="address">Starting address value</param>
		public PtrHandle(ulong address) : this(Shell.ProcessorSize.x64)	{
			this.u64 = address; }
		#endregion

		#region IEndianStreamable Members
		/// <summary>Stream the pointer data from a buffer</summary>
		/// <param name="s"></param>
		/// <remarks>Number of bytes read depends on <see cref="Is64bit"/></remarks>
		public void Read(IO.EndianReader s)
		{
			if (!this.Is64bit)	this.u32 = s.ReadUInt32();
			else			this.u64 = s.ReadUInt64();
		}
		/// <summary>Stream the pointer data to a buffer</summary>
		/// <param name="s"></param>
		/// <remarks>Number of bytes written depends on <see cref="Is64bit"/></remarks>
		public void Write(IO.EndianWriter s)
		{
			if (!this.Is64bit)	s.Write(this.u32);
			else			s.Write(this.u64);
		}
		#endregion

		#region Compare Members
		/// <summary>Compares two <see cref="PtrHandle"/>s, comparing only their address fields</summary>
		/// <param name="x">left-hand value for comparison expression</param>
		/// <param name="y">right-hand value for comparison expression</param>
		/// <returns>
		/// <see cref="kComparisonLess"/>: x is less than y
		/// <see cref="kComparisonGreater"/>: x is greater than y
		/// <see cref="kComparisonEqual"/>: x is equal to y
		/// </returns>
		public static int NonStrictCompare(PtrHandle x, PtrHandle y)
		{
			if (x.Handle == y.Handle)		return kComparisonEqual;

			else if (x.Handle < y.Handle)	return kComparisonLess;
			else							return kComparisonGreater;
		}

		/// <summary>Compare two <see cref="PtrHandle"/> objects for similar size and address values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns>
		/// <see cref="kComparisonDifferentSize"/>: x.<see cref="Is64bit"/> != y.<see cref="Is64bit"/>
		/// <see cref="kComparisonLess"/>: x is less than y
		/// <see cref="kComparisonGreater"/>: x is greater than y
		/// <see cref="kComparisonEqual"/>: x is equal to y
		/// </returns>
		public int Compare(PtrHandle x, PtrHandle y)					{ return x.Is64bit == y.Is64bit ? NonStrictCompare(x, y) : kComparisonDifferentSize; }
		/// <summary>Compare this with another <see cref="PtrHandle"/> object for similar size and address values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(PtrHandle other)							{ return this.Compare(this, other); }

		/// <summary></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// <see cref=""/>
		int System.Collections.IComparer.Compare(object x, object y)	{ return this.Compare((PtrHandle)x, (PtrHandle)y); }
		/// <summary></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		/// <see cref=""/>
		int IComparable.CompareTo(object obj)							{ return this.Compare(this, (PtrHandle)obj); }
		#endregion

		#region IEquatable & IEqualityComparer Members
		/// <summary>Compares two <see cref="PtrHandle"/>s, testing both their size and address fields for equality</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> == <paramref name="rhs"/></returns>
		public static bool StrictEquals(PtrHandle lhs, PtrHandle rhs)	{ return lhs.Is64bit == rhs.Is64bit && lhs.Handle == rhs.Handle; }

		/// <summary>Compares this to another <see cref="PtrHandle"/> object testing their address fields for equality</summary>
		/// <param name="other">other <see cref="PtrHandle"/> object</param>
		/// <returns>true if both this object and <paramref name="other"/> are equal</returns>
		public bool Equals(PtrHandle other)				{ return this == other; }
		/// <summary>Compares two <see cref="PtrHandle"/> objects testing their address fields for equality</summary>
		/// <param name="x">left-hand value for comparison expression</param>
		/// <param name="y">right-hand value for comparison expression</param>
		/// <returns>true if both <paramref name="x"/> and <paramref name="y"/> are equal</returns>
		public bool Equals(PtrHandle x, PtrHandle y)	{ return x == y; }
		/// <summary>Returns the hash code for this instance</summary>
		/// <returns></returns>
		public int GetHashCode(PtrHandle obj)			{ return obj.GetHashCode(); }
		#endregion

		#region Overrides
		/// <summary>Compares two <see cref="PtrHandle"/> objects testing their address size and address value</summary>
		/// <param name="obj">other <see cref="PtrHandle"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public override bool Equals(object obj)
		{
			if (obj is PtrHandle p)
			{
				return this == p;
			}

			return false;
		}

		/// <summary>Returns the hash code for this instance</summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			// 32-bit cases
			if (!this.Is64bit)
				return this.u32.GetHashCode();

			// 64-bit cases
			int hi, lo;
			unchecked
			{
				hi = (int)Bits.GetHighBits(this.u64);
				lo = (int)Bits.GetLowBits(this.u64);
			}

			return hi ^ lo;
		}

		/// <summary>Converts this instance to a string</summary>
		/// <returns>"[0x<see cref="Handle"/>]u32]" or "[0x<see cref="Handle"/>]u64"]</returns>
		public override string ToString()
		{
			return string.Format(Util.InvariantCultureInfo,
				"[0x{0}{1}]",
				!this.Is64bit ? this.u32.ToString("X8", Util.InvariantCultureInfo) : this.u64.ToString("X16", Util.InvariantCultureInfo),
				!this.Is64bit ? "u32" : "u64");
		}
		#endregion

		#region Operators
		#region Conversions
		/// <summary>Explicit cast to a <see cref="Boolean"/>, returning whether <paramref name="value"/> is null or not</summary>
		/// <param name="value">Address being casted</param>
		/// <returns>Whether <paramref name="value"/> is null or not</returns>
		[SuppressMessage("Microsoft.Design", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static explicit operator bool(PtrHandle value)	=> value.u64 == 0;

		public uint ToUInt32()									=> this.u32;
		/// <summary>Explicit cast to a <see cref="UInt32"/></summary>
		/// <param name="value">Address being casted</param>
		/// <returns>The address as a 32-bit integer</returns>
		public static explicit operator uint(PtrHandle value)	=> value.ToUInt32();

		public long ToInt64()									=> (long) this.u64;
		/// <summary>Explicit cast to a <see cref="Int64"/></summary>
		/// <param name="value">Address being casted</param>
		/// <returns>The address as a 64-bit integer</returns>
		public static explicit operator long(PtrHandle value)	=> value.ToInt64();

		public ulong ToUInt64()									=> this.u64;
		/// <summary>Explicit cast to a <see cref="UInt64"/></summary>
		/// <param name="value">Address being casted</param>
		/// <returns>The address as a 64-bit integer</returns>
		public static explicit operator ulong(PtrHandle value)	=> value.ToUInt64();

		/// <summary>Explicit cast to a <see cref="Shell.ProcessorSize"/></summary>
		/// <param name="value">Address being casted</param>
		/// <returns>The address size of <paramref name="value"/></returns>
		[SuppressMessage("Microsoft.Design", "CA2225:OperatorOverloadsHaveNamedAlternates")]
		public static explicit operator Shell.ProcessorSize(PtrHandle value)	{ return value.Size; }

		/// <summary>Convert this address to a <see cref="UIntPtr"/></summary>
		/// <returns></returns>
		/// <exception cref="OverflowException">
		/// On a 32-bit platform, <see cref="Handle"/> is too large to represent as
		/// an <see cref="UIntPtr"/>.
		/// </exception>
		public UIntPtr ToUIntPtr() { return new UIntPtr(this.Handle); }
		#endregion

		#region Boolean
		/// <summary>Compare two addresses (equality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> == <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		[Contracts.Pure]
		public static bool operator ==(PtrHandle lhs, PtrHandle rhs)	{ return /*lhs.Is64bit == rhs.Is64bit &&*/ lhs.Handle == rhs.Handle; }
		/// <summary>Compare two addresses (inequality)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> != <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		[Contracts.Pure]
		public static bool operator !=(PtrHandle lhs, PtrHandle rhs)	{ return /*lhs.Is64bit != rhs.Is64bit &&*/ lhs.Handle != rhs.Handle; }
		/// <summary>Compare two addresses (greater-than or equal)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> &gt;= <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		public static bool operator >=(PtrHandle lhs, PtrHandle rhs)	{ return lhs.Handle >= rhs.Handle; }
		/// <summary>Compare two addresses (less-than or equal)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> &lt;= <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		public static bool operator <=(PtrHandle lhs, PtrHandle rhs)	{ return lhs.Handle <= rhs.Handle; }
		/// <summary>Compare two addresses (greater-than)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> &gt; <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		public static bool operator >(PtrHandle lhs, PtrHandle rhs)		{ return lhs.Handle > rhs.Handle; }
		/// <summary>Compare two addresses (less-than)</summary>
		/// <param name="lhs">left-hand value for comparison expression</param>
		/// <param name="rhs">right-hand value for comparison expression</param>
		/// <returns><paramref name="lhs"/> &lt; <paramref name="rhs"/></returns>
		/// <remarks>Ignores address size</remarks>
		public static bool operator <(PtrHandle lhs, PtrHandle rhs)		{ return lhs.Handle < rhs.Handle; }
		#endregion

		#region Math
		/// <summary>Perform mathematical operation (Add)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> + <paramref name="rhs"/></returns>
		public static PtrHandle Add(PtrHandle lhs, PtrHandle rhs)
		{
			Contract.Requires<InvalidOperationException>(lhs.Is64bit == rhs.Is64bit);

			return new PtrHandle(lhs.Is64bit, lhs.Handle + rhs.Handle);
		}
		/// <summary>Perform mathematical operation (Add)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> + <paramref name="rhs"/></returns>
		public static PtrHandle operator +(PtrHandle lhs, PtrHandle rhs)
		{
			Contract.Requires<InvalidOperationException>(lhs.Is64bit == rhs.Is64bit);

			return Add(lhs, rhs);
		}

		/// <summary>Perform mathematical operation (Add)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> + <paramref name="rhs"/></returns>
		public static PtrHandle Add(PtrHandle lhs, uint rhs)
		{
			return new PtrHandle(lhs.Is64bit, lhs.Handle + rhs);
		}
		/// <summary>Perform mathematical operation (Add)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> + <paramref name="rhs"/></returns>
		public static PtrHandle operator +(PtrHandle lhs, uint rhs)
		{
			return Add(lhs, rhs);
		}

		/// <summary>Perform mathematical operation (Subtract)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> - <paramref name="rhs"/></returns>
		public static PtrHandle Subtract(PtrHandle lhs, PtrHandle rhs)
		{
			Contract.Requires<InvalidOperationException>(lhs.Is64bit == rhs.Is64bit);

			return new PtrHandle(lhs.Is64bit, lhs.Handle - rhs.Handle);
		}
		/// <summary>Perform mathematical operation (Subtract)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> - <paramref name="rhs"/></returns>
		public static PtrHandle operator -(PtrHandle lhs, PtrHandle rhs)
		{
			Contract.Requires<InvalidOperationException>(lhs.Is64bit == rhs.Is64bit);

			return Subtract(lhs, rhs);
		}

		/// <summary>Perform mathematical operation (Subtract)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> - <paramref name="rhs"/></returns>
		public static PtrHandle Subtract(PtrHandle lhs, uint rhs)
		{
			return new PtrHandle(lhs.Is64bit, lhs.Handle - rhs);
		}
		/// <summary>Perform mathematical operation (Subtract)</summary>
		/// <param name="lhs">left-hand value for operation expression</param>
		/// <param name="rhs">right-hand value for operation expression</param>
		/// <returns><paramref name="lhs"/> - <paramref name="rhs"/></returns>
		public static PtrHandle operator -(PtrHandle lhs, uint rhs)
		{
			return Subtract(lhs, rhs);
		}
		#endregion
		#endregion
	};
}
