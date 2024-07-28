using System;
using System.Collections.Generic;

using BProtoSquadID = System.Int32;
using BProtoPowerID = System.Int32;
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Phx
{
	public sealed class BLeader
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Leader")
		{
			DataName = "Name",
			Flags = 0
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Leaders.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo);

		static readonly XML.BTypeValuesXmlParams<float> kRepairCostTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("RepairCost", "Type");
		static readonly XML.BTypeValuesXmlParams<float> kReverseHotDropCostTypeValuesXmlParams = new
			XML.BTypeValuesXmlParams<float>("ReverseHotDropCost", "Type");
		#endregion

		#region IconName
		string mIconName;
		[Meta.TextureReference]
		public string IconName
		{
			get { return this.mIconName; }
			set { this.mIconName = value; }
		}
		#endregion

		#region Test
		bool mTest;
		public bool Test
		{
			get { return this.mTest; }
			set { this.mTest = value; }
		}
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha = TypeExtensions.kNone;
		public bool IsExcludedFromAlpha
		{
			get { return this.mAlpha == 0; }
			set { this.mAlpha = value ? 0 : TypeExtensions.kNone; }
		}
		#endregion

		#region LeaderPickerOrder
		sbyte mLeaderPickerOrder = TypeExtensions.kNone;
		/// <summary>
		/// Leaders with this attribute set to zero or greater (and without the test attribute set) will be sorted by this value and shown in the leader picker.
		/// </summary>
		public sbyte LeaderPickerOrder
		{
			get { return this.mLeaderPickerOrder; }
			set { this.mLeaderPickerOrder = value; }
		}
		#endregion

		#region StatsID
		sbyte mStatsID = TypeExtensions.kNone;
		/// <summary>
		/// This identifies leaders for stats/leaderboard purposes, which allows you to reorder the leaders in the XML file without messing up stats/leaderboards.
		/// </summary>
		public sbyte StatsID
		{
			get { return this.mStatsID; }
			set { this.mStatsID = value; }
		}
		#endregion

		#region DefaultPlayerSlotFlags
		byte mDefaultPlayerSlotFlags;
		/// <summary>
		/// Bit flags defining how leaders are assigned to player slots.
		/// Bits 0-5 indicate that the leader is the default leader for the associated slot when AI is assigned to that slot.
		/// Bit 6 is unused.
		/// Bit 7 indicates that the leader is the default leader to assign to a human player when they join a lobby.
		/// If multiple leaders have a particular bit set, the first matching leader will be used.
		/// </summary>
		public byte DefaultPlayerSlotFlags
		{
			get { return this.mDefaultPlayerSlotFlags; }
			set { this.mDefaultPlayerSlotFlags = value; }
		}
		#endregion

		#region IsRandom
		bool mIsRandom;
		public bool IsRandom
		{
			get { return this.mIsRandom; }
			set { this.mIsRandom = value; }
		}
		#endregion

		#region TechID
		int mTechID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return this.mTechID; }
			set { this.mTechID = value; }
		}
		#endregion

		#region CivID
		int mCivID = TypeExtensions.kNone;
		[Meta.BCivReference]
		public int CivID
		{
			get { return this.mCivID; }
			set { this.mCivID = value; }
		}
		#endregion

		#region FlashCivID
		sbyte mFlashCivID = TypeExtensions.kNone;
		/// <summary>
		/// The civilization as specified in the Flash scene. 0 is UNSC, 1 is Covenant, and 2 is Random.
		/// If you figure out how to modify the Flash scene to add other values to the leader picker, this may be useful to you.
		/// </summary>
		[Meta.BCivReference]
		public sbyte FlashCivID
		{
			get { return this.mFlashCivID; }
			set { this.mFlashCivID = value; }
		}
		#endregion

		#region LeaderPowerID
		int mPowerID = TypeExtensions.kNone;
		[Meta.BProtoPowerReference]
		public int PowerID
		{
			get { return this.mPowerID; }
			set { this.mPowerID = value; }
		}
		#endregion

		#region FlashImg
		string mFlashImg;
		[Meta.UnusedData]
		public string FlashImg
		{
			get { return this.mFlashImg; }
			set { this.mFlashImg = value; }
		}
		#endregion

		#region FlashPortrait
		string mFlashPortrait; // img://unknown.ddx
		[Meta.TextureReference]
		public string FlashPortrait
		{
			get { return this.mFlashPortrait; }
			set { this.mFlashPortrait = value; }
		}
		#endregion

		public Collections.BListArray<BLeaderSupportPower> SupportPowers { get; private set; }
		public Collections.BListArray<BLeaderStartingSquad> StartingSquads { get; private set; }
		public Collections.BListArray<BLeaderStartingUnit> StartingUnits { get; private set; }

		#region RallyPointOffset
		BVector mRallyPointOffset;
		public BVector RallyPointOffset
		{
			get { return this.mRallyPointOffset; }
			set { this.mRallyPointOffset = value; }
		}
		#endregion

		#region RepairRate
		float mRepairRate;
		public float RepairRate
		{
			get { return this.mRepairRate; }
			set { this.mRepairRate = value; }
		}
		#endregion

		#region RepairDelay
		float mRepairDelay;
		/// <remarks>In seconds</remarks>
		public float RepairDelay
		{
			get { return this.mRepairDelay; }
			set { this.mRepairDelay = value; }
		}
		#endregion

		public Collections.BTypeValuesSingle RepairCost { get; private set; }

		#region RepairTime
		float mRepairTime;
		public float RepairTime
		{
			get { return this.mRepairTime; }
			set { this.mRepairTime = value; }
		}
		#endregion

		public Collections.BTypeValuesSingle ReverseHotDropCost { get; private set; }

		public Collections.BTypeValues<BPopulation> Populations { get; private set; }

		/// <summary>Initial resources and which resources are considered 'active'</summary>
		public Collections.BTypeValuesSingle Resources { get; private set; }

		#region UIControlBackground
		string mUIControlBackground;
		[Meta.TextureReference]
		public string UIControlBackground
		{
			get { return this.mUIControlBackground; }
			set { this.mUIControlBackground = value; }
		}
		#endregion

		// Empty Leaders just have a Civ
		public bool IsEmpty { get { return this.mTechID.IsNone(); } }

		public BLeader()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasNameID = true;
			textData.HasDescriptionID = true;

			this.SupportPowers = new Collections.BListArray<BLeaderSupportPower>();
			this.StartingSquads = new Collections.BListArray<BLeaderStartingSquad>();
			this.StartingUnits = new Collections.BListArray<BLeaderStartingUnit>();
			this.RepairCost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			this.ReverseHotDropCost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			this.Populations = new Collections.BTypeValues<BPopulation>(BPopulation.kBListParams);
			this.Resources = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Icon", ref this.mIconName, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("Test", ref this.mTest, Predicates.IsTrue);
			s.StreamAttributeOpt("Alpha", ref this.mAlpha, Predicates.IsNotNone);

			s.StreamAttributeOpt("Random", ref this.mIsRandom, Predicates.IsTrue);
			s.StreamAttributeOpt("StatsID", ref this.mStatsID, Predicates.IsNotNone);
			s.StreamAttributeOpt("LeaderPickerOrder", ref this.mLeaderPickerOrder, Predicates.IsNotNone);
			s.StreamAttributeOpt("DefaultPlayerSlotFlags", ref this.mDefaultPlayerSlotFlags, Predicates.IsNotZero,
				NumeralBase.Hex);

			xs.StreamDBID(s, "Tech", ref this.mTechID, DatabaseObjectKind.Tech);
			xs.StreamDBID(s, "Civ", ref this.mCivID, DatabaseObjectKind.Civ);
			xs.StreamDBID(s, "Power", ref this.mPowerID, DatabaseObjectKind.Power);
			s.StreamElementOpt("FlashCivID", ref this.mFlashCivID, Predicates.IsNotNone);
			s.StreamStringOpt("FlashImg", ref this.mFlashImg, toLower: false, type: XML.XmlUtil.kSourceElement);
			// TODO: HW360's FlashPortrait elements have an ending " character (sans a starting quote). Be careful!
			s.StreamStringOpt("FlashPortrait", ref this.mFlashPortrait, toLower: false, type: XML.XmlUtil.kSourceElement);
			XML.XmlUtil.Serialize(s, this.SupportPowers, BLeaderSupportPower.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.StartingSquads, BLeaderStartingSquad.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.StartingUnits, BLeaderStartingUnit.kBListXmlParams);
			s.StreamBVector("RallyPointOffset", ref this.mRallyPointOffset);
			s.StreamElementOpt("RepairRate", ref this.mRepairRate, Predicates.IsNotZero);
			s.StreamElementOpt("RepairDelay", ref this.mRepairDelay, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.RepairCost, kRepairCostTypeValuesXmlParams);
			s.StreamElementOpt("RepairTime", ref this.mRepairTime, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, this.ReverseHotDropCost, kReverseHotDropCostTypeValuesXmlParams);
			XML.XmlUtil.Serialize(s, this.Populations, BPopulation.kBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Resources, BResource.kBListTypeValuesXmlParams);
			s.StreamStringOpt("UIControlBackground", ref this.mUIControlBackground, toLower: false, type: XML.XmlUtil.kSourceElement);
		}
		#endregion
	};

	public sealed class BLeaderSupportPower
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderSupportPower>
		, IEquatable<BLeaderSupportPower>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "SupportPower",
		};
		#endregion

		#region IconLocation
		int mIconLocation;
		public int IconLocation
		{
			get { return this.mIconLocation; }
			set { this.mIconLocation = value; }
		}
		#endregion

		#region TechPrereqID
		int mTechPrereqID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechPrereqID
		{
			get { return this.mTechPrereqID; }
			set { this.mTechPrereqID = value; }
		}
		#endregion

		[Meta.BProtoPowerReference]
		public List<BProtoPowerID> SupportPowerIDs { get; private set; } = new List<BProtoSquadID>();

		public bool IsEmpty { get { return this.SupportPowerIDs.Count == 0; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("IconLocation", ref this.mIconLocation);

			xs.StreamDBID(s, "Power", this.SupportPowerIDs, DatabaseObjectKind.Power, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderSupportPower other)
		{
			if (this.IconLocation != other.IconLocation)
				this.IconLocation.CompareTo(other.IconLocation);

			if (this.TechPrereqID != other.TechPrereqID)
				this.TechPrereqID.CompareTo(other.TechPrereqID);

			if (this.SupportPowerIDs.Count != other.SupportPowerIDs.Count)
				this.SupportPowerIDs.Count.CompareTo(other.SupportPowerIDs.Count);

			int a_hash = PhxUtil.CalculateHashCodeForDBIDs(this.SupportPowerIDs);
			int b_hash = PhxUtil.CalculateHashCodeForDBIDs(other.SupportPowerIDs);
			return a_hash.CompareTo(b_hash);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderSupportPower other)
		{
			return this.IconLocation == other.IconLocation
				&&
				this.TechPrereqID == other.TechPrereqID
				&&
				this.SupportPowerIDs.EqualsList(other.SupportPowerIDs);
		}
		#endregion
	};

	public sealed class BLeaderStartingSquad
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderStartingSquad>
		, IEquatable<BLeaderStartingSquad>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "StartingSquad",
		};
		#endregion

		#region SquadID
		int mSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int SquadID
		{
			get { return this.mSquadID; }
			set { this.mSquadID = value; }
		}
		#endregion

		#region FlyIn
		bool mFlyIn;
		public bool FlyIn
		{
			get { return this.mFlyIn; }
			set { this.mFlyIn = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return this.mOffset; }
			set { this.mOffset = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.SquadID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref this.mOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamAttributeOpt("FlyIn", ref this.mFlyIn, Predicates.IsTrue);

			XML.BXmlSerializerInterface.StreamSquadID(s, xs, ref this.mSquadID);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingSquad other)
		{
			if (this.FlyIn != other.FlyIn)
				this.FlyIn.CompareTo(other.FlyIn);

			if (this.Offset != other.Offset)
				this.Offset.CompareTo(other.Offset);

			return this.SquadID.CompareTo(other.SquadID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingSquad other)
		{
			return this.FlyIn == other.FlyIn
				&&
				this.Offset == other.Offset
				&&
				this.SquadID == other.SquadID;
		}
		#endregion
	};

	public sealed class BLeaderStartingUnit
		: IO.ITagElementStringNameStreamable
		, IComparable<BLeaderStartingUnit>
		, IEquatable<BLeaderStartingUnit>
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "StartingUnit",
		};
		#endregion

		#region DoppleOnStart
		bool mDoppleOnStart;
		public bool DoppleOnStart
		{
			get { return this.mDoppleOnStart; }
			set { this.mDoppleOnStart = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int ObjectTypeID
		{
			get { return this.mObjectTypeID; }
			set { this.mObjectTypeID = value; }
		}
		#endregion

		#region BuildOtherID
		int mBuildOtherID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BuildOtherID
		{
			get { return this.mBuildOtherID; }
			set { this.mBuildOtherID = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return this.mOffset; }
			set { this.mOffset = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(this.ObjectTypeID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref this.mOffset, xmlSource: XML.XmlUtil.kSourceAttr);

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mObjectTypeID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
			xs.StreamDBID(s, "BuildOther", ref this.mBuildOtherID, DatabaseObjectKind.Object, xmlSource: XML.XmlUtil.kSourceAttr);

			s.StreamAttributeOpt("DoppleOnStart", ref this.mDoppleOnStart, Predicates.IsTrue);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingUnit other)
		{
			if (this.DoppleOnStart != other.DoppleOnStart)
				this.DoppleOnStart.CompareTo(other.DoppleOnStart);

			if (this.Offset != other.Offset)
				this.Offset.CompareTo(other.Offset);

			if (this.ObjectTypeID != other.ObjectTypeID)
				this.ObjectTypeID.CompareTo(other.ObjectTypeID);

			return this.BuildOtherID.CompareTo(other.BuildOtherID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingUnit other)
		{
			return this.DoppleOnStart == other.DoppleOnStart
				&&
				this.Offset == other.Offset
				&&
				this.ObjectTypeID == other.ObjectTypeID
				&&
				this.BuildOtherID == other.BuildOtherID;
		}
		#endregion
	};
}
