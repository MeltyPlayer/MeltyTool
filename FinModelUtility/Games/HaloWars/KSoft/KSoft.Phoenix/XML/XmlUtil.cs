using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.XML
{
	public static partial class XmlUtil
	{
		/// <summary>Pass this to a Stream call when reading something from the Text data (which doesn't have a name), just to be clear of the code's intention</summary>
		public const string kNoXmlName = null;
		public const IO.TagElementNodeType kSourceAttr = IO.TagElementNodeType.Attribute;
		public const IO.TagElementNodeType kSourceElement = IO.TagElementNodeType.Element;
		public const IO.TagElementNodeType kSourceCursor = IO.TagElementNodeType.Text;

		public static void ReadDetermineListSize<TDoc, TCursor, T>(IO.TagElementStream<TDoc, TCursor, string> s, List<T> list)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(s != null);
			Contract.Requires(list != null);
			Contract.Requires(s.IsReading);

			int child_element_count = s.TryGetCursorElementCount();
			if (list.Capacity < child_element_count)
				list.Capacity = child_element_count;
		}

		public static IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, IO.TagElementNodeType xmlSource)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != kNoXmlName));
			Contract.Requires(xmlSource != IO.TagElementNodeType.Attribute);

			return xmlSource == IO.TagElementNodeType.Text
				? s.ElementsByName(xmlName)
				: s.Elements;
		}
	};
}

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public static bool StreamBVector<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref BVector vector
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string string_value = null;
			bool was_streamed = true;
			const bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref string_value, to_lower, xmlSource);
				else
					s.StreamString(xmlName, ref string_value, to_lower, xmlSource);

				if (was_streamed)
				{
					var parse_result = ParseBVectorString(string_value);
					if (!parse_result.HasValue)
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
							"Failed to parse value (hint: {0}) as vector: {1}",
							xmlSource.RequiresName() ? xmlName : "ElementText",
							string_value)));

					vector = parse_result.Value;
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(vector))
				{
					was_streamed = false;
					return was_streamed;
				}

				string_value = vector.ToBVectorString();

				if (isOptional)
					s.StreamStringOpt(xmlName, ref string_value, to_lower, xmlSource);
				else
					s.StreamString(xmlName, ref string_value, to_lower, xmlSource);
			}

			return was_streamed;
		}

		public static bool StreamIntegerColor<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref System.Drawing.Color color
			, byte defaultAlpha = 0xFF
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));

			string string_value = null;
			bool was_streamed = true;
			const bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref string_value, to_lower, xmlSource);
				else
					s.StreamString(xmlName, ref string_value, to_lower, xmlSource);

				if (was_streamed)
				{
					if (!TokenizeIntegerColor(string_value, defaultAlpha, ref color))
						s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
							"Failed to parse value (hint: {0}) as color: {1}",
							xmlSource.RequiresName() ? xmlName : "ElementText",
							string_value)));
				}
			}
			else if (s.IsWriting)
			{
				if (isOptional && PhxPredicates.IsZero(color))
				{
					was_streamed = false;
					return was_streamed;
				}

				string_value = color.ToIntegerColorString(defaultAlpha);

				if (isOptional)
					s.StreamStringOpt(xmlName, ref string_value, to_lower, xmlSource);
				else
					s.StreamString(xmlName, ref string_value, to_lower, xmlSource);
			}

			return was_streamed;
		}

		public static bool StreamProtoEnum<TDoc, TCursor>(this IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, ref int dbid
			, Collections.IProtoEnum protoEnum
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement
			, int isOptionalDefaultValue = TypeExtensions.kNone)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));
			Contract.Requires(protoEnum != null);

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					dbid = protoEnum.TryGetMemberId(id_name);
					Contract.Assert(dbid.IsNotNone(), id_name);
				}
				//else
				//	dbid = isOptionalDefaultValue;
			}
			else if (s.IsWriting)
			{
				if (isOptional && isOptionalDefaultValue.IsNotNone() && isOptionalDefaultValue == dbid)
				{
					was_streamed = false;
					return was_streamed;
				}

				id_name = protoEnum.TryGetMemberName(dbid);
				if (id_name.IsNullOrEmpty())
					Contract.Assert(!id_name.IsNullOrEmpty(), dbid.ToString());

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}
	};
}