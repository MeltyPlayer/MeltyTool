using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Expr = System.Linq.Expressions.Expression;

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Don't use me unless you're <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>. I am a util class</summary>
	public abstract class EnumBinaryStreamerBase
	{
		protected static readonly Type kBinaryReaderType;
		protected static readonly Type kBinaryWriterType;

		#region Stream Methods
		// I could have made this readonly as well, but then I would have to move the init code from InitializeMethodDictionaries to the cctor
		static Dictionary<TypeCode, MethodInfo> kReadMethods, kWriteMethods;

		/// <summary>Initialize <see cref="kReadMethods"/> with the read methods for the supported underlying enum types <see cref="EnumUtils.kSupportedTypeCodes"/></summary>
		static void InitializeReadMethods()
		{
			var methods = kBinaryReaderType.GetMethods();
			foreach (TypeCode c in EnumUtils.kSupportedTypeCodes)
			{
				var mi = kBinaryReaderType.GetMethod("Read" + c.ToString());
				kReadMethods.Add(c, mi);
			}
		}
		/// <summary>Initialize <see cref="kWriteMethods"/> with the read methods for the supported underlying enum types <see cref="EnumUtils.kSupportedTypeCodes"/></summary>
		static void InitializeWriteMethods()
		{
			var methods = kBinaryWriterType.GetMethods();
			// Avoid having to allocate a new array every iteration
			Type[] types = new Type[] { null };
			foreach (Type t in EnumUtils.kSupportedTypes)
			{
				types[0] = t;

				// GetMethod doesn't have a params overload :(
				var mi = kBinaryWriterType.GetMethod("Write", types);
				kWriteMethods.Add(Type.GetTypeCode(t), mi);
			}
		}

		static void InitializeMethodDictionaries()
		{
			int capacity = EnumUtils.kSupportedTypeCodes.Length;

			// #NOTE: The EnumComparer<TypeCode> may not be needed for .NET 4 environments:
			// http://www.codeproject.com/Messages/3968802/Re-What-about-Net-4-0.aspx
			kReadMethods = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);
			kWriteMethods = new Dictionary<TypeCode, MethodInfo>(capacity, EnumComparer<TypeCode>.Instance);

			InitializeReadMethods();
			InitializeWriteMethods();
		}
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBinaryStreamerBase()
		{
			kBinaryReaderType = typeof(BinaryReader);
			kBinaryWriterType = typeof(BinaryWriter);

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
			/// <summary><typeparamref name="TStreamType"/>'s Read method in <see cref="BinaryReader"/></summary>
			public static readonly MethodInfo kRead;
			/// <summary><typeparamref name="TStreamType"/>'s Write method in <see cref="BinaryWriter"/></summary>
			public static readonly MethodInfo kWrite;

			[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
			static StreamType()
			{
				TypeCode c = Type.GetTypeCode(typeof(TStreamType));

				kRead = kReadMethods[c];
				kWrite = kWriteMethods[c];
			}
		};
	};

	#region IEnumBinaryStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	[Contracts.ContractClass(typeof(IEnumBinaryStreamerContract<>))]
	public interface IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		TEnum Read(BinaryReader s);
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		void Read(BinaryReader s, out TEnum value);

		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		void Write(BinaryWriter s, TEnum value);
	};
	[Contracts.ContractClassFor(typeof(IEnumBinaryStreamer<>))]
	abstract class IEnumBinaryStreamerContract<TEnum> : IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		public TEnum Read(BinaryReader s)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
		public void Read(BinaryReader s, out TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
		public void Write(BinaryWriter s, TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
	};
	#endregion

	#region IEnumEndianStreamer
	/// <summary>
	/// Interface for using an <see cref="EnumBinaryStreamer{TEnum,TStreamType}"/>'s functionality via an instance object
	/// </summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	[Contracts.ContractClass(typeof(IEnumEndianStreamerContract<>))]
	public interface IEnumEndianStreamer<TEnum> : IEnumBinaryStreamer<TEnum>
		where TEnum : struct
	{
		void Stream(EndianStream s, ref TEnum value);
	};
	[Contracts.ContractClassFor(typeof(IEnumEndianStreamer<>))]
	abstract class IEnumEndianStreamerContract<TEnum> : IEnumEndianStreamer<TEnum>
		where TEnum : struct
	{
		public abstract TEnum Read(BinaryReader s);
		public abstract void Read(BinaryReader s, out TEnum value);
		public abstract void Write(BinaryWriter s, TEnum value);

		public void Stream(EndianStream s, ref TEnum value)
		{
			Contract.Requires<ArgumentNullException>(s != null);

			throw new NotImplementedException();
		}
	};
	#endregion

	public static class EnumBinaryStreamer
	{
		#region IEnumBinaryStreamer
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum, TStreamType>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumBinaryStreamer<TEnum> ForBinary<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Ensures(Contract.Result<IEnumBinaryStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion

		#region IEnumEndianStreamer
		public static IEnumEndianStreamer<TEnum> For<TEnum, TStreamType>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
			where TStreamType : struct
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum, TStreamType>.Instance;
		}
		public static IEnumEndianStreamer<TEnum> For<TEnum>()
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			Contract.Ensures(Contract.Result<IEnumEndianStreamer<TEnum>>() != null);

			return EnumBinaryStreamer<TEnum>.Instance;
		}
		#endregion
	};

	/// <summary>Utility for auto-generating methods for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	public class EnumBinaryStreamer<TEnum, TStreamType> : EnumBinaryStreamerBase, IEnumEndianStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct
	{
		class MethodGenerationArgs
		{
			/// <summary>Integer-type to stream the enum value as</summary>
			public readonly Type StreamType;
			/// <summary>Enum type to stream</summary>
			public readonly Type EnumType;
			/// <summary><see cref="EnumType"/>'s integer type used to represent its raw value</summary>
			public readonly Type UnderlyingType;
			/// <summary>True when <see cref="UnderlyingType"/> != <see cref="StreamType"/></summary>
			public readonly bool UnderlyingTypeNeedsConversion;
			public readonly bool UseUnderlyingType;

			void AssertStreamTypeIsValid()
			{
				var tc = Type.GetTypeCode(this.StreamType);

				if (!EnumUtils.TypeIsSupported(tc))
				{
					var message = string.Format(Util.InvariantCultureInfo, "{0} is an invalid stream type", this.StreamType);

					throw new NotSupportedException(message);
				}
			}

			public MethodGenerationArgs()
			{
				this.EnumType = typeof(TEnum);
				this.StreamType = typeof(TStreamType);
				this.UnderlyingType = Enum.GetUnderlyingType(this.EnumType);

				// Check if the user wants us to always use the underlying type
				this.UseUnderlyingType = this.StreamType == typeof(EnumBinaryStreamerUseUnderlyingType);
				if (this.UseUnderlyingType)
					this.StreamType = this.UnderlyingType;

				EnumUtils.AssertTypeIsEnum(this.EnumType);
				EnumUtils.AssertUnderlyingTypeIsSupported(this.EnumType, this.UnderlyingType);
				this.AssertStreamTypeIsValid();

				this.UnderlyingTypeNeedsConversion = this.UnderlyingType != this.StreamType;
			}
		};

		/// <summary>Auto-generated method for reading enum values</summary>
		static readonly ReadDelegate kRead;
		/// <summary>Auto-generated method for writing enum values</summary>
		static readonly Action<BinaryWriter, TEnum> kWrite;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumEndianStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBinaryStreamer{TEnum}"/> class by generating the IO methods.</summary>
		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBinaryStreamer()
		{
			var generation_args = new MethodGenerationArgs();
			MethodInfo read_method_info, write_method_info;
			#region Get read/write method info
			if (generation_args.UseUnderlyingType)
			{
				// Since we use a type-parameter hack to imply we want to use the underlying type
				// for the TStreamType, we have to use reflection to instantiate StreamType<>
				// using kUnderlyingType, which kStreamType is set to up above
				var stream_type_gen_class = typeof(StreamType<>);
				var stream_type_class = stream_type_gen_class.MakeGenericType(generation_args.StreamType);
				read_method_info = stream_type_class.GetField("kRead").GetValue(null) as MethodInfo;
				write_method_info = stream_type_class.GetField("kWrite").GetValue(null) as MethodInfo;
			}
			else
			{
				// If we don't use the type-parameter hack and instead are explicitly given the
				// integer type, we can safely instantiate StreamType<> without reflection
				read_method_info = StreamType<TStreamType>.kRead;
				write_method_info = StreamType<TStreamType>.kWrite;
			}
			#endregion

			kRead = GenerateReadMethod(generation_args, read_method_info);
			kWrite = GenerateWriteMethod(generation_args, write_method_info);

			Instance = new EnumBinaryStreamer<TEnum, TStreamType>();
		}

		#region Method generators
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		public delegate void ReadDelegate(BinaryReader s, out TEnum v);

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Read(BinaryReader s, out TEnum v)
		/// {
		///     v = (UnderlyingType)s.Read[TStreamType]();
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="readMethodInfo"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static ReadDelegate GenerateReadMethod(MethodGenerationArgs args, MethodInfo readMethodInfo)
		{
			// Get a "ref type" of the enum we're dealing with so we can define the enum value as an 'out' parameter
			var enum_ref = args.EnumType.MakeByRefType();

			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var param_s =		Expr.Parameter(kBinaryReaderType, "s");					// BinaryReader s
			var param_v =		Expr.Parameter(enum_ref, "v");							// ref TEnum v

			//////////////////////////////////////////////////////////////////////////
			// Define the Read call
			var call_read =		Expr.Call(param_s, readMethodInfo);						// i.e., 's.Read<Type>()'
			var read_result =	args.UnderlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're reading,
									Expr.Convert(call_read, args.UnderlyingType) :		// we need to cast the Read result from TStreamType to UnderlyingType
									(Expr)call_read;

			//////////////////////////////////////////////////////////////////////////
			// Define the member assignment
			var param_v_member =Expr.PropertyOrField(param_v, EnumUtils.kMemberName);	// i.e., 'v.value__'
			// i.e., 'v.value__ = s.Read<Type>()' or 'v.value__ = (UnderlyingType)s.Read<Type>()'
			var assign =		Expr.Assign(param_v_member, read_result);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda =		Expr.Lambda<ReadDelegate>(assign, param_s, param_v);
			return lambda.Compile();
		}

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Write(BinaryWriter s, TEnum v)
		/// {
		///     s.Write((TStreamType)v);
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="writeMethodInfo"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static Action<BinaryWriter, TEnum> GenerateWriteMethod(MethodGenerationArgs args, MethodInfo writeMethodInfo)
		{
			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var param_s =		Expr.Parameter(kBinaryWriterType, "s");					// BinaryWriter s
			var param_v =		Expr.Parameter(args.EnumType, "v");						// TEnum v

			//////////////////////////////////////////////////////////////////////////
			// Define the member access
			var param_v_member =Expr.PropertyOrField(param_v, EnumUtils.kMemberName);	// i.e., 'v.value__'
			var write_param =	args.UnderlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're writing,
									Expr.Convert(param_v_member, args.StreamType) :		// we need to cast the Write param from UnderlyingType to TStreamType
									(Expr)param_v_member;

			//////////////////////////////////////////////////////////////////////////
			// Define the Write call
			// i.e., 's.Write(v.value__)' or 's.Write((TStreamType)v.value__)'
			var call_write =	Expr.Call(param_s, writeMethodInfo, write_param);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda = Expr.Lambda<Action<BinaryWriter, TEnum>>(call_write, param_s, param_v);
			return lambda.Compile();
		}
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(BinaryReader s)
		{
			kRead(s, out TEnum value);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="BinaryReader"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		public static void Read(BinaryReader s, out TEnum value)	{ kRead(s, out value); }
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="BinaryWriter"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		public static void Write(BinaryWriter s, TEnum value)		{ kWrite(s, value); }

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.EndianStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		public static void Stream(EndianStream s, ref TEnum value)
		{
				 if (s.IsReading) Read(s.Reader, out value);
			else if (s.IsWriting) Write(s.Writer, value);
		}
		#endregion

		#region IEnumEndianStreamer<TEnum> Members
		TEnum IEnumBinaryStreamer<TEnum>.Read(BinaryReader s)						{ return Read(s); }
		void IEnumBinaryStreamer<TEnum>.Read(BinaryReader s, out TEnum value)		{ Read(s, out value); }
		void IEnumBinaryStreamer<TEnum>.Write(BinaryWriter s, TEnum value)			{ Write(s, value); }
		void IEnumEndianStreamer<TEnum>.Stream(EndianStream s, ref TEnum value)	{ Stream(s, ref value); }
		#endregion
	};

	public struct EnumBinaryStreamerUseUnderlyingType;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from binary streams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBinaryStreamer<TEnum> : EnumBinaryStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, IComparable, IFormattable, IConvertible;
}
