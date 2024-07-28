using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	public static class Kontracts
	{
		public const string kCategory = "Microsoft.Contracts";

		// Based on ContractsManual.pdf: 5.2.3, Delegating Checks to Other Methods
		//Usage: [System.Diagnostics.CodeAnalysis.SuppressMessage(Kontracts.kCategory, Kontracts.kIgnoreOverrideId, Justification=Kontracts.kIgnoreOverrideJust)]
		/// <summary>SuppressMessage warning id</summary>
		public const string kIgnoreOverrideId = "CC1055";
		/// <summary>SuppressMessage justification</summary>
		public const string kIgnoreOverrideJust = "Validation performed in base method";
	};

	[SuppressMessage("Microsoft.Design", "CA1724:TypeNamesShouldNotMatchNamespaces")]
	public static partial class Util
	{
		// Based on http://blogs.msdn.com/b/jaredpar/archive/2011/03/18/debuggerdisplay-attribute-best-practices.aspx
		// MSDN example uses "nq" suffix, yet doesn't explain it. I knew SOMEONE had to have commented on this. If you check
		// the VS2010-era DebuggerDisplay docs you'll find a Community Addition reply with WTF "nq" is for (and the above link)
		/// <summary>DebuggerDisplay value to use to display the object's {DebuggerDisplay} value without any quotes</summary>
		public const string DebuggerDisplayPropNameSansQuotes = "{DebuggerDisplay,nq}";

		/// <summary>When you want to declare a variable, perhaps for debugging, but only ever assign it, use this to silence compiler warnings</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="unused"></param>
		public static void MarkUnusedVariable<T>(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			ref T unused)
		{}

		public static System.Globalization.CultureInfo InvariantCultureInfo { get => System.Globalization.CultureInfo.InvariantCulture; }

		#region static EmptyArray
		/// <summary>A global zero-length array of objects. Should only be used as input for functions that don't use 'params'</summary>
		[SuppressMessage("Microsoft.Design", "CA1819:PropertiesShouldNotReturnArrays")]
		public static object[] EmptyArray { get => Array.Empty<object>(); }
		#endregion

		#region static GetNullException function ptr
		private static Func<Exception> gGetNullException;
		internal static Func<Exception> GetNullException { get {
			if (gGetNullException == null)
				gGetNullException = () => null;

			return gGetNullException;
		} }
		#endregion

		#region static pre-boxed boolean values
		private static object gFalseObject;
		/// <summary>false boolean pre-boxed to an object</summary>
		public static object FalseObject { get {
			if (gFalseObject == null)
				gFalseObject = (object)false;

			return gFalseObject;
		} }

		private static object gTrueObject;
		/// <summary>true boolean pre-boxed to an object</summary>
		public static object TrueObject { get {
			if (gTrueObject == null)
				gTrueObject = (object)true;

			return gTrueObject;
		} }
		#endregion

		#region IDisposable
		sealed class NullDisposableImpl
			: IDisposable
		{
			public void Dispose() { }
		};
		/// <summary>Object which can be disposed of without limit and is thread safe</summary>
		public static readonly IDisposable NullDisposable = new NullDisposableImpl();

		/// <summary>If <paramref name="theObj"/> isn't already null, calls its Dispose and nulls the reference</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="theObj"></param>
		public static void DisposeAndNull<T>(ref T theObj)
			where T : class, IDisposable
		{
			if (theObj != null)
			{
				theObj.Dispose();
				theObj = null;
			}
		}
		#endregion

		public static void ClearAndNull<T>(ref T[] array)
		{
			array = null;
		}
		public static void ClearAndNull<T>(ref ICollection<T> coll)
		{
			if (coll != null)
			{
				coll.Clear();
				coll = null;
			}
		}

		#region Unix Time
		private static long kUnixTimeEpochInTicks;
		private static DateTime kUnixTimeEpoch;
		/// <summary>The UTC of the <b>time_t</b> C++ construct</summary>
		public static DateTime UnixTimeEpoch { get {
			if (kUnixTimeEpochInTicks == 0)
			{
				kUnixTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				kUnixTimeEpochInTicks = kUnixTimeEpoch.Ticks;
			}

			return kUnixTimeEpoch;
		} }

		/// <summary>Convert a <b>time_t</b> or <b>time64_t</b> value to a <see cref="System.DateTime"/></summary>
		/// <param name="time_t">The <b>time_t</b> numerical value</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static DateTime ConvertDateTimeFromUnixTime(
			[SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores")]
			long time_t)
		{
			Contract.Requires<ArgumentOutOfRangeException>(time_t >= 0);

			return UnixTimeEpoch.AddSeconds(time_t);
		}

		/// <summary>Convert a <see cref="System.DateTime"/> to a <b>time64_t</b> value</summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static long ConvertDateTimeToUnixTime(DateTime value)
		{
			Contract.Requires<ArgumentOutOfRangeException>(value >= UnixTimeEpoch);

			long time_t = 0;

			// Subtract unix's epoch then rebase back into seconds
			time_t = (value.ToFileTimeUtc() - UnixTimeEpoch.ToFileTimeUtc()) / 10000000;

			return time_t;
		}
		#endregion

		#region Comparer Factory
		sealed class ComparerFactory<T>
			: IComparer<T>
		{
			readonly Func<T, T, int> mComparer;

			ComparerFactory(Func<T, T, int> comparer)
			{
				this.mComparer = comparer;
			}

			#region IComparer<T> Members
			public int Compare(T x, T y)
			{
				return this.mComparer(x, y);
			}
			#endregion

			[Contracts.Pure]
			public static IComparer<T> Create(Func<T, T, int> comparer)
			{
				return new ComparerFactory<T>(comparer);
			}
		};
		[Contracts.Pure]
		public static IComparer<T> CreateComparer<T>(Func<T, T, int> comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			Contract.Ensures(Contract.Result<IComparer<T>>() != null);

			return ComparerFactory<T>.Create(comparer);
		}
		#endregion

		/// <summary>Comapres two equatable reference objects, taking same-instance and null cases into account</summary>
		/// <typeparam name="T">Reference object which is equatable</typeparam>
		/// <param name="x">First object to compare</param>
		/// <param name="y">Second object to compare</param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool GenericReferenceEquals<T>(T x, T y)
			where T : class, IEquatable<T>
		{
			// Handles 'x is same instance as y' and 'null == null'
			bool result = ReferenceEquals(x, y);

			// If they're not the same instance, or both null, either one of them is null or neither is.
			// If x isn't null, then either y is null or they're two objects in which case they can be equated
			if (!result && !(x is null))
				return x.Equals(y);

			return result;
		}

		#region Min/Max Choice
		/// <summary>Returns the object with the smaller of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MinChoice<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : class
		{
			Contract.Requires<ArgumentNullException>(lhs != null);
			Contract.Requires<ArgumentNullException>(rhs != null);
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) < choiceProperty(rhs)
				? lhs
				: rhs;
		}
		/// <summary>Returns the value with the smaller of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MinChoiceValue<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : struct
		{
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) < choiceProperty(rhs)
				? lhs
				: rhs;
		}

		/// <summary>Returns the object with the larger of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MaxChoice<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : class
		{
			Contract.Requires<ArgumentNullException>(lhs != null);
			Contract.Requires<ArgumentNullException>(rhs != null);
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) > choiceProperty(rhs)
				? lhs
				: rhs;
		}
		/// <summary>Returns the value with the larger of two properties</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="lhs"></param>
		/// <param name="rhs"></param>
		/// <param name="choiceProperty"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static T MaxChoiceValue<T>(T lhs, T rhs, Func<T, int> choiceProperty)
			where T : struct
		{
			Contract.Requires(choiceProperty != null);

			return choiceProperty(lhs) > choiceProperty(rhs)
				? lhs
				: rhs;
		}

		#endregion

		#region CountNumberOfFormatArguments
		private static int CountNumberOfFormatArgumentsFormatError(int position)
		{
			return -position;
		}
		// Based on AppendFormatHelper in https://referencesource.microsoft.com/#mscorlib/system/text/stringbuilder.cs
		/// <summary>Returns the number of expected arguments to successfully call string.Format, or a negative number representing the offset at which there is an error</summary>
		public static int CountNumberOfFormatArguments(string format)
		{
			if (string.IsNullOrEmpty(format))
				return 0;

			int highestAddressedIndex = -1;

			int pos = 0;
			int len = format.Length;
			char ch = '\x0';

			while (true)
			{
				while (pos < len)
				{
					ch = format[pos];

					pos++;
					if (ch == '}')
					{
						if (pos < len && format[pos] == '}') // Treat as escape character for }}
							pos++;
						else
							return CountNumberOfFormatArgumentsFormatError(pos);
					}

					if (ch == '{')
					{
						if (pos < len && format[pos] == '{') // Treat as escape character for {{
							pos++;
						else
						{
							pos--;
							break;
						}
					}
				}

				if (pos == len)
					break;

				pos++;
				if (pos == len || (ch = format[pos]) < '0' || ch > '9')
					return CountNumberOfFormatArgumentsFormatError(pos);

				int index = 0;
				do
				{
					index = index * 10 + ch - '0';
					pos++;
					if (pos == len)
						return CountNumberOfFormatArgumentsFormatError(pos);

					ch = format[pos];
				} while (ch >= '0' && ch <= '9' && index < 1000000);

				highestAddressedIndex = Math.Max(highestAddressedIndex, index);

				//if (index >= args.Length)
				//	throw new FormatException(Environment.GetResourceString("Format_IndexOutOfRange"));

				while (pos < len && (ch = format[pos]) == ' ')
					pos++;

				int width = 0;

				if (ch == ',')
				{
					pos++;
					while (pos < len && format[pos] == ' ')
						pos++;

					if (pos == len)
						return CountNumberOfFormatArgumentsFormatError(pos);

					ch = format[pos];
					if (ch == '-')
					{
						pos++;
						if (pos == len)
							return CountNumberOfFormatArgumentsFormatError(pos);

						ch = format[pos];
					}
					if (ch < '0' || ch > '9')
						return CountNumberOfFormatArgumentsFormatError(pos);
					do
					{
						width = width * 10 + ch - '0';
						pos++;
						if (pos == len)
							return CountNumberOfFormatArgumentsFormatError(pos);
						ch = format[pos];
					} while (ch >= '0' && ch <= '9' && width < 1000000);
				}

				while (pos < len && (ch = format[pos]) == ' ')
					pos++;

				if (ch == ':')
				{
					pos++;
					while (true)
					{
						if (pos == len)
							return CountNumberOfFormatArgumentsFormatError(pos);
						ch = format[pos];
						pos++;
						if (ch == '{')
						{
							if (pos < len && format[pos] == '{')  // Treat as escape character for {{
								pos++;
							else
								return CountNumberOfFormatArgumentsFormatError(pos);
						}
						else if (ch == '}')
						{
							if (pos < len && format[pos] == '}')  // Treat as escape character for }}
								pos++;
							else
							{
								pos--;
								break;
							}
						}
					}
				}

				if (ch != '}')
					return CountNumberOfFormatArgumentsFormatError(pos);

				pos++;
			}

			return highestAddressedIndex + 1;
		}
		#endregion

		public static string[] Trim(string[] array)
		{
			if (array == null || array.Length == 0)
				return array;

			var trimmed = new string[array.Length];

			for (int x = 0; x < array.Length; x++)
				trimmed[x] = array[x].Trim();

			return trimmed;
		}

		#region FullNameOf
		public static string FullNameOf(string nameof0, string nameof1)
		{
			return
				nameof0 +
				"." +
				nameof1;
		}
		public static string FullNameOf(string nameof0, string nameof1, string nameof2)
		{
			return
				nameof0 +
				"." +
				nameof1 +
				"." +
				nameof2;
		}
		public static string FullNameOf(string nameof0, string nameof1, string nameof2, params string[] nameofArray)
		{
			string fullnameof = FullNameOf(nameof0, nameof1, nameof2);

			if (nameofArray.IsNullOrEmpty())
			{
				foreach (string nameof_substring in nameofArray)
				{
					fullnameof += "." + nameof_substring;
				}
			}

			return fullnameof;
		}

		public static string FullNameOfThis(string nameof1)
			=> FullNameOf("this", nameof1);
		public static string FullNameOfThis(string nameof1, string nameof2)
			=> FullNameOf("this", nameof1, nameof2);
		public static string FullNameOfThis(string nameof1, string nameof2, params string[] nameofArray)
			=> FullNameOf("this", nameof1, nameof2, nameofArray);
		#endregion

		/// <summary>
		/// Emulate .NET's Enum.TryParse. Only real difference is we by default IGNORE CASE, and they don't
		/// </summary>
		public static bool TryParseEnum<TEnum>(string str, out TEnum value
			, bool ignoreCase = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			value = default(TEnum);
			return TryParseEnumOpt(str, ref value, ignoreCase);
		}
		/// <summary>
		/// Emulate .NET's Enum.TryParse. Only real difference is we by default IGNORE CASE, and they don't
		/// </summary>
		/// <remarks>Opt = Optional, as it we won't overwrite 'value' on failure (can't overload based on ref/out alone)</remarks>
		public static bool TryParseEnumOpt<TEnum>(string str, ref TEnum value
			, bool ignoreCase = true)
			where TEnum : struct, IComparable, IFormattable, IConvertible
		{
			if (string.IsNullOrEmpty(str))
				return false;

#if false // #NOTE Unity implementation
			try
			{
				value = (TEnum)Enum.Parse(typeof(TEnum), str, ignoreCase);
			}
			catch (Exception)
			{
				return false;
			}

			return true;
#else
			return Enum.TryParse(str, ignoreCase, out value);
#endif
		}

		public static bool ParseStringList(string line, List<string> list
			, bool sort = false
			, string valueSeperator = TypeExtensions.kDefaultArrayValueSeperator)
		{
			if (line == null)
			{
				return false;
			}
			if (list == null)
			{
				return false;
			}

			// LINQ stmt below allows there to be whitespace around the commas
			string[] values = System.Text.RegularExpressions.Regex.Split(line, valueSeperator);
			list.Clear();

			ParseStringList(Trim(values), list, sort);

			// handles cases where there's extra valueSeperator values
			list.RemoveAll(string.IsNullOrEmpty);

			if (sort)
				list.Sort();

			return true;
		}
		public static bool ParseStringList(IEnumerable<string> collection, List<string> list
			, bool sort = false)
		{
			if (collection == null)
			{
				return false;
			}
			if (list == null)
			{
				return false;
			}

			list.AddRange(collection);

			if (sort)
				list.Sort();

			return true;
		}

		#region GetRelativePath
		// based on https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path/32113484#32113484

		/// <summary>
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path.</returns>
		public static string GetRelativePath(string fromPath, string toPath)
		{
			Contract.Requires<ArgumentNullException>(fromPath.IsNotNullOrEmpty());
			Contract.Requires<ArgumentNullException>(toPath.IsNotNullOrEmpty());
			Contract.Ensures(Contract.Result<string>()==toPath || fromPath.IsNotNullOrEmpty());

			Uri fromUri = new Uri(AppendDirectorySeparatorChar(fromPath));
			Uri toUri = new Uri(AppendDirectorySeparatorChar(toPath));

			if (fromUri.Scheme != toUri.Scheme)
			{
				return toPath;
			}

			Uri relativeUri = fromUri.MakeRelativeUri(toUri);
			string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (string.Equals(toUri.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
			{
				relativePath = ReplaceAltDirectorySeparatorWithNormalChar(relativePath);
			}

			return relativePath;
		}

		/// <summary>Appends a slash if the path does not end with an extension and does not already have a slash</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string AppendDirectorySeparatorChar(string path)
		{
			Contract.Ensures(Contract.Result<string>().IsNullOrEmpty() || path.IsNotNullOrEmpty());

			string result = path;

			if (result.IsNotNullOrEmpty())
			{
				// Append a slash only if the path is a directory and does not have a slash.
				if (!Path.HasExtension(result) &&
					!result.EndsWith(Path.DirectorySeparatorChar))
				{
					result += Path.DirectorySeparatorChar;
				}
			}

			return result;
		}
		#endregion

		/// <summary>Prefixes a slash to the path if it does not already have a slash</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string PrependDirectorySeparatorChar(string path)
		{
			Contract.Ensures(Contract.Result<string>().IsNullOrEmpty() || path.IsNotNullOrEmpty());

			string result = path;

			if (result.IsNotNullOrEmpty())
			{
				// Prefix a slash only if the path does not have a slash.
				if (!result.StartsWith(Path.DirectorySeparatorChar))
				{
					result = Path.DirectorySeparatorChar + result;
				}
			}

			return result;
		}

		/// <summary>Removes a final slash from the path if it has one</summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string RemoveTrailingDirectorySeparatorChar(string path)
		{
			Contract.Ensures(Contract.Result<string>().IsNullOrEmpty() || path.IsNotNullOrEmpty());

			string result = path;

			if (result.IsNotNullOrEmpty())
			{
				if (result.EndsWith(Path.DirectorySeparatorChar))
				{
					result = result.Substring(0, result.Length-1);
				}
			}

			return result;
		}

		public static string ReplaceDirectorySeparatorWithAltChar(string path)
		{
			Contract.Ensures(Contract.Result<string>().IsNullOrEmpty() || path.IsNotNullOrEmpty());

			string result = path;

			if (result.IsNotNullOrEmpty())
			{
				result = result.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}

			return result;
		}
		public static string ReplaceAltDirectorySeparatorWithNormalChar(string path)
		{
			Contract.Ensures(Contract.Result<string>().IsNullOrEmpty() || path.IsNotNullOrEmpty());

			string result = path;

			if (result.IsNotNullOrEmpty())
			{
				result = result.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return result;
		}
	};
}
