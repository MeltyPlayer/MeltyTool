
namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoObjectTrainLimit
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TrainLimit",
		};
		#endregion

		#region Type
		LimitType mType = LimitType.Invalid;
		public LimitType Type
		{
			get { return this.mType; }
			set { this.mType = value; }
		}
		#endregion

		#region ID
		int mID = TypeExtensions.kNone;
		public int ID
		{
			get { return this.mID; }
			set { this.mID = value; }
		}
		#endregion

		#region Count
		int mCount;
		public int Count
		{
			get { return this.mCount; }
			set { this.mCount = value; }
		}

		public bool IsCountValid { get { return this.Count >= byte.MinValue && this.Count < byte.MaxValue; } }
		#endregion

		#region Bucket
		int mBucket;
		public int Bucket
		{
			get { return this.mBucket; }
			set { this.mBucket = value; }
		}

		public bool IsBucketValid { get { return this.Bucket >= byte.MinValue && this.Bucket < byte.MaxValue; } }
		#endregion

		public bool IsValid { get {
			return this.Type != LimitType.Invalid
				&&
				this.ID.IsNotNone()
				&&
				this.IsCountValid
				&&
				this.IsBucketValid;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("Type", ref this.mType);

			switch (this.mType)
			{
				case LimitType.Unit:
					xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
					break;
				case LimitType.Squad:
					xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
					break;
			}

			s.StreamAttributeOpt("Count", ref this.mCount, Predicates.IsNotZero);
			s.StreamAttributeOpt("Bucket", ref this.mBucket, Predicates.IsNotZero);
		}
		#endregion
	};
}