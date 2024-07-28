
namespace KSoft.Phoenix.Phx
{
	public sealed class BRumbleEvent
		: IO.ITagElementStringNameStreamable
	{
		#region LeftRumbleType
		BRumbleType mLeftRumbleType;
		public BRumbleType LeftRumbleType
		{
			get { return this.mLeftRumbleType; }
			set { this.mLeftRumbleType = value; }
		}
		#endregion

		#region RightRumbleType
		BRumbleType mRightRumbleType;
		public BRumbleType RightRumbleType
		{
			get { return this.mRightRumbleType; }
			set { this.mRightRumbleType = value; }
		}
		#endregion

		#region Duration
		float mDuration;
		public float Duration
		{
			get { return this.mDuration; }
			set { this.mDuration = value; }
		}
		#endregion

		#region LeftStrength
		float mLeftStrength;
		public float LeftStrength
		{
			get { return this.mLeftStrength; }
			set { this.mLeftStrength = value; }
		}
		#endregion

		#region RightStrength
		float mRightStrength;
		public float RightStrength
		{
			get { return this.mRightStrength; }
			set { this.mRightStrength = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnumOpt("LeftRumbleType", ref this.mLeftRumbleType, e => e != BRumbleType.None);
			s.StreamAttributeEnumOpt("RightRumbleType", ref this.mRightRumbleType, e => e != BRumbleType.None);
			s.StreamAttributeOpt("Duration", ref this.mDuration, Predicates.IsNotZero);
			s.StreamAttributeOpt("LeftStrength", ref this.mLeftStrength, Predicates.IsNotZero);
			s.StreamAttributeOpt("RightStrength", ref this.mRightStrength, Predicates.IsNotZero);
		}
		#endregion
	};
}