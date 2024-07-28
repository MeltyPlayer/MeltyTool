using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectDamageType
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectDamageType>
		, IEquatable<BProtoObjectDamageType>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "DamageType",
		};
		#endregion

		#region DamageType
		int mDamageType = TypeExtensions.kNone;
		[Meta.BDamageTypeReference]
		public int DamageType
		{
			get { return this.mDamageType; }
			set { this.mDamageType = value; }
		}
		#endregion

		#region Direction
		DamageDirection mDirection = DamageDirection.Invalid;
		public DamageDirection Direction
		{
			get { return this.mDirection; }
			set { this.mDirection = value; }
		}
		#endregion

		#region Mode
		BSquadMode mMode = BSquadMode.Normal;
		public BSquadMode Mode
		{
			get { return this.mMode; }
			set { this.mMode = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDamageType, DatabaseObjectKind.DamageType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
			s.StreamAttributeEnum("direction", ref this.mDirection);
			s.StreamAttributeEnumOpt("mode", ref this.mMode, e => e != BSquadMode.Normal);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectDamageType other)
		{
			if (this.DamageType != other.DamageType)
				this.DamageType.CompareTo(other.DamageType);

			if (this.Direction != other.Direction)
				this.Direction.CompareTo(other.Direction);

			return this.Mode.CompareTo(other.Mode);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectDamageType other)
		{
			return this.DamageType == other.DamageType
				&&
				this.Direction == other.Direction
				&&
				this.Mode == other.Mode;
		}
		#endregion
	};
}