
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTerrainImpactDecalHandle
		: IO.ITagElementStringNameStreamable
	{
		public enum OrientationType
		{
			Random,
			Forward,
		};

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "ImpactDecal",
		};
		#endregion

		#region Size
		BVector mSize = new BVector(2.0f, 0, 2.0f, 0);
		public BVector Size
		{
			get { return this.mSize; }
			set { this.mSize = value; }
		}
		#endregion

		#region TimeFullyOpaque
		float mTimeFullyOpaque = 5.0f;
		public float TimeFullyOpaque
		{
			get { return this.mTimeFullyOpaque; }
			set { this.mTimeFullyOpaque = value; }
		}
		#endregion

		#region FadeOutTime
		float mFadeOutTime = 10.0f;
		public float FadeOutTime
		{
			get { return this.mFadeOutTime; }
			set { this.mFadeOutTime = value; }
		}
		#endregion

		#region Orientation
		OrientationType mOrientation = OrientationType.Random;
		public OrientationType Orientation
		{
			get { return this.mOrientation; }
			set { this.mOrientation = value; }
		}
		#endregion

		#region TextureName
		string mTextureName;
		[Meta.TextureReference]
		public string TextureName
		{
			get { return this.mTextureName; }
			set { this.mTextureName = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeOpt("sizeX", ref this.mSize.X, f => f != 2.0f);
			s.StreamAttributeOpt("sizeZ", ref this.mSize.Z, f => f != 2.0f);
			s.StreamAttributeOpt("timeFullyOpaque", ref this.mTimeFullyOpaque, f => f != 5.0f);
			s.StreamAttributeOpt("fadeOutTime", ref this.mFadeOutTime, f => f != 10.0f);
			s.StreamAttributeEnumOpt("orientation", ref this.mOrientation, e => e != OrientationType.Random);
			s.StreamCursor(ref this.mTextureName);
		}
		#endregion
	};
}