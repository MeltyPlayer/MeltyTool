
namespace KSoft.Phoenix.Phx
{
	// #TODO we shouldn't serialize out hardpoints without a name, the engine ASSERTs
	public sealed class BHardpoint
		: IO.ITagElementStringNameStreamable
	{
		const double cPiOver12 = (float)((1.0 / 12.0) * System.Math.PI);
		const float cPiOver12InDegrees = (float)(cPiOver12 * TypeExtensions.kDegreesPerRadian);

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Hardpoint",
		};
		#endregion

		#region Name
		string mName;
		public string Name
		{
			get { return this.mName; }
			set { this.mName = value; }
		}
		#endregion

		#region YawAttachment
		string mYawAttachment;
		[Meta.AttachmentTypeReference]
		public string YawAttachment
		{
			get { return this.mYawAttachment; }
			set { this.mYawAttachment = value; }
		}
		#endregion

		#region PitchAttachment
		string mPitchAttachment;
		[Meta.AttachmentTypeReference]
		public string PitchAttachment
		{
			get { return this.mPitchAttachment; }
			set { this.mPitchAttachment = value; }
		}
		#endregion

		#region AutoCenter
		bool mAutoCenter = true;
		public bool AutoCenter
		{
			get { return this.mAutoCenter; }
			set { this.mAutoCenter = value; }
		}
		#endregion

		#region SingleBoneIK
		bool mSingleBoneIK;
		public bool SingleBoneIK
		{
			get { return this.mSingleBoneIK; }
			set { this.mSingleBoneIK = value; }
		}
		#endregion

		#region Combined
		bool mCombined;
		public bool Combined
		{
			get { return this.mCombined; }
			set { this.mCombined = value; }
		}
		#endregion

		#region HardPitchLimits
		bool mHardPitchLimits;
		public bool HardPitchLimits
		{
			get { return this.mHardPitchLimits; }
			set { this.mHardPitchLimits = value; }
		}
		#endregion

		#region RelativeToUnit
		bool mRelativeToUnit;
		public bool RelativeToUnit
		{
			get { return this.mRelativeToUnit; }
			set { this.mRelativeToUnit = value; }
		}
		#endregion

		#region UseYawAndPitchAsTolerance
		bool mUseYawAndPitchAsTolerance;
		public bool UseYawAndPitchAsTolerance
		{
			get { return this.mUseYawAndPitchAsTolerance; }
			set { this.mUseYawAndPitchAsTolerance = value; }
		}
		#endregion

		#region InfiniteRateWhenHasTarget
		bool mInfiniteRateWhenHasTarget;
		public bool InfiniteRateWhenHasTarget
		{
			get { return this.mInfiniteRateWhenHasTarget; }
			set { this.mInfiniteRateWhenHasTarget = value; }
		}
		#endregion

		#region YawRotationRate
		float mYawRotationRate = cPiOver12InDegrees;
		// angle
		public float YawRotationRate
		{
			get { return this.mYawRotationRate; }
			set { this.mYawRotationRate = value; }
		}
		#endregion

		#region PitchRotationRate
		float mPitchRotationRate = cPiOver12InDegrees;
		// angle
		public float PitchRotationRate
		{
			get { return this.mPitchRotationRate; }
			set { this.mPitchRotationRate = value; }
		}
		#endregion

		#region YawLeftMaxAngle
		const float cDefaultYawLeftMaxAngle = (float)-(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mYawLeftMaxAngle = cDefaultYawLeftMaxAngle;
		// angle
		public float YawLeftMaxAngle
		{
			get { return this.mYawLeftMaxAngle; }
			set { this.mYawLeftMaxAngle = value; }
		}
		#endregion

		#region YawRightMaxAngle
		const float cDefaultYawRightMaxAngle = (float)+(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mYawRightMaxAngle = cDefaultYawRightMaxAngle;
		// angle
		public float YawRightMaxAngle
		{
			get { return this.mYawRightMaxAngle; }
			set { this.mYawRightMaxAngle = value; }
		}
		#endregion

		#region PitchMaxAngle
		const float cDefaultPitchMaxAngle = (float)-(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mPitchMaxAngle = cDefaultPitchMaxAngle;
		// angle
		public float PitchMaxAngle
		{
			get { return this.mPitchMaxAngle; }
			set { this.mPitchMaxAngle = value; }
		}
		#endregion

		#region PitchMinAngle
		const float cDefaultPitchMinAngle = (float)+(System.Math.PI * TypeExtensions.kDegreesPerRadian);

		float mPitchMinAngle = cDefaultPitchMinAngle;
		// angle
		public float PitchMinAngle
		{
			get { return this.mPitchMinAngle; }
			set { this.mPitchMinAngle = value; }
		}
		#endregion

		#region StartYawSound
		string mStartYawSound;
		[Meta.SoundCueReference]
		public string StartYawSound
		{
			get { return this.mStartYawSound; }
			set { this.mStartYawSound = value; }
		}
		#endregion

		#region StopYawSound
		string mStopYawSound;
		[Meta.SoundCueReference]
		public string StopYawSound
		{
			get { return this.mStopYawSound; }
			set { this.mStopYawSound = value; }
		}
		#endregion

		#region StartPitchSound
		string mStartPitchSound;
		[Meta.SoundCueReference]
		public string StartPitchSound
		{
			get { return this.mStartPitchSound; }
			set { this.mStartPitchSound = value; }
		}
		#endregion

		#region StopPitchSound
		string mStopPitchSound;
		[Meta.SoundCueReference]
		public string StopPitchSound
		{
			get { return this.mStopPitchSound; }
			set { this.mStopPitchSound = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref this.mName);
			s.StreamStringOpt("yawattachment", ref this.mYawAttachment, toLower: false);
			s.StreamStringOpt("pitchattachment", ref this.mPitchAttachment, toLower: false);
			s.StreamAttributeOpt("autocenter", ref this.mAutoCenter, Predicates.IsFalse);
			s.StreamAttributeOpt("singleboneik", ref this.mSingleBoneIK, Predicates.IsTrue);
			s.StreamAttributeOpt("combined", ref this.mCombined, Predicates.IsTrue);
			s.StreamAttributeOpt("hardpitchlimits", ref this.mHardPitchLimits, Predicates.IsTrue);
			s.StreamAttributeOpt("relativeToUnit", ref this.mRelativeToUnit, Predicates.IsTrue);
			s.StreamAttributeOpt("useYawAndPitchAsTolerance", ref this.mUseYawAndPitchAsTolerance, Predicates.IsTrue);
			s.StreamAttributeOpt("infiniteRateWhenHasTarget", ref this.mInfiniteRateWhenHasTarget, Predicates.IsTrue);
			s.StreamAttributeOpt("yawrate", ref this.mYawRotationRate, x => x != cPiOver12InDegrees);
			s.StreamAttributeOpt("pitchrate", ref this.mPitchRotationRate, x => x != cPiOver12InDegrees);

			#region YawLeftMaxAngle and YawRightMaxAngle
			if (s.IsReading)
			{
				float yawMaxAngle = PhxUtil.kInvalidSingleNaN;
				if (s.ReadAttributeOpt("yawMaxAngle", ref yawMaxAngle) ||
					// #HACK fucking deal with original HW game data that was hand edited, but only when reading
					s.ReadAttributeOpt("yawmaxangle", ref yawMaxAngle))
				{
					this.mYawLeftMaxAngle = -yawMaxAngle;
					this.mYawRightMaxAngle = yawMaxAngle;
				}

				s.StreamAttributeOpt("YawLeftMaxAngle", ref this.mYawLeftMaxAngle, x => x != cDefaultYawLeftMaxAngle);
				s.StreamAttributeOpt("YawRightMaxAngle", ref this.mYawRightMaxAngle, x => x != cDefaultYawRightMaxAngle);
			}
			else if (s.IsWriting)
			{
				if (this.mYawLeftMaxAngle == cDefaultYawLeftMaxAngle &&
				    this.mYawRightMaxAngle == cDefaultYawRightMaxAngle)
				{
					// don't stream anything
				}
				else if (System.Math.Abs(this.mYawLeftMaxAngle) == this.mYawRightMaxAngle)
				{
					s.WriteAttribute("yawMaxAngle", this.mYawRightMaxAngle);
				}
				else
				{
					s.StreamAttributeOpt("YawLeftMaxAngle", ref this.mYawLeftMaxAngle, x => x != cDefaultYawLeftMaxAngle);
					s.StreamAttributeOpt("YawRightMaxAngle", ref this.mYawRightMaxAngle, x => x != cDefaultYawRightMaxAngle);
				}
			}
			#endregion

			s.StreamAttributeOpt("pitchMaxAngle", ref this.mPitchMaxAngle, x => x != cDefaultPitchMaxAngle);
			s.StreamAttributeOpt("pitchMinAngle", ref this.mPitchMinAngle, x => x != cDefaultPitchMinAngle);

			s.StreamStringOpt("StartYawSound", ref this.mStartYawSound, toLower: false);
			s.StreamStringOpt("StopYawSound", ref this.mStopYawSound, toLower: false);
			s.StreamStringOpt("StartPitchSound", ref this.mStartPitchSound, toLower: false);
			s.StreamStringOpt("StopPitchSound", ref this.mStopPitchSound, toLower: false);
		}
		#endregion
	};
}