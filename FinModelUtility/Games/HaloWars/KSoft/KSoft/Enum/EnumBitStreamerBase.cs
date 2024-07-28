using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Don't use me unless you're <see cref="EnumBitStreamer{TEnum,TStreamType}"/>. I am a util class</summary>
	public abstract class EnumBitStreamerBase
	{
		/// <summary>
		/// <see cref="EnumBitStreamer{T}"/> makes use of the typeof(BitStream), so why not statically define it instead of executing typeof every time?
		/// </summary>
		protected static readonly Type kBitStreamType;

		#region Stream\Swap Methods
		// I could have made this readonly as well, but then I would have to move the init code from InitializeMethodDictionaries to the cctor
		static Dictionary<TypeCode, MethodInfo> kReadMethods, kWriteMethods, kBitSwapMethods;

		/// <summary>Initialize <see cref="kReadMethods"/> with the read methods for the supported underlying enum types <see cref="EnumUtils.kSupportedTypeCodes"/></summary>
		static void InitializeReadMethods()
		{
			foreach (TypeCode c in EnumUtils.kSupportedTypeCodes)
			{
				var mi = kBitStreamType.GetMethod("Read" + c.ToString());
				kReadMethods.Add(c, mi);
			}
		}
		/// <summary>Initialize <see cref="kWriteMethods"/> with the write methods for the supported underlying enum types <see cref="EnumUtils.kSupportedTypeCodes"/></summary>
		static void InitializeWriteMethods()
		{
			// Avoid having to allocate a new array every iteration
			Type[] types = new Type[] { null, null };
			types[1] = typeof(int); // bitCount
			foreach (Type t in EnumUtils.kSupportedTypes)
			{
				types[0] = t;

				// GetMethod doesn't have a params overload :(
				var mi = kBitStreamType.GetMethod("Write", types);
				kWriteMethods.Add(Type.GetTypeCode(t), mi);
			}
		}
		/// <summary>Initialize <see cref="kBitSwapMethods"/> with the BitSwap methods for the supported (unsigned) underlying enum types <see cref="EnumUtils.kSupportedTypeCodes"/></summary>
		static void InitializeBitSwapMethods()
		{
			var Bits_type = typeof(Bits);

			// Avoid having to allocate a new array every iteration
			Type[] types = new Type[] { null, null };
			types[1] = typeof(int); // startBitIndex
			foreach (Type t in EnumUtils.kSupportedTypes)
			{
				if (Type.GetTypeCode(t).IsSigned())
					continue;

				types[0] = t;

				// GetMethod doesn't have a params overload :(
				var mi = Bits_type.GetMethod("BitSwap", types);
				kBitSwapMethods.Add(Type.GetTypeCode(t), mi);
			}
		}

		static void InitializeMethodDictionaries()
		{
			int capacity = EnumUtils.kSupportedTypeCodes.Length;
			int unsigned_capacity = capacity >> 1; // number of unsigned types should be half the total types

			// #NOTE: The EnumComparer<TypeCode> may not be needed for .NET 4 environments:
			// http://www.codeproject.com/Messages/3968802/Re-What-about-Net-4-0.aspx
			kReadMethods = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);
			kWriteMethods = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);
			kBitSwapMethods = new Dictionary<TypeCode, MethodInfo>(unsigned_capacity, EnumComparer<TypeCode>.Instance);

			InitializeReadMethods();
			InitializeWriteMethods();
			InitializeBitSwapMethods();
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBitStreamerBase()
		{
			kBitStreamType = typeof(BitStream);

			InitializeMethodDictionaries();
		}

		/// <summary>Utility for instant look-up of a type's read/write methods</summary>
		/// <typeparam name="TStreamType">Integer-type</typeparam>
		/// <remarks>
		/// Why did I make a static generic class just for this? It feels clean and
		/// http://stackoverflow.com/questions/686630/static-generic-class-as-dictionary/686689#686689
		/// </remarks>
		internal protected static class StreamType<TStreamType>
			where TStreamType : struct
		{
			/// <summary><typeparamref name="TStreamType"/>'s Read method in <see cref="IO.BitStream"/></summary>
			public static readonly MethodInfo kRead;
			/// <summary><typeparamref name="TStreamType"/>'s Write method in <see cref="IO.BitStream"/></summary>
			public static readonly MethodInfo kWrite;
			/// <summary><typeparamref name="TStreamType"/>'s BitSwap method in <see cref="KSoft.Bits"/></summary>
			/// <remarks>Will be null for signed types</remarks>
			public static readonly MethodInfo kBitSwap;

			[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static StreamType()
			{
				TypeCode c = Type.GetTypeCode(typeof(TStreamType));

				kRead = kReadMethods[c];
				kWrite = kWriteMethods[c];
				kBitSwapMethods.TryGetValue(c, out kBitSwap);
			}
		};
	};

	public static class EnumBitStreamer
	{
		public static IEnumBitStreamer<TEnum> For<TEnum, TStreamType, TOptions>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
			where TOptions : EnumBitStreamerOptions, new()
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum, TStreamType, TOptions>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBitStreamer<TEnum> For<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamer<TEnum>.Instance;
		}

		public static IEnumBitStreamer<TEnum> ForWithOptions<TEnum, TOptions>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TOptions : EnumBitStreamerOptions, new()
		{
			Contract.Ensures(Contract.Result<IEnumBitStreamer<TEnum>>() != null);

			return EnumBitStreamerWithOptions<TEnum, TOptions>.Instance;
		}
	};
}
