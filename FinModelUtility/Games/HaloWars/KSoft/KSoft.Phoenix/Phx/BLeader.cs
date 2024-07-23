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
			get { return mIconName; }
			set { mIconName = value; }
		}
		#endregion

		#region Test
		bool mTest;
		public bool Test
		{
			get { return mTest; }
			set { mTest = value; }
		}
		#endregion

		#region IsExcludedFromAlpha
		int mAlpha = TypeExtensions.kNone;
		public bool IsExcludedFromAlpha
		{
			get { return mAlpha == 0; }
			set { mAlpha = value ? 0 : TypeExtensions.kNone; }
		}
		#endregion

		#region LeaderPickerOrder
		sbyte mLeaderPickerOrder = TypeExtensions.kNone;
		/// <summary>
		/// Leaders with this attribute set to zero or greater (and without the test attribute set) will be sorted by this value and shown in the leader picker.
		/// </summary>
		public sbyte LeaderPickerOrder
		{
			get { return mLeaderPickerOrder; }
			set { mLeaderPickerOrder = value; }
		}
		#endregion

		#region StatsID
		sbyte mStatsID = TypeExtensions.kNone;
		/// <summary>
		/// This identifies leaders for stats/leaderboard purposes, which allows you to reorder the leaders in the XML file without messing up stats/leaderboards.
		/// </summary>
		public sbyte StatsID
		{
			get { return mStatsID; }
			set { mStatsID = value; }
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
			get { return mDefaultPlayerSlotFlags; }
			set { mDefaultPlayerSlotFlags = value; }
		}
		#endregion

		#region IsRandom
		bool mIsRandom;
		public bool IsRandom
		{
			get { return mIsRandom; }
			set { mIsRandom = value; }
		}
		#endregion

		#region TechID
		int mTechID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return mTechID; }
			set { mTechID = value; }
		}
		#endregion

		#region CivID
		int mCivID = TypeExtensions.kNone;
		[Meta.BCivReference]
		public int CivID
		{
			get { return mCivID; }
			set { mCivID = value; }
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
			get { return mFlashCivID; }
			set { mFlashCivID = value; }
		}
		#endregion

		#region LeaderPowerID
		int mPowerID = TypeExtensions.kNone;
		[Meta.BProtoPowerReference]
		public int PowerID
		{
			get { return mPowerID; }
			set { mPowerID = value; }
		}
		#endregion

		#region FlashImg
		string mFlashImg;
		[Meta.UnusedData]
		public string FlashImg
		{
			get { return mFlashImg; }
			set { mFlashImg = value; }
		}
		#endregion

		#region FlashPortrait
		string mFlashPortrait; // img://unknown.ddx
		[Meta.TextureReference]
		public string FlashPortrait
		{
			get { return mFlashPortrait; }
			set { mFlashPortrait = value; }
		}
		#endregion

		public Collections.BListArray<BLeaderSupportPower> SupportPowers { get; private set; }
		public Collections.BListArray<BLeaderStartingSquad> StartingSquads { get; private set; }
		public Collections.BListArray<BLeaderStartingUnit> StartingUnits { get; private set; }

		#region RallyPointOffset
		BVector mRallyPointOffset;
		public BVector RallyPointOffset
		{
			get { return mRallyPointOffset; }
			set { mRallyPointOffset = value; }
		}
		#endregion

		#region RepairRate
		float mRepairRate;
		public float RepairRate
		{
			get { return mRepairRate; }
			set { mRepairRate = value; }
		}
		#endregion

		#region RepairDelay
		float mRepairDelay;
		/// <remarks>In seconds</remarks>
		public float RepairDelay
		{
			get { return mRepairDelay; }
			set { mRepairDelay = value; }
		}
		#endregion

		public Collections.BTypeValuesSingle RepairCost { get; private set; }

		#region RepairTime
		float mRepairTime;
		public float RepairTime
		{
			get { return mRepairTime; }
			set { mRepairTime = value; }
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
			get { return mUIControlBackground; }
			set { mUIControlBackground = value; }
		}
		#endregion

		// Empty Leaders just have a Civ
		public bool IsEmpty { get { return mTechID.IsNone(); } }

		public BLeader()
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasNameID = true;
			textData.HasDescriptionID = true;

			SupportPowers = new Collections.BListArray<BLeaderSupportPower>();
			StartingSquads = new Collections.BListArray<BLeaderStartingSquad>();
			StartingUnits = new Collections.BListArray<BLeaderStartingUnit>();
			RepairCost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			ReverseHotDropCost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			Populations = new Collections.BTypeValues<BPopulation>(BPopulation.kBListParams);
			Resources = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			var xs = s.GetSerializerInterface();

			s.StreamAttributeOpt("Icon", ref mIconName, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("Test", ref mTest, Predicates.IsTrue);
			s.StreamAttributeOpt("Alpha", ref mAlpha, Predicates.IsNotNone);

			s.StreamAttributeOpt("Random", ref mIsRandom, Predicates.IsTrue);
			s.StreamAttributeOpt("StatsID", ref mStatsID, Predicates.IsNotNone);
			s.StreamAttributeOpt("LeaderPickerOrder", ref mLeaderPickerOrder, Predicates.IsNotNone);
			s.StreamAttributeOpt("DefaultPlayerSlotFlags", ref mDefaultPlayerSlotFlags, Predicates.IsNotZero,
				NumeralBase.Hex);

			xs.StreamDBID(s, "Tech", ref mTechID, DatabaseObjectKind.Tech);
			xs.StreamDBID(s, "Civ", ref mCivID, DatabaseObjectKind.Civ);
			xs.StreamDBID(s, "Power", ref mPowerID, DatabaseObjectKind.Power);
			s.StreamElementOpt("FlashCivID", ref mFlashCivID, Predicates.IsNotNone);
			s.StreamStringOpt("FlashImg", ref mFlashImg, toLower: false, type: XML.XmlUtil.kSourceElement);
			// TODO: HW360's FlashPortrait elements have an ending " character (sans a starting quote). Be careful!
			s.StreamStringOpt("FlashPortrait", ref mFlashPortrait, toLower: false, type: XML.XmlUtil.kSourceElement);
			XML.XmlUtil.Serialize(s, SupportPowers, BLeaderSupportPower.kBListXmlParams);
			XML.XmlUtil.Serialize(s, StartingSquads, BLeaderStartingSquad.kBListXmlParams);
			XML.XmlUtil.Serialize(s, StartingUnits, BLeaderStartingUnit.kBListXmlParams);
			s.StreamBVector("RallyPointOffset", ref mRallyPointOffset);
			s.StreamElementOpt("RepairRate", ref mRepairRate, Predicates.IsNotZero);
			s.StreamElementOpt("RepairDelay", ref mRepairDelay, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, RepairCost, kRepairCostTypeValuesXmlParams);
			s.StreamElementOpt("RepairTime", ref mRepairTime, Predicates.IsNotZero);
			XML.XmlUtil.Serialize(s, ReverseHotDropCost, kReverseHotDropCostTypeValuesXmlParams);
			XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParams);
			XML.XmlUtil.Serialize(s, Resources, BResource.kBListTypeValuesXmlParams);
			s.StreamStringOpt("UIControlBackground", ref mUIControlBackground, toLower: false, type: XML.XmlUtil.kSourceElement);
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
			get { return mIconLocation; }
			set { mIconLocation = value; }
		}
		#endregion

		#region TechPrereqID
		int mTechPrereqID = TypeExtensions.kNone;
		[Meta.BProtoTechReference]
		public int TechPrereqID
		{
			get { return mTechPrereqID; }
			set { mTechPrereqID = value; }
		}
		#endregion

		[Meta.BProtoPowerReference]
		public List<BProtoPowerID> SupportPowerIDs { get; private set; } = new List<BProtoSquadID>();

		public bool IsEmpty { get { return SupportPowerIDs.Count == 0; } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute("IconLocation", ref mIconLocation);

			xs.StreamDBID(s, "Power", SupportPowerIDs, DatabaseObjectKind.Power, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderSupportPower other)
		{
			if (IconLocation != other.IconLocation)
				IconLocation.CompareTo(other.IconLocation);

			if (TechPrereqID != other.TechPrereqID)
				TechPrereqID.CompareTo(other.TechPrereqID);

			if (SupportPowerIDs.Count != other.SupportPowerIDs.Count)
				SupportPowerIDs.Count.CompareTo(other.SupportPowerIDs.Count);

			int a_hash = PhxUtil.CalculateHashCodeForDBIDs(SupportPowerIDs);
			int b_hash = PhxUtil.CalculateHashCodeForDBIDs(other.SupportPowerIDs);
			return a_hash.CompareTo(b_hash);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderSupportPower other)
		{
			return IconLocation == other.IconLocation
				&& TechPrereqID == other.TechPrereqID
				&& SupportPowerIDs.EqualsList(other.SupportPowerIDs);
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
			get { return mSquadID; }
			set { mSquadID = value; }
		}
		#endregion

		#region FlyIn
		bool mFlyIn;
		public bool FlyIn
		{
			get { return mFlyIn; }
			set { mFlyIn = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return mOffset; }
			set { mOffset = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(SquadID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref mOffset, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamAttributeOpt("FlyIn", ref mFlyIn, Predicates.IsTrue);

			XML.BXmlSerializerInterface.StreamSquadID(s, xs, ref mSquadID);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingSquad other)
		{
			if (FlyIn != other.FlyIn)
				FlyIn.CompareTo(other.FlyIn);

			if (Offset != other.Offset)
				Offset.CompareTo(other.Offset);

			return SquadID.CompareTo(other.SquadID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingSquad other)
		{
			return FlyIn == other.FlyIn
				&& Offset == other.Offset
				&& SquadID == other.SquadID;
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
			get { return mDoppleOnStart; }
			set { mDoppleOnStart = value; }
		}
		#endregion

		#region ObjectTypeID
		int mObjectTypeID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int ObjectTypeID
		{
			get { return mObjectTypeID; }
			set { mObjectTypeID = value; }
		}
		#endregion

		#region BuildOtherID
		int mBuildOtherID = TypeExtensions.kNone;
		[Meta.BProtoObjectReference]
		public int BuildOtherID
		{
			get { return mBuildOtherID; }
			set { mBuildOtherID = value; }
		}
		#endregion

		#region Offset
		BVector mOffset;
		public BVector Offset
		{
			get { return mOffset; }
			set { mOffset = value; }
		}
		#endregion

		public bool IsInvalid { get { return PhxUtil.IsUndefinedReferenceHandleOrNone(ObjectTypeID); } }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamBVector("Offset", ref mOffset, xmlSource: XML.XmlUtil.kSourceAttr);

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mObjectTypeID, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceCursor);
			xs.StreamDBID(s, "BuildOther", ref mBuildOtherID, DatabaseObjectKind.Object, xmlSource: XML.XmlUtil.kSourceAttr);

			s.StreamAttributeOpt("DoppleOnStart", ref mDoppleOnStart, Predicates.IsTrue);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(BLeaderStartingUnit other)
		{
			if (DoppleOnStart != other.DoppleOnStart)
				DoppleOnStart.CompareTo(other.DoppleOnStart);

			if (Offset != other.Offset)
				Offset.CompareTo(other.Offset);

			if (ObjectTypeID != other.ObjectTypeID)
				ObjectTypeID.CompareTo(other.ObjectTypeID);

			return BuildOtherID.CompareTo(other.BuildOtherID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(BLeaderStartingUnit other)
		{
			return DoppleOnStart == other.DoppleOnStart
				&& Offset == other.Offset
				&& ObjectTypeID == other.ObjectTypeID
				&& BuildOtherID == other.BuildOtherID;
		}
		#endregion
	};
}
