using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.XML
{
	public abstract class BCollectionXmlParams
	{
		/// <summary>Root element name in the XML</summary>
		public /*readonly*/ string RootName;
		/// <summary>Name of the elements, that appear under the root element, and host our values</summary>
		public /*readonly*/ string ElementName;

		/// <summary>Do we explicitly filter the XML tags to match <see cref="ElementName"/>?</summary>
		public bool UseElementName { get { return this.ElementName != null; } }

		#region Flags
		public /*readonly*/ BCollectionXmlParamsFlags Flags;

		[Contracts.Pure]
		protected bool HasFlag(BCollectionXmlParamsFlags flag) { return (this.Flags & flag) == flag; }

		public void SetForceNoRootElementStreaming(bool isSet)
		{
			if (isSet)
				this.Flags |= BCollectionXmlParamsFlags.ForceNoRootElementStreaming;
			else
				this.Flags &= ~BCollectionXmlParamsFlags.ForceNoRootElementStreaming;
		}
		#endregion

		public string GetOptionalRootName()
		{
			if (!this.HasFlag(BCollectionXmlParamsFlags.ForceNoRootElementStreaming))
				return this.RootName;

			return null;
		}

		protected BCollectionXmlParams() {}
		/// <summary>Sets RootName to plural of ElementName</summary>
		/// <param name="elementName"></param>
		protected BCollectionXmlParams(string elementName)
		{
			this.RootName = elementName + "s";
			this.ElementName = elementName;
		}

		#region IO.TagElementStream util
		protected static void StreamValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string valueName, ref string value,
			bool useInnerText, bool useElement, bool internValue, bool toLower)
			where TDoc : class
			where TCursor : class
		{
				 if (useInnerText)		s.StreamCursor(ref value);
			else if (useElement)		s.StreamElement(valueName, ref value);
			else if (valueName != null)	s.StreamAttribute(valueName, ref value);

			if (s.IsReading)
			{
				if (toLower) value = value.ToLowerInvariant();
				if (internValue) value = string.Intern(value);
			}
		}
		protected static void StreamValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string valueName, ref int value,
			bool useInnerText, bool useElement)
			where TDoc : class
			where TCursor : class
		{
				 if (useInnerText)		s.StreamCursor(ref value);
			else if (useElement)		s.StreamElement(valueName, ref value);
			else if (valueName != null)	s.StreamAttribute(valueName, ref value);
		}
		#endregion
	};
}