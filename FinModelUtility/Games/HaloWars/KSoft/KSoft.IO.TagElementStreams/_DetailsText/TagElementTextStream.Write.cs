
namespace KSoft.IO
{
	partial class TagElementTextStream<TDoc, TCursor>
	{
		#region WriteElement impl
		protected override void WriteElementEnum<TEnum>(TCursor n, TEnum value, bool isFlags)
		{
			this.WriteElement(n, isFlags ?
				                  Text.Util.EnumToFlagsString(value) :
				                  Text.Util.EnumToString(value));
		}

		protected override void WriteElement(TCursor n, Values.KGuid value)
		{
			this.WriteElement(n, value.ToString(this.mGuidFormatString, Util.InvariantCultureInfo));
		}
		#endregion

		#region WriteAttribute
		protected abstract void CursorWriteAttribute(string name, string value);

		public override void WriteAttributeEnum<TEnum>(string name, TEnum value, bool isFlags = false)
		{
			this.CursorWriteAttribute(name, isFlags ?
				                          Text.Util.EnumToFlagsString(value) :
				                          Text.Util.EnumToString(value));
		}

		public override void WriteAttribute(string name, Values.KGuid value)
		{
			this.CursorWriteAttribute(name, value.ToString(this.mGuidFormatString, Util.InvariantCultureInfo));
		}
		#endregion
	};
}
