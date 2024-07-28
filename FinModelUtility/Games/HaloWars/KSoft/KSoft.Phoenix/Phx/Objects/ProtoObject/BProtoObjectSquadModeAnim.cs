using System;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoObjectSquadModeAnim
		: IO.ITagElementStringNameStreamable
		, IComparable<BProtoObjectSquadModeAnim>
		, IEquatable<BProtoObjectSquadModeAnim>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "SquadModeAnim",
		};
		#endregion

		#region Mode
		BSquadMode mMode;
		public BSquadMode Mode
		{
			get { return this.mMode; }
			set { this.mMode = value; }
		}
		#endregion

		#region AnimType
		string mAnimType;
		[Meta.BAnimTypeReference]
		public string AnimType
		{
			get { return this.mAnimType; }
			set { this.mAnimType = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum("Mode", ref this.mMode);
			s.StreamCursor(ref this.mAnimType);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BProtoObjectSquadModeAnim other)
		{
			if (this.Mode != other.Mode)
				this.Mode.CompareTo(other.Mode);

			return this.AnimType.CompareTo(other.AnimType);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BProtoObjectSquadModeAnim other)
		{
			return this.Mode == other.Mode
				&&
				this.AnimType == other.AnimType;
		}
		#endregion
	};
}