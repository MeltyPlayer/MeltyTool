using Contracts = System.Diagnostics.Contracts;

namespace KSoft.Phoenix.XML
{
	public class BListXmlParams : BCollectionXmlParams
	{
		public /*readonly*/ string DataName;

		#region Flags
		[Contracts.Pure]
		public bool InternDataNames { get { return this.HasFlag(BCollectionXmlParamsFlags.InternDataNames); } }
		[Contracts.Pure]
		public bool UseInnerTextForData { get { return this.HasFlag(BCollectionXmlParamsFlags.UseInnerTextForData); } }
		[Contracts.Pure]
		public bool UseElementForData { get { return this.HasFlag(BCollectionXmlParamsFlags.UseElementForData); } }
		[Contracts.Pure]
		public bool ToLowerDataNames { get { return this.HasFlag(BCollectionXmlParamsFlags.ToLowerDataNames); } }
		[Contracts.Pure]
		public bool RequiresDataNamePreloading { get { return this.HasFlag(BCollectionXmlParamsFlags.RequiresDataNamePreloading); } }
		[Contracts.Pure]
		public bool SupportsUpdating { get { return this.HasFlag(BCollectionXmlParamsFlags.SupportsUpdating); } }
		[Contracts.Pure]
		public bool DoNotWriteUndefinedData { get { return this.HasFlag(BCollectionXmlParamsFlags.DoNotWriteUndefinedData); } }
		#endregion

		public BListXmlParams() { }
		/// <summary>Sets RootName to plural of ElementName and sets UseInnerTextForData</summary>
		/// <param name="elementName"></param>
		/// <param name="additionalFlags"></param>
		public BListXmlParams(string elementName, BCollectionXmlParamsFlags additionalFlags = 0) : base(elementName)
		{
			this.Flags = additionalFlags;
			this.Flags |= BCollectionXmlParamsFlags.UseInnerTextForData;
		}

		public void StreamDataName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref string name)
			where TDoc : class
			where TCursor : class
		{
			StreamValue(s,
			            this.DataName, ref name,
			            this.UseInnerTextForData,
			            this.UseElementForData,
			            this.InternDataNames,
				false/*ToLowerDataNames*/);
		}
	};
}