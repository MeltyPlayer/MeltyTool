using System;

namespace KSoft.Phoenix.Phx
{
	/// <summary>Script objects which map to a editor-only database</summary>
	public abstract class TriggerScriptObject : TriggerScriptCodeObject
	{
		#region Xml constants
		protected const string kXmlAttrType = "Type";
		const string kXmlAttrDbId = "DBID";
		const string kXmlAttrVersion = "Version";
		#endregion

//		string mTypeStr; // TODO: temporary!
		int mDbId = TypeExtensions.kNone;
		public int DbId { get { return this.mDbId; } }

		int mVersion = TypeExtensions.kNone;
		public int Version { get { return this.mVersion; } }

		protected void StreamType<TTypeEnum, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref TTypeEnum type)
			where TTypeEnum : struct, IComparable, IFormattable, IConvertible
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum(kXmlAttrType, ref type);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(kXmlAttrDbId, ref this.mDbId);
			s.StreamAttribute(kXmlAttrVersion, ref this.mVersion);
			// Stream it last, so when we save it ourselves, the (relatively) fixed width stuff comes first
//			XML.XmlUtil.StreamInternString(s, kXmlAttrType, ref mTypeStr, false);
		}
	};
}