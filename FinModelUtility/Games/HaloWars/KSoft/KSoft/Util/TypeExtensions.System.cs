using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	partial class TypeExtensions
	{
		public const string kDefaultArrayValueSeperator = ",";

		#region TypeCode
		[Contracts.Pure]
		public static TypeCode TryGetTypeCode(this object theObj)
		{
			var type_code = TypeCode.Empty;

			if (theObj != null)
			{
				type_code = Type.GetTypeCode(theObj.GetType());
			}

			return type_code;
		}

		[Contracts.Pure]
		public static bool IsSigned(this TypeCode c)
		{
			switch (c)
			{
				case TypeCode.SByte:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
					return true;

				default:
					return false;
			}
		}

		[Contracts.Pure]
		public static bool IsUnsigned(this TypeCode c)
		{
			switch (c)
			{
				case TypeCode.Byte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
					return true;

				default:
					return false;
			}
		}
		#endregion

		public static string ToStringInvariant(this float v, string format = null)
		{
			if (!string.IsNullOrEmpty(format))
				return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);

			return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
		public static string ToStringInvariant(this double v, string format = null)
		{
			if (!string.IsNullOrEmpty(format))
				return v.ToString(format, System.Globalization.CultureInfo.InvariantCulture);

			return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		#region Boolean
		public static string ToBinaryString(this bool b)
		{
			return b == false ? "0" : "1";
		}
		public static string ToLowerString(this bool b)
		{
			return b == false ? "false" : "true";
		}

		public static sbyte SByte(this bool b)
		{
			return b == false ? (sbyte)0 : (sbyte)1;
		}
		public static byte ToByte(this bool b)
		{
			return b == false ? (byte)0U : (byte)1U;
		}
		public static bool ToBoolean(this sbyte v)
		{
			return v != 0;
		}
		public static bool ToBoolean(this byte v)
		{
			return v != 0;
		}

		public static short ToInt16(this bool b)
		{
			return b == false ? (short)0 : (short)1;
		}
		public static ushort ToUInt16(this bool b)
		{
			return b == false ? (ushort)0 : (ushort)1;
		}
		public static bool ToBoolean(this short v)
		{
			return v != 0;
		}
		public static bool ToBoolean(this ushort v)
		{
			return v != 0;
		}

		public static int ToInt32(this bool b)
		{
			return b == false ? 0 : 1;
		}
		public static uint ToUInt32(this bool b)
		{
			return b == false ? 0U : 1U;
		}
		public static bool ToBoolean(this int v)
		{
			return v != 0;
		}
		public static bool ToBoolean(this uint v)
		{
			return v != 0;
		}

		public static long ToInt64(this bool b)
		{
			return b == false ? 0L : 1L;
		}
		public static ulong ToUInt64(this bool b)
		{
			return b == false ? 0UL : 1UL;
		}
		public static bool ToBoolean(this long v)
		{
			return v != 0;
		}
		public static bool ToBoolean(this ulong v)
		{
			return v != 0;
		}
		#endregion

		#region Exception
		public static void UnusedExceptionVar(this Exception e)
		{
		}

		public static Exception GetOnlyExceptionOrAllWhenAggregate(this Exception e)
		{
			var ae = e as AggregateException;
			return ae.GetOnlyExceptionOrAll() ?? e;
		}

		public static Exception GetOnlyExceptionOrAll(this AggregateException e)
		{
			if (e == null)
				return null;

			//e = e.ReallyFlatten();

			if (e.InnerExceptions.Count == 1)
			{
				var inner = e.InnerExceptions[0];
				return inner;
			}

			return e;
		}

		// based on MS reference source for Flatten(). Handles the case when a non-AggregateException
		// has an InnerException which IS an AggregateException
		public static AggregateException ReallyFlatten(this AggregateException e)
		{
			// Initialize a collection to contain the flattened exceptions.
			var flattenedExceptions = new List<Exception>();

			// Create a list to remember all aggregates to be flattened, this will be accessed like a FIFO queue
			var exceptionsToFlatten = new List<AggregateException>
			{
				e
			};
			int nDequeueIndex = 0;

			// Continue removing and recursively flattening exceptions, until there are no more.
			while (exceptionsToFlatten.Count > nDequeueIndex)
			{
				// dequeue one from exceptionsToFlatten
				var currentInnerExceptions = exceptionsToFlatten[nDequeueIndex++].InnerExceptions;

				for (int i = 0; i < currentInnerExceptions.Count; i++)
				{
					Exception currentInnerException = currentInnerExceptions[i];

					if (currentInnerException == null)
					{
						continue;
					}

					// If this exception is an aggregate, keep it around for later.  Otherwise,
					// simply add it to the list of flattened exceptions to be returned.
					if (currentInnerException is AggregateException currentInnerAsAggregate)
					{
						exceptionsToFlatten.Add(currentInnerAsAggregate);
					}
					else
					{
						flattenedExceptions.Add(currentInnerException);

						currentInnerAsAggregate = currentInnerException.InnerException as AggregateException;
						if (currentInnerAsAggregate != null)
						{
							exceptionsToFlatten.Add(currentInnerAsAggregate);
						}
					}
				}
			}

			var inner = flattenedExceptions.Count > 0
				? flattenedExceptions.Distinct()
				: null;

			return new AggregateException(e.Message, inner);
		}

		public static string ToBasicString(this Exception e)
		{
			if (e == null)
				return null;

			if (!(e is AggregateException ae))
				return e.Message;

			var sb = new System.Text.StringBuilder();
			foreach (var inner in ae.InnerExceptions)
			{
				if (inner.TargetSite == null)
					continue;

				sb.AppendLine(inner.Message);
			}

			return sb.ToString();
		}
		public static string ToVerboseString(this Exception e)
		{
			if (e == null)
				return null;

			if (!(e is AggregateException ae))
				return e.ToString();

			var sb = new System.Text.StringBuilder();
			foreach (var inner in ae.InnerExceptions)
			{
				if (inner.TargetSite == null)
					continue;

				sb.AppendLine(inner.Message);
				var trace = inner.GetKSoftStackTrace();
				sb.AppendLine(trace);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		public static List<string> GetKSoftStackTraceList(this Exception e, bool needFileInfo = true)
		{
			var list = new List<string>();
			var trace = new StackTrace(e, needFileInfo);
			if (trace.FrameCount == 0)
				return list;

			var sb = new System.Text.StringBuilder(128);
			for (int x = 0; x < trace.FrameCount; x++)
			{
				var frame = trace.GetFrame(x);

				var mb = frame.GetMethod();
				if (mb == null)
					continue;

				Type classType = mb.DeclaringType;
				if (classType == null)
					continue;

				// Add namespace.classname:MethodName
				string ns = classType.Namespace;
				if (!string.IsNullOrEmpty(ns))
				{
					sb.Append(ns);
					sb.Append(".");
				}

				sb.Append(classType.Name);
				sb.Append(":");
				sb.Append(mb.Name);
				sb.Append("(");

				bool firstParam = true;
				foreach (var param in mb.GetParameters())
				{
					if (firstParam)
						firstParam = false;
					else
						sb.Append(", ");

					sb.Append(param.ParameterType.Name);
				}

				sb.Append(")");

				string path = frame.GetFileName();
				if (path.IsNotNullOrEmpty())
				{
					// Unify path names to unix style
					//path = path.Replace('\\', '/');

					const string kBasePath = @"KStudio\Vita\"; // #TODO need a better way of doing this
					int base_path_index = path.IndexOf(kBasePath, StringComparison.InvariantCultureIgnoreCase);
					if (base_path_index >= 0)
					{
						path = path.Substring(base_path_index + kBasePath.Length);
					}

					sb.Append(" (");
					sb.Append(path);

					int lineNum = frame.GetFileLineNumber();
					if (lineNum > 0)
					{
						sb.Append(":");
						sb.Append(lineNum);
					}
					sb.Append(")");
				}

				list.Add(sb.ToString());
				sb.Clear();
			}

			return list;
		}

		public static string GetKSoftStackTrace(this Exception e, bool needFileInfo = true)
		{
			var list = GetKSoftStackTraceList(e, needFileInfo);
			if (list.IsNullOrEmpty())
				return string.Empty;

			var sb = new System.Text.StringBuilder(512);
			foreach (var line in list)
			{
				sb.AppendLine(line);
			}

			return sb.ToString();
		}
		#endregion

		#region String
		[SuppressMessage("Microsoft.Design", "CA1305:SpecifyIFormatProvider")]
		public static string Format(this string format, params object[] args)
		{
			return string.Format(format, args);
		}
		public static string FormatWith(this string format, IFormatProvider provider, params object[] args)
		{
			Contract.Requires<ArgumentNullException>(provider != null);

			return string.Format(provider, format, args);
		}

		[Contracts.Pure]
		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}
		[Contracts.Pure]
		public static bool IsNotNullOrEmpty(this string str)
		{
			return !string.IsNullOrEmpty(str);
		}

		[Contracts.Pure]
		public static bool StartsWith(this string str, char character)
		{
			return str != null && str.Length > 0 && str[0] == character;
		}

		[Contracts.Pure]
		public static bool EndsWith(this string str, char character)
		{
			return str != null && str.Length > 0 && str[str.Length-1] == character;
		}

		[Contracts.Pure]
		public static bool Contains(this string str, char c)
		{
			return !string.IsNullOrEmpty(str) && str.IndexOf(c) != -1;
		}

		[Contracts.Pure]
		public static int GetDeterministicHashCode(this string str)
		{
			Contract.Ensures(!string.IsNullOrEmpty(str) || Contract.Result<int>() == 0);

			if (string.IsNullOrEmpty(str))
				return 0;

			int h = 0;
			int x;
			int end = str.Length - 1;
			for (x = 0; x < end; x += 2)
			{
				h = (h << 5) - h + str[x];
				h = (h << 5) - h + str[x+1];
			}
			++end;
			if (x < end)
				h = (h << 5) - h + str[x];
			return h;
		}

		public static char[] ToWideCharBuffer(this string s, int maxBufferSize, bool nullTerminate = true)
		{
			Contract.Requires(maxBufferSize >= 0);
			Contract.Ensures(Contract.Result<char[]>() != null);

			var buffer = new char[maxBufferSize];

			int x;
			for (x = 0; s != null && x < s.Length; x++)
				buffer[x] = s[x];

			if (s != null && x == maxBufferSize && nullTerminate)
				buffer[x - 1] = '\0';

			return buffer;
		}

		public static byte[] ToAsciiCharBuffer(this string s, int maxBufferSize, bool nullTerminate = true)
		{
			Contract.Requires(maxBufferSize >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			var buffer = new byte[maxBufferSize];

			int x;
			for (x = 0; s != null && x < s.Length; x++)
			{
				char c = s[x];
				if (c < 0 || c > sbyte.MaxValue)
					throw new System.IO.InvalidDataException(string.Format(Util.InvariantCultureInfo,
						"0x{0:X4} does not look like ASCII. #{1} in '{2}'",
						(int)c, x, s));

				buffer[x] = (byte)c;
			}

			if (s != null && x == maxBufferSize && nullTerminate)
				buffer[x - 1] = 0;

			return buffer;
		}

		/// <remarks>Handles the case where no args are provided, for whatever reason, so there's no hidden object[] allocation</remarks>
		public static string AddFormat(this ICollection<string> collection, string value)
		{
			Contract.Ensures((collection != null && collection.IsReadOnly) || Contract.Result<string>() == null);

			if (collection != null && !collection.IsReadOnly)
			{
				collection.Add(value);
				return value;
			}
			return null;
		}

		[SuppressMessage("Microsoft.Design", "CA1305:SpecifyIFormatProvider")]
		public static string AddFormat(this ICollection<string> collection, string format, params object[] args)
		{
			Contract.Ensures((collection != null && collection.IsReadOnly) || Contract.Result<string>() == null);

			if (collection != null && !collection.IsReadOnly)
			{
				string value = string.Format(format, args);
				collection.Add(value);
				return value;
			}
			return null;
		}

		public static string Join(this IList<string> list
			, string valueSeperator = ",")
		{
			if (list.IsNullOrEmpty() || valueSeperator.IsNullOrEmpty())
				return "";

			return string.Join(valueSeperator, list.ToArray());
		}

		public static string TransformToString(this IEnumerable<string> list
			, string valueSeperator = ",")
		{
			Contract.Requires(list != null);

			var sb = new System.Text.StringBuilder();
			foreach (var str in list)
			{
				if (sb.Length > 0)
					sb.Append(valueSeperator);

				sb.Append(str);
			}

			return sb.ToString();
		}
		#endregion

		#region Array
		[Contracts.Pure]
		[DebuggerStepThrough]
		public static IEnumerator<T> GetGenericEnumerator<T>(this T[] array)
		{
			Contract.Requires(array != null);

			return (IEnumerator<T>)array.GetEnumerator();
		}

		[Contracts.Pure]
		public static string ArrayToConcatString(this Array array
			, string valueSeperator = kDefaultArrayValueSeperator)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (array == null || array.Length == 0)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in array)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToString());
			}

			return sb.ToString();
		}

		[Contracts.Pure]
		public static bool EqualsZero<T>(this T[] array)
			where T : struct, IEquatable<T>
		{
			Contract.Requires(array != null);

			var zero = new T();

			for (int x = 0; x < array.Length; x++)
				if (!array[x].Equals(zero))
					return false;

			return true;
		}
		[Contracts.Pure]
		public static bool EqualsDefault<T>(this T[] array)
			where T : class, IEquatable<T>, new()
		{
			Contract.Requires(array != null);

			var zero = new T();

			for (int x = 0; x < array.Length; x++)
				if (!array[x].Equals(zero))
					return false;

			return true;
		}
		[Contracts.Pure]
		public static bool EqualsArray<T>(this T[] lhs, T[] rhs, int lhsOffset = 0)
			where T : IEquatable<T>
		{
			Contract.Requires(lhs != null && rhs != null);
			Contract.Requires(lhsOffset < lhs.Length);

			if (lhs == rhs)
				return true;
			else if (rhs.Length < (lhs.Length-lhsOffset))
				return false;

			for (int x = lhsOffset; x < (lhs.Length-lhsOffset); x++)
				if (!rhs[x].Equals(lhs[x]))
					return false;

			return true;
		}

		[Contracts.Pure]
		public static bool TrueForAny<T>(this T[] array, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(array != null);
			Contract.Requires<ArgumentNullException>(match != null);

			for (int x = 0; x < array.Length; x++)
				if (match(array[x]))
					return true;

			return false;
		}

		#region FastFill
		// currently based on http://stackoverflow.com/a/33865267/444977
		public const int kDefaultFastMemSetLoopThreshold = 100;
		public const int kFastMemSetBlockSize = 32;

		public static void FastClear<T>(this T[] array
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			FastClear(array, array.Length, loopThreshold);
		}
		public static void FastClear<T>(this T[] array, int length
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			if (length <= loopThreshold)
			{
				var zero = default(T);
				for (int x = 0; x < array.Length; x++)
					array[x] = zero;
			}
			else
			{
				Array.Clear(array, 0, length);
			}
		}

		public static void FastFill<T>(this T[] array, T fillValue, int sizeOfT
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			FastFill(array, array.Length, fillValue, sizeOfT, loopThreshold);
		}
		public static void FastFill<T>(this T[] array, int length, T fillValue, int sizeOfT
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			if (length <= loopThreshold)
			{
				for (int x = 0; x < array.Length; x++)
					array[x] = fillValue;
			}
			else
			{
				int block_size = kFastMemSetBlockSize;

				// fill the starting block in the array
				int index = 0;
				for (int initializeLength = Math.Min(block_size, length); index < initializeLength; index++)
				{
					array[index] = fillValue;
				}

				// use the starting block to fill the rest, increasing the block size by however much we've filled so far or what's left
				for (; index < length; index += block_size, block_size *= 2)
				{
					int copy_length = Math.Min(block_size, length-index) * sizeOfT;
					// Array.Copy is not the same as BlockCopy. We want BlockCopy.
					Buffer.BlockCopy(
						array, 0,
						array, index*sizeOfT,
						copy_length);
				}
			}
		}

		public static void FastFillOrClear<T>(this T[] array, bool fillOrClear, T fillValue, int sizeOfT
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			FastFillOrClear(array, array.Length, fillOrClear, fillValue, sizeOfT, loopThreshold);
		}
		public static void FastFillOrClear<T>(this T[] array, int length, bool fillOrClear, T fillValue, int sizeOfT
			, int loopThreshold = kDefaultFastMemSetLoopThreshold)
		{
			if (fillOrClear)
			{
				FastFill(array, length, fillValue, sizeOfT, loopThreshold);
			}
			else
			{
				FastClear(array, length, loopThreshold);
			}
		}
		#endregion
		#endregion

		#region Type
		/// <summary>Test whether the subject type implements the specified interface type</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="genericType">The interface type which the subject may or may not implement</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool ImplementsInterface(this Type subject, Type interfaceType)
		{
			Contract.Requires(subject != null);
			Contract.Requires(interfaceType != null);
			Contract.Requires(interfaceType.IsInterface);

			return interfaceType.IsAssignableFrom(subject);
		}

		/// <summary>Test whether the subject type derives from a generic type using CRTP</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="genericType">The generic type which has a single type parameter, which will be populated with subject</param>
		/// <returns></returns>
		/// <remarks>See: http://en.wikipedia.org/wiki/Curiously_recurring_template_pattern </remarks>
		[Contracts.Pure]
		public static bool IsCuriouslyRecurringTemplatePattern(this Type subject, Type genericType)
		{
			Contract.Requires(subject != null);
			Contract.Requires(genericType != null);
			Contract.Requires(genericType.IsGenericType && genericType.IsGenericTypeDefinition);
			Contract.Requires(genericType.GetGenericArguments().Length == 1);

			var concrete_type = genericType.MakeGenericType(subject);

			return concrete_type.IsAssignableFrom(subject);
		}

		/// <summary>Force the .cctor of a type to run by invoking the getter of a static property</summary>
		/// <param name="subject">The type in question</param>
		/// <param name="staticPropertyName">Name of the static property to get</param>
		public static void ForceStaticCtorToRunViaProperty(this Type subject, string staticPropertyName)
		{
			Contract.Requires(subject != null);
			Contract.Requires(!string.IsNullOrEmpty(staticPropertyName));

			const BindingFlags k_static_property_binding_flags
				= BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.DeclaredOnly;

			var static_props = subject.GetProperties(k_static_property_binding_flags);
			var static_prop = (
					from prop in static_props
					where prop.Name == staticPropertyName
					select prop
				).First();

			// This doesn't cause the static ctor to run it would seem.
			//Activator.CreateInstance(concrete_type);

			// However, effectively invoking a static property does
			static_prop.GetValue(null);
		}
		#endregion

		#region Collections
		[Contracts.Pure]
		[DebuggerStepThrough]
		public static bool IsNullOrEmpty<T>(this ICollection<T> coll)
		{
			return coll == null || coll.Count == 0;
		}

		[Contracts.Pure]
		[DebuggerStepThrough]
		public static bool IsNotNullOrEmpty<T>(this ICollection<T> coll)
		{
			return coll != null && coll.Count != 0;
		}

		[Contracts.Pure]
		[DebuggerStepThrough]
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> seq)
		{
			return seq ?? Enumerable.Empty<T>();
		}

		/// <summary>Query if 'seq' contains duplicate values</summary>
		/// <param name="seq">The sequence to act on</param>
		/// <returns>True if there are duplicates, false if all values are distinct</returns>
		/// <remarks>Based on this answer http://stackoverflow.com/a/4712539/444977 </remarks>
		[Contracts.Pure]
		[DebuggerStepThrough]
		public static bool ContainsDuplicates<T>(this IEnumerable<T> seq)
		{
			// A list has *no* duplicates when All items can be Add-ed to a set.
			return !seq.All(new HashSet<T>().Add);
		}

		[Contracts.Pure]
		public static TOutput[] ConvertAllArray<TInput, TOutput>(this IList<TInput> list, Converter<TInput, TOutput> converter)
		{
			Contract.Requires(converter != null);

			if (list == null)
				return null;

			var array = new TOutput[list.Count];

			for (int x = 0; x < array.Length; x++)
				array[x] = converter(list[x]);

			return array;
		}

		[Contracts.Pure]
		public static int FindIndex<T>(this IEnumerable<T> seq, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(seq != null);
			Contract.Requires<ArgumentNullException>(match != null);

			var found = seq
				.Select((v, i) => new KeyValuePair<T, int?>(v, i))
				.FirstOrDefault(kvp => match(kvp.Key));

			return found.Value ?? kNone;
		}

		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			int startIndex, int count, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Requires<ArgumentOutOfRangeException>(list.Count == 0 || startIndex < list.Count);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0 && startIndex <= list.Count-count);
			Contract.Requires<ArgumentNullException>(match != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < startIndex+count);

			int end_index = startIndex + count;
			for (int x = startIndex; x < end_index; x++)
			{
				if (match(list[x]))
					return x;
			}

			return kNone;
		}
		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			int startIndex, Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < startIndex+list.Count);

			return FindIndex(list, startIndex, list.Count-startIndex, match);
		}
		[Contracts.Pure]
		public static int FindIndex<T>(this IReadOnlyList<T> list,
			Predicate<T> match)
		{
			Contract.Requires<ArgumentNullException>(list != null);
			Contract.Ensures(Contract.Result<int>().IsNoneOrPositive());
			Contract.Ensures(Contract.Result<int>() < list.Count);

			return FindIndex(list, 0, list.Count, match);
		}

		/// <summary>
		/// Grows the collection, using the default value of <typeparamref name="T"/>, if it is less than <paramref name="requiredCount"/>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <param name="requiredCount"></param>
		public static void EnsureCount<T>(this ICollection<T> collection, int requiredCount)
		{
			Contract.Requires<ArgumentNullException>(collection != null);
			Contract.Requires<InvalidOperationException>(!collection.IsReadOnly);
			Contract.Requires<ArgumentOutOfRangeException>(requiredCount >= 0);

			if (collection.Count < requiredCount)
			{
				var default_value = default(T);
				int add_count = requiredCount - collection.Count;

				// arbitrary add threshold for List optimization
				if (add_count > 16)
				{
					if (collection is List<T> list && list.Capacity < requiredCount)
					{
						list.Capacity += add_count;
					}
				}

				for (int x = 0; x < add_count; x++)
				{
					collection.Add(default_value);
				}
			}
		}

		[Contracts.Pure]
		public static bool EqualsList<T>(this IList<T> lhs, IList<T> rhs, int lhsOffset = 0)
			where T : IEquatable<T>
		{
			Contract.Requires(lhs != null && rhs != null);
			Contract.Requires(lhsOffset < lhs.Count);

			if (lhs == rhs)
				return true;
			else if (rhs.Count < (lhs.Count - lhsOffset))
				return false;

			for (int x = lhsOffset; x < (lhs.Count - lhsOffset); x++)
				if (!rhs[x].Equals(lhs[x]))
					return false;

			return true;
		}

		[Contracts.Pure]
		public static string ToConcatString<T>(this IEnumerable<T> e
			, string valueSeperator = kDefaultArrayValueSeperator)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (e == null)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in e)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToString());
			}

			return sb.ToString();
		}
		[Contracts.Pure]
		public static string ToConcatBinaryString(this IEnumerable<bool> e
			, string valueSeperator = kDefaultArrayValueSeperator)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (e == null)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in e)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToBinaryString());
			}

			return sb.ToString();
		}
		[Contracts.Pure]
		public static string ToConcatLowerString(this IEnumerable<bool> e
			, string valueSeperator = kDefaultArrayValueSeperator)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (e == null)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in e)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToLowerString());
			}

			return sb.ToString();
		}
		[Contracts.Pure]
		public static string ToConcatStringInvariant(this IEnumerable<float> e
			, string valueSeperator = kDefaultArrayValueSeperator
			, string format = null)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (e == null)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in e)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToStringInvariant(format));
			}

			return sb.ToString();
		}
		[Contracts.Pure]
		public static string ToConcatStringInvariant(this IEnumerable<double> e
			, string valueSeperator = kDefaultArrayValueSeperator
			, string format = null)
		{
			Contract.Ensures(Contract.Result<string>() != null);

			if (e == null)
				return string.Empty;

			var sb = new System.Text.StringBuilder();
			foreach (var obj in e)
			{
				if (sb.Length > 0 && valueSeperator.IsNotNullOrEmpty())
					sb.Append(valueSeperator);

				sb.Append(obj.ToStringInvariant(format));
			}

			return sb.ToString();
		}

		public static List<T> SortAndReturn<T>(this List<T> list, Comparison<T> comparison)
		{
			list.Sort(comparison);
			return list;
		}
		#endregion

		#region Diagnostics
		public static void TraceDataSansId(this TraceSource source, TraceEventType eventType, params object[] data)
		{
			Contract.Requires(source != null);

			source.TraceData(eventType, kNone, data);
		}

		public static void TraceDataSansId(this TraceSource source, TraceEventType eventType, object data)
		{
			Contract.Requires(source != null);

			source.TraceData(eventType, kNone, data);
		}
		#endregion

		#region IO
		[Contracts.Pure]
		public static bool CanRead(this System.IO.FileAccess value)
		{
			return (value & System.IO.FileAccess.Read) == System.IO.FileAccess.Read;
		}
		[Contracts.Pure]
		public static bool CanWrite(this System.IO.FileAccess value)
		{
			return (value & System.IO.FileAccess.Write) == System.IO.FileAccess.Write;
		}

		[Contracts.Pure]
		public static long BytesRemaining(this System.IO.Stream s)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);

			return s.Length - s.Position;
		}
		[Contracts.Pure]
		public static long BytesRemaining(this System.IO.Stream s, long endPosition)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(endPosition <= s.Length);

			return endPosition - s.Position;
		}
		[Contracts.Pure]
		public static bool HasPermissions(this System.IO.Stream s, System.IO.FileAccess permissions)
		{
			Contract.Requires(s != null);
			Contract.Requires<InvalidOperationException>(s.CanSeek);
			bool result = true;

			if (permissions.CanRead())
				result &= s.CanRead;
			if (permissions.CanWrite())
				result &= s.CanWrite;

			return result;
		}

		public static int PeekByte(this System.IO.BinaryReader r)
		{
			Contract.Requires(r != null);

			if (!r.BaseStream.CanSeek)
				return -1;

			int b = r.ReadByte();
			r.BaseStream.Seek(-sizeof(byte), System.IO.SeekOrigin.Current);

			return b;
		}
		#endregion

		/// <summary>
		/// When the position is 32-bits, returns 8-char hex string, else 16-char
		/// </summary>
		/// <param name="filePos"></param>
		/// <returns></returns>
		public static string ToFilePositionHexString(this long filePos)
		{
			Contract.Requires(filePos >= 0);

			string result;

			if (filePos <= uint.MaxValue)
				result = filePos.ToString("X8", Util.InvariantCultureInfo);
			else
				result = filePos.ToString("X16", Util.InvariantCultureInfo);

			return result;
		}

		#region HashAlgorithm
		//[Obsolete("Use StreamHashComputer instead")]
		public static byte[] ComputeHash(this System.Security.Cryptography.HashAlgorithm algo,
			System.IO.Stream inputStream, long offset, long count,
			bool restorePosition = false,
			byte[] preallocatedBuffer = null)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNoneOrPositive());
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset+count) <= inputStream.Length);

			int buffer_size;
			byte[] buffer;

			if (preallocatedBuffer == null)
			{
				buffer_size = Math.Min((int)count, 0x1000);
				buffer = new byte[buffer_size];
			}
			else
			{
				buffer = preallocatedBuffer;
				buffer_size = buffer.Length;
			}

			algo.Initialize();

			long orig_pos = inputStream.Position;
			if (offset.IsNotNone() && offset != orig_pos)
				inputStream.Seek(offset, System.IO.SeekOrigin.Begin);

			for (long bytes_remaining = count; bytes_remaining > 0; )
			{
				long num_bytes_to_read = Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = inputStream.Read(buffer, num_bytes_read, (int)num_bytes_to_read);
					if (n == 0)
						break;

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
					algo.TransformBlock(buffer, 0, num_bytes_read, null, 0);
				else
					break;

				bytes_remaining -= num_bytes_read;
			}

			algo.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			if (restorePosition)
				inputStream.Seek(orig_pos, System.IO.SeekOrigin.Begin);

			return algo.Hash;
		}

		//[Obsolete("Use StreamBlockHashComputer instead")]
		public static byte[] ComputeHash(this Security.Cryptography.BlockHashAlgorithm algo,
			System.IO.Stream inputStream, long offset, long count,
			bool restorePosition = false)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNoneOrPositive());
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset+count) <= inputStream.Length);

			algo.Initialize();

			int buffer_size = algo.BlockSize;
			byte[] buffer = algo.InternalBlockBuffer;

			long orig_pos = inputStream.Position;
			if (offset.IsNotNone() && offset != orig_pos)
				inputStream.Seek(offset, System.IO.SeekOrigin.Begin);

			for (long bytes_remaining = count; bytes_remaining > 0; )
			{
				long num_bytes_to_read = Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = inputStream.Read(buffer, num_bytes_read, (int)num_bytes_to_read);
					if (n == 0)
						break;

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
					algo.TransformBlock(buffer, 0, num_bytes_read, null, 0);
				else
					break;

				bytes_remaining -= num_bytes_read;
			}

			algo.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			if (restorePosition)
				inputStream.Seek(orig_pos, System.IO.SeekOrigin.Begin);

			return algo.Hash;
		}
		#endregion

		#region Event handlers
		public static void SafeNotify(this PropertyChangedEventHandler handler,
			object sender, PropertyChangedEventArgs args)
		{
			if (handler != null)
				handler(sender, args);
		}
		public static void SafeNotify(this PropertyChangedEventHandler handler,
			object sender, PropertyChangedEventArgs[] argsList, int startIndex = 0)
		{
			Contract.Requires(argsList != null);
			Contract.Requires(startIndex >= 0 && startIndex < argsList.Length);

			if (handler != null)
			{
				for (int x = startIndex; x < argsList.Length; x++)
				{
					var args = argsList[x];
					handler(sender, args);
				}
			}
		}
		public static void SafeNotify(this NotifyCollectionChangedEventHandler handler,
			object sender, NotifyCollectionChangedEventArgs args)
		{
			if (handler != null)
				handler(sender, args);
		}

		// Based on http://www.codeproject.com/KB/cs/EventSafeTrigger.aspx
		public static void SafeTrigger(this EventHandler eventToTrigger,
			object sender, EventArgs eventArgs)
		{
			if (eventToTrigger != null)
				eventToTrigger(sender, eventArgs);
		}

		public static void SafeTrigger<TEventArgs>(this EventHandler<TEventArgs> eventToTrigger,
			object sender, TEventArgs eventArgs)
			where TEventArgs : EventArgs
		{
			if (eventToTrigger != null)
				eventToTrigger(sender, eventArgs);
		}

		public static TReturnType SafeTrigger<TEventArgs, TReturnType>
			(this EventHandler<TEventArgs> eventToTrigger, object sender,
			TEventArgs eventArgs, Func<TEventArgs, TReturnType> retrieveDataFunction)
			where TEventArgs : EventArgs
		{
			Contract.Requires(retrieveDataFunction != null);

			if (eventToTrigger != null)
			{
				eventToTrigger(sender, eventArgs);
				return retrieveDataFunction(eventArgs);
			}
			else
				return default(TReturnType);
		}
		#endregion

		#region GetCustomAttribute
		// Based on http://www.codeproject.com/Tips/72637/Get-CustomAttributes-the-easy-way.aspx

		/// <summary>Returns first custom attribute of type T in the inheritance chain</summary>
		[Contracts.Pure]
		public static T GetCustomAttribute<T>(this ICustomAttributeProvider provider, bool inherited = false)
			where T : Attribute
		{
			Contract.Requires<ArgumentNullException>(provider != null);

			return provider.GetCustomAttributes<T>(inherited).FirstOrDefault();
		}

		/// <summary>Returns all custom attributes of type T in the inheritance chain</summary>
		[Contracts.Pure]
		public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeProvider provider, bool inherited = false)
			where T : Attribute
		{
			Contract.Requires<ArgumentNullException>(provider != null);

			return provider.GetCustomAttributes(typeof(T), inherited).Cast<T>();
		}
		#endregion

		#region OrderBy LINQ
		// Based on http://stackoverflow.com/questions/271398/what-are-your-favorite-extension-methods-for-c-codeplex-com-extensionoverflow/858681#858681

		/// <summary>Sorts elements in a sequence in ascending order</summary>
		/// <typeparam name="TSrc"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="src">Sequence to sort</param>
		/// <param name="keySelector"></param>
		/// <param name="comparerFunc">Comparison delegate</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static IOrderedEnumerable<TSrc> OrderBy<TSrc, TKey>(this IEnumerable<TSrc> src,
			Func<TSrc, TKey> keySelector, Func<TKey, TKey, int> comparerFunc)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires/*<ArgumentNullException>*/(keySelector != null);
			Contract.Requires/*<ArgumentNullException>*/(comparerFunc != null);
			Contract.Ensures(Contract.Result<IOrderedEnumerable<TSrc>>() != null);

			var comparer = Util.CreateComparer(comparerFunc);
			return src.OrderBy(keySelector, comparer);
		}
		/// <summary>Sorts elements in a sequence in ascending order</summary>
		/// <typeparam name="TSrc"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="src">Sequence to sort</param>
		/// <param name="keySelector"></param>
		/// <param name="comparerFunc">Comparison delegate</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static IOrderedEnumerable<TSrc> OrderByDescending<TSrc, TKey>(this IEnumerable<TSrc> src,
			Func<TSrc, TKey> keySelector, Func<TKey, TKey, int> comparerFunc)
		{
			Contract.Requires<ArgumentNullException>(src != null);
			Contract.Requires/*<ArgumentNullException>*/(keySelector != null);
			Contract.Requires/*<ArgumentNullException>*/(comparerFunc != null);
			Contract.Ensures(Contract.Result<IOrderedEnumerable<TSrc>>() != null);

			var comparer = Util.CreateComparer(comparerFunc);
			return src.OrderByDescending(keySelector, comparer);
		}
		#endregion

		#region ObjectModel
		public static bool SetFieldVal<T>(this INotifyPropertyChanged theObj, PropertyChangedEventHandler handler
			, ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : struct, IEquatable<T>
		{
			if (theObj == null)
				return false;

			if (!overrideChecks)
				if (field.Equals(value))
					return false;

			field = value;

			if (handler != null)
				handler(theObj, new PropertyChangedEventArgs(propertyName));

			return true;
		}

		public static bool SetFieldEnum<TEnum>(this INotifyPropertyChanged theObj, PropertyChangedEventHandler handler
			, ref TEnum field, TEnum value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (theObj == null)
				return false;

			if (!overrideChecks)
				if (field.ToInt64(null) == value.ToInt64(null))
					return false;

			field = value;

			if (handler != null)
				handler(theObj, new PropertyChangedEventArgs(propertyName));

			return true;
		}

		public static bool SetFieldObj<T>(this INotifyPropertyChanged theObj, PropertyChangedEventHandler handler
			, ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
			where T : class, IEquatable<T>
		{
			if (theObj == null)
				return false;

			if (!overrideChecks)
			{
				if (field == null)
				{
					if (value == null)
						return false;
				}
				else if (field.Equals(value))
					return false;
			}

			field = value;

			if (handler != null)
				handler(theObj, new PropertyChangedEventArgs(propertyName));

			return true;
		}

		public static bool SetField<T>(this INotifyPropertyChanged theObj, PropertyChangedEventHandler handler
			, ref T field, T value
			, bool overrideChecks = false
			, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
		{
			if (theObj == null)
				return false;

			if (!overrideChecks)
				if (EqualityComparer<T>.Default.Equals(field, value))
					return false;

			field = value;

			if (handler != null)
				handler(theObj, new PropertyChangedEventArgs(propertyName));

			return true;
		}

		private sealed class ObservableCollectionHacks<T>
		{
			private delegate void OnPropertyChangedDelegate(PropertyChangedEventArgs e);
			public delegate void OnPropertyChangedDelegateWithThis(ObservableCollection<T> @this, PropertyChangedEventArgs e);

			private delegate void OnCollectionChangedDelegate(NotifyCollectionChangedEventArgs e);
			public delegate void OnCollectionChangedWithThis(ObservableCollection<T> @this, NotifyCollectionChangedEventArgs e);

			private static Func<ObservableCollection<T>, IList<T>> gGetItems;
			public static Func<ObservableCollection<T>, IList<T>> GetItems { get {
				if (gGetItems == null)
					gGetItems = Reflection.Util.GenerateMemberGetter<ObservableCollection<T>, IList<T>>("Items");

				return gGetItems;
			} }

			private static OnPropertyChangedDelegateWithThis gOnPropertyChangedFunc;
			public static OnPropertyChangedDelegateWithThis OnPropertyChangedFunc { get {
				if (gOnPropertyChangedFunc == null)
					gOnPropertyChangedFunc = Reflection.Util.GenerateObjectMethodProxy<ObservableCollection<T>, OnPropertyChangedDelegateWithThis, OnPropertyChangedDelegate>
						("OnPropertyChanged");

				return gOnPropertyChangedFunc;
			} }

			private static OnCollectionChangedWithThis gOnCollectionChangedFunc;
			public static OnCollectionChangedWithThis OnCollectionChangedFunc { get {
				if (gOnCollectionChangedFunc == null)
					gOnCollectionChangedFunc = Reflection.Util.GenerateObjectMethodProxy<ObservableCollection<T>, OnCollectionChangedWithThis, OnCollectionChangedDelegate>
						("OnCollectionChanged");

				return gOnCollectionChangedFunc;
			} }
		}

		[Contracts.Pure]
		public static bool ItemsIsGenericList<T>(this ObservableCollection<T> list)
		{
			if (list == null)
				return false;

			var items = ObservableCollectionHacks<T>.GetItems(list);

			return items is List<T>;
		}

		public static void TriggerAllItemsChanged<T>(this ObservableCollection<T> list)
		{
			var on_prop_changed = ObservableCollectionHacks<T>.OnPropertyChangedFunc;
			var on_coll_changed = ObservableCollectionHacks<T>.OnCollectionChangedFunc;
			on_prop_changed(list, new PropertyChangedEventArgs(ObjectModel.Util.kIndexerPropertyName));
			on_coll_changed(list, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, null, -1));
		}

		public static void AddRange<T>(this ObservableCollection<T> list, IEnumerable<T> collection)
		{
			Contract.Requires(list != null);
			Contract.Requires(collection != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			items.AddRange(collection);
			list.TriggerAllItemsChanged();
		}

		public static int BinarySearch<T>(this ObservableCollection<T> list, T value, IComparer<T> comparer)
		{
			Contract.Requires(list != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			return items.BinarySearch(value, comparer);
		}

		public static void Sort<T>(this ObservableCollection<T> list)
		{
			Contract.Requires(list != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			items.Sort();
			list.TriggerAllItemsChanged();
		}

		public static void Sort<T>(this ObservableCollection<T> list, IComparer<T> comparer)
		{
			Contract.Requires(list != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			items.Sort(comparer);
			list.TriggerAllItemsChanged();
		}

		public static void Sort<T>(this ObservableCollection<T> list, Comparison<T> comparison)
		{
			Contract.Requires(list != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			items.Sort(comparison);
			list.TriggerAllItemsChanged();
		}

		public static void Reverse<T>(this ObservableCollection<T> list)
		{
			Contract.Requires(list != null);
			Contract.Requires(list.ItemsIsGenericList());

			var items = (List<T>)ObservableCollectionHacks<T>.GetItems(list);

			items.Reverse();
			list.TriggerAllItemsChanged();
		}
		#endregion
	};
}
