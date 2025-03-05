using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Expr = System.Linq.Expressions.Expression;

namespace KSoft.IO
{
	using EnumUtils = Reflection.EnumUtils;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <typeparam name="TOptions">TBD</typeparam>
	public class EnumBitStreamer<TEnum, TStreamType, TOptions> : EnumBitStreamerBase, IEnumBitStreamer<TEnum>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct
		where TOptions : EnumBitStreamerOptions, new()
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
			public readonly bool StreamTypeIsSigned;

			public TOptions Options;

			void AssertStreamTypeIsValid(out bool isSigned)
			{
				var tc = Type.GetTypeCode(this.StreamType);
				isSigned = tc.IsSigned();

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
				this.AssertStreamTypeIsValid(out this.StreamTypeIsSigned);

				this.UnderlyingTypeNeedsConversion = this.UnderlyingType != this.StreamType;

				this.Options = new TOptions();

				if (this.Options.UseNoneSentinelEncoding)
				{
					if (this.StreamType == typeof(sbyte) || this.StreamType == typeof(byte))
						throw new ArgumentException(
							"{0}: UseNoneSentinelEncoding can't operate on (s)byte types (StreamType)",
							this.EnumType.FullName);
				}
				#region Options.BitSwap
				if (this.Options.BitSwap)
				{
					if (this.StreamTypeIsSigned)
						throw new ArgumentException(
							"{0}: Bit-swapping only makes sense on flags/unsigned types, but StreamType is signed",
							this.EnumType.FullName);
				}
				else
				{
					if (this.Options.BitSwapGuardAgainstOneBit)
						Debug.Trace.IO.TraceInformation("{0}'s {1} says we should guard against one bit cases, but not bitswap",
						                                this.EnumType.FullName, typeof(TOptions).FullName);
				}
				#endregion
			}
		};

		/// <summary>Auto-generated method for reading enum values</summary>
		static readonly ReadDelegate kRead;
		/// <summary>Auto-generated method for writing enum values</summary>
		static readonly Action<BitStream, TEnum, int> kWrite;

		/// <summary>Object for referencing the streamer functionality as an instance instead of as a type</summary>
		public static readonly IEnumBitStreamer<TEnum> Instance;

		/// <summary>Initializes the <see cref="EnumBitStreamer{TEnum}"/> class by generating the IO methods.</summary>
		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static EnumBitStreamer()
		{
			var generation_args = new MethodGenerationArgs();
			MethodInfo read_method_info, write_method_info, swap_method;
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
				swap_method = stream_type_class.GetField("kBitSwap").GetValue(null) as MethodInfo;
			}
			else
			{
				// If we don't use the type-parameter hack and instead are explicitly given the
				// integer type, we can safely instantiate StreamType<> without reflection
				read_method_info = StreamType<TStreamType>.kRead;
				write_method_info = StreamType<TStreamType>.kWrite;
				swap_method = StreamType<TStreamType>.kBitSwap;
			}
			#endregion

			kRead = GenerateReadMethod(generation_args, read_method_info, swap_method);
			kWrite = GenerateWriteMethod(generation_args, write_method_info, swap_method);

			Instance = new EnumBitStreamer<TEnum, TStreamType, TOptions>();
		}

		#region Method generators
		/// <summary>Signature for a method which reads a <typeparamref name="TEnum"/> value from a stream</summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="v">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public delegate void ReadDelegate(BitStream s, out TEnum v, int bitCount);

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Read(IO.BitStream s, out TEnum v, int bitCount)
		/// {
		///     v = (UnderlyingType)s.Read[TStreamType](bitCount);
		/// }
		/// </code>
		/// </summary>
		/// <param name="args"></param>
		/// <param name="readMethodInfo"></param>
		/// <param name="bitSwapMethod"></param>
		/// <returns>The generated method.</returns>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static ReadDelegate GenerateReadMethod(MethodGenerationArgs args, MethodInfo readMethodInfo, MethodInfo bitSwapMethod)
		{
			// Get a "ref type" of the enum we're dealing with so we can define the enum value as an 'out' parameter
			var enum_ref = args.EnumType.MakeByRefType();

			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var param_s =		Expr.Parameter(kBitStreamType, "s");					// BitStream s
			var param_v =		Expr.Parameter(enum_ref, "v");							// ref TEnum v
			var param_bc =		Expr.Parameter(typeof(int), "bitCount");				// int bitCount

			//////////////////////////////////////////////////////////////////////////
			// Define the Read call
			Expr call_read;
			if (args.StreamTypeIsSigned)
				call_read =		Expr.Call(param_s, readMethodInfo, param_bc, Expr.Constant(args.Options.SignExtend));
			else
				call_read =		Expr.Call(param_s, readMethodInfo, param_bc);			// i.e., 's.Read<Type>(bitCount)'

			if (args.Options.UseNoneSentinelEncoding)
				call_read = Expr.Decrement(call_read);

			#region options.BitSwap
			if (args.Options.BitSwap)
			{
				// i.e., Bits.BitSwap( Read(), bitCount-1 );
				var start_bit_index = Expr.Decrement(param_bc);
				Expr swap_call = Expr.Call(null, bitSwapMethod, call_read, start_bit_index);

				// i.e., bitCount-1 ? Bits.BitSwap( Read(), bitCount-1 ) : Read() ;
				if (args.Options.BitSwapGuardAgainstOneBit)
				{
					var start_bit_index_is_not_zero = Expr.NotEqual(start_bit_index, Expr.Constant(0, typeof(int)));
					swap_call = Expr.Condition(start_bit_index_is_not_zero,
						swap_call, call_read);
				}

				call_read = swap_call;
			}
			#endregion

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
			var lambda =		Expr.Lambda<ReadDelegate>(assign, param_s, param_v, param_bc);
			return lambda.Compile();
		}

		/// <summary>Generates a method similar to this:
		/// <code>
		/// void Write(IO.BitStream s, TEnum v, int bitCount)
		/// {
		///     s.Write((TStreamType)v, bitCount);
		/// }
		/// </code>
		/// </summary>
		/// <returns>The generated method.</returns>
		/// <param name="args"></param>
		/// <param name="writeMethodInfo"></param>
		/// <param name="bitSwapMethod"></param>
		/// <remarks>
		/// If <see cref="args.UnderlyingType"/> is the same as <typeparamref name="TStreamType"/>, no conversion code is generated
		/// </remarks>
		static Action<BitStream, TEnum, int> GenerateWriteMethod(MethodGenerationArgs args, MethodInfo writeMethodInfo, MethodInfo bitSwapMethod)
		{
			//////////////////////////////////////////////////////////////////////////
			// Define the generated method's parameters
			var param_s =		Expr.Parameter(kBitStreamType, "s");					// BitStream s
			var param_v =		Expr.Parameter(args.EnumType, "v");							// TEnum v
			var param_bc =		Expr.Parameter(typeof(int), "bitCount");				// int bitCount

			//////////////////////////////////////////////////////////////////////////
			// Define the member access
			var param_v_member =Expr.PropertyOrField(param_v, EnumUtils.kMemberName);	// i.e., 'v.value__'
			var write_param =	args.UnderlyingTypeNeedsConversion ?					// If the underlying type is different from the type we're writing,
									Expr.Convert(param_v_member, args.StreamType) :		// we need to cast the Write param from UnderlyingType to TStreamType
									(Expr)param_v_member;

			if (args.Options.UseNoneSentinelEncoding)
				write_param = Expr.Increment(write_param);

			#region options.BitSwap
			if (args.Options.BitSwap)
			{
				// i.e., Bits.BitSwap( value, bitCount-1 );
				var start_bit_index = Expr.Decrement(param_bc);
				Expr swap_call = Expr.Call(null, bitSwapMethod, write_param, start_bit_index);

				// i.e., bitCount-1 ? Bits.BitSwap( value, bitCount-1 ) : value ;
				if (args.Options.BitSwapGuardAgainstOneBit)
				{
					var start_bit_index_is_not_zero = Expr.NotEqual(start_bit_index, Expr.Constant(0, typeof(int)));
					swap_call = Expr.Condition(start_bit_index_is_not_zero,
						swap_call, write_param);
				}

				write_param = swap_call;
			}
			#endregion

			//////////////////////////////////////////////////////////////////////////
			// Define the Write call
			// i.e., 's.Write(v.value__, bitCount)' or 's.Write((TStreamType)v.value__, bitCount)'
			var call_write =	Expr.Call(param_s, writeMethodInfo, write_param, param_bc);

			//////////////////////////////////////////////////////////////////////////
			// Generate a method based on the expression tree we've built
			var lambda = Expr.Lambda<Action<BitStream, TEnum, int>>(call_write, param_s, param_v, param_bc);
			return lambda.Compile();
		}
		#endregion

		#region Static interface
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="bitCount"></param>
		/// <returns>Value read from the stream</returns>
		public static TEnum Read(BitStream s, int bitCount)
		{
			kRead(s, out TEnum value, bitCount);

			return value;
		}
		/// <summary>Stream a <typeparamref name="TEnum"/> value from a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Reader we're streaming from</param>
		/// <param name="value">Value read from the stream</param>
		/// <param name="bitCount"></param>
		public static void Read(BitStream s, out TEnum value, int bitCount)	{ kRead(s, out value, bitCount); }
		/// <summary>Stream a <typeparamref name="TEnum"/> value to a <see cref="IO.BitStream"/></summary>
		/// <param name="s">Writer we're streaming to</param>
		/// <param name="value"></param>
		/// <param name="bitCount"></param>
		public static void Write(BitStream s, TEnum value, int bitCount)		{ kWrite(s, value, bitCount); }

		/// <summary>Serialize a <typeparamref name="TEnum"/> value using an <see cref="IO.BitStream"/></summary>
		/// <param name="s">Stream we're using for serialization</param>
		/// <param name="value">Value to serialize</param>
		/// <param name="bitCount"></param>
		public static void Stream(BitStream s, ref TEnum value, int bitCount)
		{
				 if (s.IsReading) Read(s, out value, bitCount);
			else if (s.IsWriting) Write(s, value, bitCount);
		}
		#endregion

		#region IEnumBitStreamer<TEnum> Members
		TEnum IEnumBitStreamer<TEnum>.Read(BitStream s, int bitCount)					{ return Read(s, bitCount); }
		void IEnumBitStreamer<TEnum>.Read(BitStream s, out TEnum value, int bitCount)	{ Read(s, out value, bitCount); }
		void IEnumBitStreamer<TEnum>.Write(BitStream s, TEnum value, int bitCount)		{ Write(s, value, bitCount); }
		void IEnumBitStreamer<TEnum>.Stream(BitStream s, ref TEnum value, int bitCount)	{ Stream(s, ref value, bitCount); }
		#endregion
	};

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <typeparam name="TStreamType">Integer-type to stream the enum value as</typeparam>
	/// <remarks>Uses the default options in <see cref="EnumBitStreamerOptions"/></remarks>
	public class EnumBitStreamer<TEnum, TStreamType> : EnumBitStreamer<TEnum, TStreamType, EnumBitStreamerOptions>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TStreamType : struct;

	/// <summary>Utility for auto-generating methods for streaming enum types to/from bitstreams</summary>
	/// <typeparam name="TEnum">Enum type to stream</typeparam>
	/// <remarks>Implicitly uses the Enum's underlying type for the stream type</remarks>
	public sealed class EnumBitStreamer<TEnum> : EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType>
		where TEnum : struct, IComparable, IFormattable, IConvertible;

	public sealed class EnumBitStreamerWithOptions<TEnum, TOptions> : EnumBitStreamer<TEnum, EnumBinaryStreamerUseUnderlyingType, TOptions>
		where TEnum : struct, IComparable, IFormattable, IConvertible
		where TOptions : EnumBitStreamerOptions, new();
}
