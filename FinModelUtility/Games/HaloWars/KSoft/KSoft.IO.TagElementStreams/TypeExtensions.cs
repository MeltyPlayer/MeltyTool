using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft
{
	public static partial class TypeExtensionsTagElementStreams
	{
		/// <summary>Does the requested type require an associated name in the element stream?</summary>
		/// <param name="nodeType"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool RequiresName(this IO.TagElementNodeType nodeType)
		{
			return nodeType != IO.TagElementNodeType.Text; // aka, Cursor
		}

		#region TagElementStreamFormat
		[Contracts.Pure]
		public static IO.TagElementStreamFormat GetBaseFormat(this IO.TagElementStreamFormat format)
		{
			return format & ~IO.TagElementStreamFormat.kTypeFlags;
		}
		[Contracts.Pure]
		public static IO.TagElementStreamFormat GetTypeFlags(this IO.TagElementStreamFormat format)
		{
			return format & IO.TagElementStreamFormat.kTypeFlags;
		}
		[Contracts.Pure]
		public static bool IsText(this IO.TagElementStreamFormat format)
		{
			return (format & IO.TagElementStreamFormat.Binary) == 0;
		}
		[Contracts.Pure]
		public static bool IsBinary(this IO.TagElementStreamFormat format)
		{
			return (format & IO.TagElementStreamFormat.Binary) != 0;
		}
		#endregion

		public static void Serialize<TDoc, TCursor, TContext>(this Collections.BitSet @this,
			IO.TagElementStream<TDoc, TCursor, string> s,
			string elementName,
			TContext ctxt,
			IO.TagElementStreamDefaultSerializer.SerializeBitToTagElementStreamDelegate<TDoc, TCursor, TContext> streamElement,
			int highestBitIndex = TypeExtensions.kNoneInt32)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(streamElement != null);
			Contract.Requires(highestBitIndex.IsNoneOrPositive());
			Contract.Requires(highestBitIndex < @this.Length);

			IO.TagElementStreamDefaultSerializer.Serialize(@this, s, elementName,
				ctxt, streamElement,
				highestBitIndex);
		}
	};
};

namespace KSoft.IO
{
	public static partial class TagElementStreamDefaultSerializer;
};
