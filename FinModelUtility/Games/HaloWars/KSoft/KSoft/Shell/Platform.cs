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

	/// <summary>Represents a platform definition</summary>
	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public struct Platform
		: IComparer<Platform>, System.Collections.IComparer
		, IComparable<Platform>, IComparable
		, IEquatable<Platform>
	{
		#region Constants
		// nesting these into a static class makes them run before the struct's static ctor...
		// which, being a value type cctor, may not run when we want it
		static class Constants
		{
			public static readonly BitFieldTraits kProcessorBitField =
				new BitFieldTraits(Processor.BitCount);
			public static readonly BitFieldTraits kPlatformTypeBitField =
				new BitFieldTraits(BitEncoders.PlatformType.BitCountTrait, kProcessorBitField);

			public static readonly BitFieldTraits kLastBitField =
				kPlatformTypeBitField;
		};

		/// <summary>Number of bits required to represent a bit-encoded representation of this value type</summary>
		/// <remarks>10 bits at last count</remarks>
		public static int BitCount { get => Constants.kLastBitField.FieldsBitCount; }
		public static uint Bitmask { get => Constants.kLastBitField.FieldsBitmask.u32; }
		#endregion

		#region Internal Value
		[Interop.FieldOffset(0)] readonly uint mHandle;

		internal uint Handle { get => this.mHandle; }

		static void InitializeHandle(out uint handle,
			PlatformType platformType, Processor processorType)
		{
			var encoder = new Bitwise.HandleBitEncoder();
			encoder.Encode32(processorType.Handle, Processor.Bitmask);
			encoder.Encode32(platformType, BitEncoders.PlatformType);

			Contract.Assert(encoder.UsedBitCount == BitCount);

			handle = encoder.GetHandle32();
		}
		#endregion

		/// <summary>Construct a platform definition</summary>
		/// <param name="platformType">Operating system of the platform</param>
		/// <param name="processorType">Processor definition of the platform</param>
		public Platform(PlatformType platformType, Processor processorType)
		{
			InitializeHandle(out this.mHandle, platformType, processorType);
		}
		internal Platform(uint handle, BitFieldTraits platformField)
		{
			handle >>= platformField.BitIndex;
			handle &= Bitmask;

			this.mHandle = handle;
		}

		#region Value properties
		/// <summary>This platform's type</summary>
		public PlatformType Type { get {
			return BitEncoders.PlatformType.BitDecode(this.mHandle, Constants.kPlatformTypeBitField.BitIndex);
		} }
		/// <summary>This platform's normal processor type</summary>
		public Processor ProcessorType { get {
			return new Processor(this.mHandle, Constants.kProcessorBitField);
		} }
		#endregion

		#region Overrides
		/// <summary>See <see cref="Object.Equals"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is Platform p)
				return this.mHandle == p.mHandle;

			return false;
		}
		/// <summary>Returns a unique 32-bit identifier for this object based on its exposed properties</summary>
		/// <returns></returns>
		/// <see cref="Object.GetHashCode"/>
		public override int GetHashCode() => (int) this.mHandle;
		/// <summary>Returns a string representation of this object</summary>
		/// <returns>"[<see cref="Type"/>\t<see cref="ProcessorType.ToString()"/>]"</returns>
		public override string ToString()
		{
			Contract.Ensures(Contract.Result<string>() != null);

			return string.Format(Util.InvariantCultureInfo,
				"[{0}\t{1}]",
				this.Type.ToString(),
				this.ProcessorType.ToString()
				);
		}
		#endregion

		#region IComparer<Platform> Members
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(Platform x, Platform y) => StaticCompare(x, y);
		/// <summary>See <see cref="IComparer{T}.Compare"/></summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		int System.Collections.IComparer.Compare(object x, object y)
		{
			Debug.TypeCheck.CastValue(x, out Platform _x);
			Debug.TypeCheck.CastValue(y, out Platform _y);

			return StaticCompare(_x, _y);
		}
		#endregion

		#region IComparable<Platform> Members
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(Platform other) => StaticCompare(this, other);
		/// <summary>See <see cref="IComparable{T}.CompareTo"/></summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		int IComparable.CompareTo(object obj)
		{
			Debug.TypeCheck.CastValue(obj, out Platform _obj);

			return StaticCompare(this, _obj);
		}
		#endregion

		#region IEquatable<Platform> Members
		/// <summary>See <see cref="IEquatable{T}.Equals"/></summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(Platform other) => this.mHandle == other.mHandle;
		#endregion

		#region Operators
		public static bool operator ==(Platform a, Platform b) => a.Equals(b);
		public static bool operator !=(Platform a, Platform b) => !(a == b);
		#endregion


		#region Util
		static int StaticCompare(Platform lhs, Platform rhs)
		{
			Contract.Assert(Processor.BitCount < Bits.kInt32BitCount,
				"Handle bits needs to be <= 31 (ie, sans sign bit) in order for this implementation of CompareTo to reasonably work");

			int lhs_data = (int)lhs.mHandle;
			int rhs_data = (int)rhs.mHandle;
			int result = lhs_data - rhs_data;

			return result;
		}
		#endregion

		/*
		 Environment Signatures:
			'MS32' - Windows 32bit
			'MS64' - Windows 64bit
			'MPPC' - Mac PowerPC
			'Mx86' - Mac Intel x86
			'Mx64' - Mac Intel x64
			'PING' - Linux (aka Ping1)

			'XBOX' - Xbox 1
			'X360' - Xbox 360
			'X720' - Xbox Durango
			'SPS2' - Playstation 2
			'SPS3' - Playstation 3
			'WIWI' - Nintendo Wii
			'CUBE' - GameCube
		*/
		static readonly Platform kUndefined = new Platform(PlatformType.Undefined, Processor.Undefined);
		/// <summary>Undefined platform</summary>
		/// <remarks>Only use for comparison operations, don't query Processor properties. Results will be...undefined</remarks>
		public static Platform Undefined { get { return kUndefined; } }

		#region Windows
		static readonly Platform kWin32 = new Platform(PlatformType.Windows, Processor.Intelx86);
		/// <summary>Microsoft Windows 32-bit platform</summary>
		public static Platform Win32 { get { return kWin32; } }

		static readonly Platform kWin64 = new Platform(PlatformType.Windows, Processor.Intelx64);
		/// <summary>Microsoft Windows 64-bit platform</summary>
		public static Platform Win64 { get { return kWin64; } }
		#endregion

		#region Xbox
		static readonly Platform kXbox1 = new Platform(PlatformType.Xbox, Processor.Intelx86);
		/// <summary>Microsoft Xbox (Original) platform</summary>
		public static Platform Xbox1 { get { return kXbox1; } }

		static readonly Platform kXbox360 = new Platform(PlatformType.Xbox, Processor.PowerPcXenon);
		/// <summary>Microsoft Xbox 360 platform</summary>
		public static Platform Xbox360 { get { return kXbox360; } }

		static readonly Platform kXboxDurango = new Platform(PlatformType.Xbox, Processor.Intelx64);
		/// <summary>Microsoft Xbox One platform</summary>
		public static Platform XboxDurango { get { return kXboxDurango; } }
		#endregion

		#region Mac
		static readonly Platform kMac32 = new Platform(PlatformType.Mac, Processor.PowerPc32);
		/// <summary>Apple's Macintosh PowerPC 32-bit platform</summary>
		public static Platform Mac32 { get { return kMac32; } }

		static readonly Platform kMac64 = new Platform(PlatformType.Mac, Processor.PowerPc64);
		/// <summary>Apple's Macintosh PowerPC 64-bit platform</summary>
		public static Platform Mac64 { get { return kMac64; } }

		static readonly Platform kMacIntel32 = new Platform(PlatformType.Mac, Processor.Intelx86);
		/// <summary>Apple's Macintosh for Intel 32-bit platform</summary>
		public static Platform MacIntel32 { get { return kMacIntel32; } }

		static readonly Platform kMacIntel64 = new Platform(PlatformType.Mac, Processor.Intelx64);
		/// <summary>Apple's Macintosh for Intel 64-bit platform</summary>
		public static Platform MacIntel64 { get { return kMacIntel64; } }
		#endregion

		#region Operating Environment
		static class OperatingEnvironment
		{
			internal static readonly bool kIsMonoRuntime;
			internal static readonly Platform kEnvironment;

			[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
			[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static OperatingEnvironment()
			{
				kIsMonoRuntime = System.Type.GetType("Mono.Runtime") != null;

				// #REVIEW: .NET 4 upgrade:
				// System.Environment.Is64BitProcess and Is64BitOperatingSystem
				ProcessorSize size;
				switch (IntPtr.Size) // HACK: the only way I've read on how to detect the processor size (assuming you compile with AnyCPU)
				{
					case 4: size = ProcessorSize.x32; break;
					case 8: size = ProcessorSize.x64; break;

					default:
						throw new Debug.UnreachableException(string.Format(Util.InvariantCultureInfo,
							"Pointer Size: {0}",
							IntPtr.Size.ToString(Util.InvariantCultureInfo)));
				}

				Contract.Assume(System.Environment.OSVersion != null);
				switch (System.Environment.OSVersion.Platform)
				{
					case PlatformID.Win32NT:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Win32; break;
							case ProcessorSize.x64: kEnvironment = Win64; break;
						}
						break;

					// #REVIEW: Somehow use Environment.OSVersion.Version to detect if this is
					// a Mac Intel (I think PPC was discontinued after 10.5?)
					case PlatformID.MacOSX:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Mac32; break;
							case ProcessorSize.x64: kEnvironment = Mac64; break;
						}
						break;

					case PlatformID.Xbox:
						switch (size)
						{
							case ProcessorSize.x32: kEnvironment = Xbox360; break;
							case ProcessorSize.x64: kEnvironment = XboxDurango; break;
						}
						break;

					// #REVIEW: iPod maybe?

					default:
						throw new Debug.UnreachableException(string.Format(Util.InvariantCultureInfo,
							"PlatformID: {0}",
							System.Environment.OSVersion.Platform.ToString()));
				}
			}
		};

		/// <summary>Is the current .NET runtime Mono based, or Microsoft?</summary>
		public static bool IsMonoRuntime  { get => OperatingEnvironment.kIsMonoRuntime; }
		/// <summary>Get the library's platform definition for the current operating environment</summary>
		public static Platform Environment { get => OperatingEnvironment.kEnvironment; }
		#endregion
	};
}
