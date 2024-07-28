using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Shell
{
	using BitFieldTraits = Bitwise.BitFieldTraits;
	using BitEncoders = TypeExtensions.BitEncoders;

	/// <summary>Represents a processor definition</summary>
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[System.Diagnostics.DebuggerDisplay("IA = {InstructionSet}, WordSize = {ProcessorSize}, Endian = {ByteOrder}")]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public struct Processor :
		IComparer<Processor>, System.Collections.IComparer,
		IComparable<Processor>, IComparable,
		IEquatable<Processor>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kByteOrderBitField =
				new BitFieldTraits(BitEncoders.EndianFormat.BitCountTrait);
			public static readonly BitFieldTraits kProcessorSizeBitField =
				new BitFieldTraits(BitEncoders.ProcessorSize.BitCountTrait, kByteOrderBitField);
			public static readonly BitFieldTraits kInstructionSetBitField =
				new BitFieldTraits(BitEncoders.InstructionSet.BitCountTrait, kProcessorSizeBitField);

			public static readonly BitFieldTraits kLastBitField =
				kInstructionSetBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>6 bits at last count</remarks>
		public static int BitCount { get => Constants.kLastBitField.FieldsBitCount; }
		public static uint Bitmask { get => Constants.kLastBitField.FieldsBitmask.u32; }
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		internal uint Handle { get => this.mHandle; }

		static void InitializeHandle(out uint handle,
			InstructionSet instSet, ProcessorSize procSize, EndianFormat byteOrder)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32(byteOrder, BitEncoders.EndianFormat);
			encoder.Encode32(procSize, BitEncoders.ProcessorSize);
			encoder.Encode32(instSet, BitEncoders.InstructionSet);

			Contract.Assert(encoder.UsedBitCount == BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		/// <summary>Construct a processor definition</summary>
		/// <param name="size">Instruction size of the processor</param>
		/// <param name="byteOrder">Default byte ordering of the processor</param>
		/// <param name="instructionSet">Instruction set of the processor</param>
		public Processor(ProcessorSize size, EndianFormat byteOrder, InstructionSet instructionSet)
		{
			InitializeHandle(out this.mHandle, instructionSet, size, byteOrder);
		}
		internal Processor(uint handle, BitFieldTraits processorField)
		{
			handle >>= processorField.BitIndex;
			handle &= Bitmask;

			this.mHandle = handle;
		}

		#region Value properties
		/// <summary>The processor's instruction set</summary>
		public InstructionSet InstructionSet { get =>
			BitEncoders.InstructionSet.BitDecode(this.mHandle, Constants.kInstructionSetBitField.BitIndex);
		}
		/// <summary>The processor's instruction size</summary>
		public ProcessorSize ProcessorSize { get =>
			BitEncoders.ProcessorSize.BitDecode(this.mHandle, Constants.kProcessorSizeBitField.BitIndex);
		}
		/// <summary>The processor's byte ordering</summary>
		public EndianFormat ByteOrder { get =>
			BitEncoders.EndianFormat.BitDecode(this.mHandle, Constants.kByteOrderBitField.BitIndex);
		}
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is Processor)
				return this.mHandle == ((Processor)obj).mHandle;

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode() => (int) this.mHandle;
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[InstructionSet\tProcessorSize\tByteOrder]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format(Util.InvariantCultureInfo,
				"[{0}\t{1}\t{2}]",
				this.InstructionSet.ToString(),
				this.ProcessorSize.ToString(),
				this.ByteOrder.ToString()
				);
		}
		#endregion

		#region IComparer<Processor> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(Processor x, Processor y) => StaticCompare(x, y);
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			Debug.TypeCheck.CastValue(x, out Processor _x);
			Debug.TypeCheck.CastValue(y, out Processor _y);

			return StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<Processor> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(Processor other) => StaticCompare(this, other);
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			Debug.TypeCheck.CastValue(obj, out Processor _obj);

			return StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<Processor> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Processor other) => this.mHandle == other.mHandle;
		#endregion

		#region Operators
		public static bool operator ==(Processor a, Processor b) => a.Equals(b);
		public static bool operator !=(Processor a, Processor b) => !(a == b);
		#endregion


		#region Util
		static int StaticCompare(Processor lhs, Processor rhs)
		{
			Contract.Assert(BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion

		static readonly Processor kUndefined = new Processor(uint.MaxValue, BitFieldTraits.Empty);
		/// <summary>Undefined processor definition</summary>
		/// <remarks>Only use for comparison operations, don't query value properties. Results will be...undefined</remarks>
		public static Processor Undefined		{ get => kUndefined; }

		#region Intel
		static readonly Processor kIntelx86 = new Processor(ProcessorSize.x32, EndianFormat.Little, InstructionSet.Intel);
		/// <summary>Intel's x86 processor definition</summary>
		public static Processor Intelx86		{ get { return kIntelx86; } }

		static readonly Processor kIntelx64 = new Processor(ProcessorSize.x64, EndianFormat.Little, InstructionSet.Intel);
		/// <summary>Intel's x64 processor definition</summary>
		public static Processor Intelx64		{ get { return kIntelx64; } }
		#endregion

		#region PowerPc
		static readonly Processor kPowerPc32 = new Processor(ProcessorSize.x32, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC 32-bit processor definition</summary>
		public static Processor PowerPc32		{ get { return kPowerPc32; } }

		static readonly Processor kPowerPc64 = new Processor(ProcessorSize.x64, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC 64-bit processor definition</summary>
		public static Processor PowerPc64		{ get { return kPowerPc64; } }

		static readonly Processor kPowerPcXenon = new Processor(ProcessorSize.x32, EndianFormat.Big, InstructionSet.PPC);
		/// <summary>IBM's PowerPC (Xenon) processor definition</summary>
		/// <remarks>
		/// Why is there a special Xenon definition? Because if I recall correctly, the Xenon is a 64-bit processor however
		/// its instruction set is treated as if it were 32-bit. So we may adjust the definition system later on for these
		/// types of processor cases.
		/// </remarks>
		public static Processor PowerPcXenon	{ get { return kPowerPcXenon; } }
		#endregion
	};
}
