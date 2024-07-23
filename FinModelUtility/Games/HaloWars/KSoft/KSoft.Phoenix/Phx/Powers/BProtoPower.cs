using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPower
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Power")
		{
			DataName = DatabaseNamedObject.kXmlAttrName,
			Flags = XML.BCollectionXmlParamsFlags.RequiresDataNamePreloading
		};
		public static readonly Engine.XmlFileInfo kXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.Data,
			FileName = "Powers.xml",
			RootName = kBListXmlParams.RootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo kProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.ProtoData,
			kXmlFileInfo);

		static readonly Collections.CodeEnum<BPowerFlags> kFlagsProtoEnum = new Collections.CodeEnum<BPowerFlags>();
		static readonly Collections.BBitSetParams kFlagsParams = new Collections.BBitSetParams(() => kFlagsProtoEnum);

		static readonly Collections.CodeEnum<BPowerToggableFlags> kFlags2ProtoEnum = new Collections.CodeEnum<BPowerToggableFlags>();
		static readonly Collections.BBitSetParams kFlags2Params = new Collections.BBitSetParams(() => kFlags2ProtoEnum)
		{
			kGetMemberDefaultValue = (id) =>
			{
				switch (id)
				{
				case (int)BPowerToggableFlags.CameraEnableUserScroll:
				case (int)BPowerToggableFlags.CameraEnableUserYaw:
				case (int)BPowerToggableFlags.CameraEnableUserZoom:
				case (int)BPowerToggableFlags.CameraEnableAutoZoomInstant:
				case (int)BPowerToggableFlags.CameraEnableAutoZoom:
				case (int)BPowerToggableFlags.ShowInPowerMenu:
					return true;

				case (int)BPowerToggableFlags.ShowTransportArrows:
				default:
					return false;
				}
			}
		};
		#endregion

		public Collections.BTypeValuesSingle Cost { get; private set; }
		public Collections.BListArray<BPowerDynamicCost> DynamicCosts { get; private set; }
		public Collections.BListArray<BPowerTargetEffectiveness> TargetEffectiveness { get; private set; }
		public Collections.BTypeValuesSingle Populations { get; private set; }
		#region UIRadius
		float mUIRadius;
		public float UIRadius
		{
			get { return mUIRadius; }
			set { mUIRadius = value; }
		}
		#endregion
		#region PowerType
		BPowerType mPowerType = BPowerType.Invalid;
		public BPowerType PowerType
		{
			get { return mPowerType; }
			set { mPowerType = value; }
		}
		#endregion
		#region AutoRecharge
		int mAutoRecharge;
		/// <remarks>In milliseconds</remarks>
		public int AutoRecharge
		{
			get { return mAutoRecharge; }
			set { mAutoRecharge = value; }
		}
		#endregion
		#region UseLimit
		int mUseLimit;
		public int UseLimit
		{
			get { return mUseLimit; }
			set { mUseLimit = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		public Collections.BBitSet Flags2 { get; private set; }
		#region IconTextureName
		string mIconTextureName;
		[Meta.TextureReference]
		public string IconTextureName
		{
			get { return mIconTextureName; }
			set { mIconTextureName = value; }
		}
		#endregion
		public List<int> IconLocations { get; private set; }
		[Meta.BProtoTechReference]
		public List<int> TechPrereqs { get; private set; }
		#region ActionType
		BActionType mActionType = BActionType.Invalid;
		public BActionType ActionType
		{
			get { return mActionType; }
			set { mActionType = value; }
		}
		#endregion
		#region MinigameType
		BMinigameType mMinigameType;
		public BMinigameType MinigameType
		{
			get { return mMinigameType; }
			set { mMinigameType = value; }
		}
		#endregion
		#region CameraZoomMin
		float mCameraZoomMin;
		public float CameraZoomMin
		{
			get { return mCameraZoomMin; }
			set { mCameraZoomMin = value; }
		}
		#endregion
		#region CameraZoomMax
		float mCameraZoomMax;
		public float CameraZoomMax
		{
			get { return mCameraZoomMax; }
			set { mCameraZoomMax = value; }
		}
		#endregion
		#region CameraPitchMin
		float mCameraPitchMin;
		public float CameraPitchMin
		{
			get { return mCameraPitchMin; }
			set { mCameraPitchMin = value; }
		}
		#endregion
		#region CameraPitchMax
		float mCameraPitchMax;
		public float CameraPitchMax
		{
			get { return mCameraPitchMax; }
			set { mCameraPitchMax = value; }
		}
		#endregion
		#region CameraEffectIn
		string mCameraEffectIn;
		[Meta.CameraEffectReference]
		public string CameraEffectIn
		{
			get { return mCameraEffectIn; }
			set { mCameraEffectIn = value; }
		}
		#endregion
		#region CameraEffectOut
		string mCameraEffectOut;
		[Meta.CameraEffectReference]
		public string CameraEffectOut
		{
			get { return mCameraEffectOut; }
			set { mCameraEffectOut = value; }
		}
		#endregion
		#region MinDistanceToSquad
		float mMinDistanceToSquad = PhxUtil.kInvalidSingle;
		public float MinDistanceToSquad
		{
			get { return mMinDistanceToSquad; }
			set { mMinDistanceToSquad = value; }
		}
		#endregion
		#region MaxDistanceToSquad
		float mMaxDistanceToSquad = PhxUtil.kInvalidSingle;
		public float MaxDistanceToSquad
		{
			get { return mMaxDistanceToSquad; }
			set { mMaxDistanceToSquad = value; }
		}
		#endregion
		#region ShowTargetHighlight

		int mShowTargetHighlightObjectType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ShowTargetHighlightObjectType
		{
			get { return mShowTargetHighlightObjectType; }
			set { mShowTargetHighlightObjectType = value; }
		}

		BRelationType mShowTargetHighlightRelation = BRelationType.Any;
		public BRelationType ShowTargetHighlightRelation
		{
			get { return mShowTargetHighlightRelation; }
			set { mShowTargetHighlightRelation = value; }
		}

		bool HasShowTargetHighlightData { get {
			return mShowTargetHighlightObjectType.IsNotNone()
				|| mShowTargetHighlightRelation != BRelationType.Any;
		} }
		#endregion
		public List<int> ChildObjectIDs { get; private set; }
		#region BaseDataLevel
		BProtoPowerDataLevel mBaseDataLevel;
		public BProtoPowerDataLevel BaseDataLevel
		{
			get { return mBaseDataLevel; }
			set { mBaseDataLevel = value; }
		}
		#endregion
		public Collections.BListExplicitIndex<BProtoPowerDataLevel> LevelData { get; private set; }
		#region TriggerScript
		string mTriggerScript;
		[Meta.TriggerScriptReference]
		public string TriggerScript
		{
			get { return mTriggerScript; }
			set { mTriggerScript = value; }
		}
		#endregion
		#region CommandTriggerScript
		string mCommandTriggerScript;
		[Meta.TriggerScriptReference]
		public string CommandTriggerScript
		{
			get { return mCommandTriggerScript; }
			set { mCommandTriggerScript = value; }
		}
		#endregion

		public BProtoPower()
		{
			var textData = base.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasPrereqTextID = true;
			textData.HasChooseTextID = true;

			Cost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			DynamicCosts = new Collections.BListArray<BPowerDynamicCost>();
			TargetEffectiveness = new Collections.BListArray<BPowerTargetEffectiveness>();
			Populations = new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);

			Flags = new Collections.BBitSet(kFlagsParams);
			Flags2 = new Collections.BBitSet(kFlags2Params);

			IconLocations = new List<int>();
			TechPrereqs = new List<int>();

			ChildObjectIDs = new List<int>();
			LevelData = new Collections.BListExplicitIndex<BProtoPowerDataLevel>(BProtoPowerDataLevel.kBListExplicitIndexParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			using (s.EnterCursorBookmark("Attributes"))
			{
				base.Serialize(s);

				using (var bm = s.EnterCursorBookmarkOpt("Cost", Cost, x => x.HasNonZeroItems)) if (bm.IsNotNull)
					XML.XmlUtil.SerializeCostHack(s, Cost);
				XML.XmlUtil.Serialize(s, DynamicCosts, BPowerDynamicCost.kBListXmlParams);
				XML.XmlUtil.Serialize(s, TargetEffectiveness, BPowerTargetEffectiveness.kBListXmlParams);
				XML.XmlUtil.Serialize(s, Populations, BPopulation.kBListXmlParamsSingle_LowerCase);
				s.StreamElementOpt("UIRadius", ref mUIRadius, Predicates.IsNotZero);
				s.StreamElementEnumOpt("PowerType", ref mPowerType, e => e != BPowerType.Invalid);
				s.StreamElementOpt("AutoRecharge", ref mAutoRecharge, Predicates.IsNotZero);
				s.StreamElementOpt("UseLimit", ref mUseLimit, Predicates.IsNotZero);
				XML.XmlUtil.Serialize(s, Flags, XML.BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
				XML.XmlUtil.Serialize(s, Flags2, XML.BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
				s.StreamElementOpt("Icon", ref mIconTextureName, Predicates.IsNotNullOrEmpty);
				s.StreamElements("IconLocation", IconLocations, xs, StreamIconLocation);
				s.StreamElements("TechPrereq", TechPrereqs, xs, XML.BDatabaseXmlSerializerBase.StreamTechID);
				s.StreamElementEnumOpt("Action", ref mActionType, BProtoAction.kNotInvalidActionType);
				s.StreamElementEnumOpt("Minigame", ref mMinigameType, e => e != BMinigameType.None);
				s.StreamElementOpt("CameraZoomMin", ref mCameraZoomMin, Predicates.IsNotZero);
				s.StreamElementOpt("CameraZoomMax", ref mCameraZoomMax, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMin", ref mCameraPitchMin, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMax", ref mCameraPitchMax, Predicates.IsNotZero);
				s.StreamElementOpt("CameraEffectIn", ref mCameraEffectIn, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("CameraEffectOut", ref mCameraEffectOut, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("MinDistanceToSquad", ref mMinDistanceToSquad, PhxPredicates.IsNotInvalid);
				s.StreamElementOpt("MaxDistanceToSquad", ref mMaxDistanceToSquad, PhxPredicates.IsNotInvalid);
				using (var bm = s.EnterCursorBookmarkOpt("ShowTargetHighlight", this, x => x.HasShowTargetHighlightData)) if (bm.IsNotNull)
				{
					xs.StreamDBID(s, "ObjectType", ref mShowTargetHighlightObjectType, DatabaseObjectKind.ObjectType, xmlSource: XML.XmlUtil.kSourceAttr);
					s.StreamAttributeEnumOpt("Relation", ref mShowTargetHighlightRelation, e => e != BRelationType.Any);
				}
				using (var bm = s.EnterCursorBookmarkOpt("ChildObjects", ChildObjectIDs, Predicates.HasItems)) if (bm.IsNotNull)
				{
					s.StreamElements("Object", ChildObjectIDs, xs, XML.BDatabaseXmlSerializerBase.StreamObjectID);
				}
				using (var bm = s.EnterCursorBookmarkOpt("BaseDataLevel", this, x => x.BaseDataLevel != null)) if (bm.IsNotNull)
				{
					if (s.IsReading)
						mBaseDataLevel = new BProtoPowerDataLevel();

					BaseDataLevel.Serialize(s);
				}
				XML.XmlUtil.Serialize(s, LevelData, BProtoPowerDataLevel.kBListExplicitIndexXmlParams);
			}
			s.StreamElementOpt("TriggerScript", ref mTriggerScript, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CommandTriggerScript", ref mCommandTriggerScript, Predicates.IsNotNullOrEmpty);
		}

		static void StreamIconLocation<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs,
			ref int value)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref value);
		}
		#endregion
	};

	public sealed class BPowerDynamicCost
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "DynamicCost",
		};
		#endregion

		#region ObjectType
		int mObjectType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return mObjectType; }
			set { mObjectType = value; }
		}
		#endregion

		#region Multiplier
		float mMultiplier = 1.0f;
		public float Multiplier
		{
			get { return mMultiplier; }
			set { mMultiplier = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "ObjectType", ref mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref mMultiplier);
		}
		#endregion
	};

	public sealed class BPowerTargetEffectiveness
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "TargetEffectiveness",
		};
		#endregion

		#region ObjectType
		int mObjectType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return mObjectType; }
			set { mObjectType = value; }
		}
		#endregion

		#region Effectiveness
		int mEffectiveness;
		public int Effectiveness
		{
			get { return mEffectiveness; }
			set { mEffectiveness = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "ObjectType", ref mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref mEffectiveness);
		}
		#endregion
	};

	public sealed class BProtoPowerDataLevel
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BProtoPowerDataLevel> kBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BProtoPowerDataLevel>()
		{
			kComparer = new ComparerForDataCount(),//PhxUtil.CreateDummyComparerAlwaysNonZero<BProtoPowerDataLevel>(),
			kTypeGetInvalid = () => new BProtoPowerDataLevel()
		};
		public static readonly XML.BListExplicitIndexXmlParams<BProtoPowerDataLevel> kBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BProtoPowerDataLevel>("DataLevel", "level")
		{
			IndexBase = 0
		};

		private sealed class ComparerForDataCount
			: IComparer<BProtoPowerDataLevel>
		{
			public int Compare(BProtoPowerDataLevel x, BProtoPowerDataLevel y)
			{
				if (x == null || y == null)
					return -1;

				if (x.Data.Count == 0 && y.Data.Count == 0)
					return 0;

				return -1;
			}
		};
		#endregion

		public Collections.BListArray<BProtoPowerData> Data { get; private set; } = new Collections.BListArray<BProtoPowerData>();

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			XML.XmlUtil.Serialize(s, Data, BProtoPowerData.kBListXmlParams);
		}
		#endregion
	};

	public sealed class BProtoPowerData
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "Data",
		};
		#endregion

		#region Name
		string mName;
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		#endregion

		#region DataType
		ProtoPowerDataType mDataType = ProtoPowerDataType.Invalid;
		public ProtoPowerDataType DataType
		{
			get { return mDataType; }
			set { mDataType = value; }
		}
		#endregion

		#region Data
		float mDataFloat;
		int mDataInt = TypeExtensions.kNone;
		bool mDataBool;
		string mDataString;

		public float Float
		{
			get { return mDataFloat; }
			set { mDataFloat = value; }
		}

		public int Int
		{
			get { return mDataInt; }
			set { mDataInt = value; }
		}
		[Meta.BProtoObjectReference]
		public int ObjectID
		{
			get { return mDataInt; }
			set { mDataInt = value; }
		}
		[Meta.BProtoSquadReference]
		public int SquadID
		{
			get { return mDataInt; }
			set { mDataInt = value; }
		}
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return mDataInt; }
			set { mDataInt = value; }
		}
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return mDataInt; }
			set { mDataInt = value; }
		}

		public bool Bool
		{
			get { return mDataBool; }
			set { mDataBool = value; }
		}

		[Meta.SoundCueReference]
		public string SoundCue
		{
			get { return mDataString; }
			set { mDataString = value; }
		}
		[Meta.TextureReference]
		public string TextureName
		{
			get { return mDataString; }
			set { mDataString = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref mDataType);
			s.StreamAttribute("name", ref mName);

			switch (DataType)
			{
			case ProtoPowerDataType.Float:
				s.StreamCursor(ref mDataFloat);
				break;

			case ProtoPowerDataType.Int:
				s.StreamCursor(ref mDataInt);
				break;
			case ProtoPowerDataType.ProtoObject:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDataInt, DatabaseObjectKind.Object, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.ProtoSquad:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDataInt, DatabaseObjectKind.Squad, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.Tech:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDataInt, DatabaseObjectKind.Tech, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.ObjectType:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref mDataInt, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;

			case ProtoPowerDataType.Bool:
				s.StreamCursor(ref mDataBool);
				break;

			case ProtoPowerDataType.Sound:
				s.StreamCursor(ref mDataString);
				break;
			case ProtoPowerDataType.Texture:
				s.StreamCursor(ref mDataString);
				break;
			}

			//xs.StreamDBID(s, "ObjectType", ref mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
		}
		#endregion
	};

	public static class BProtoPowerTypesData
	{
		public static Dictionary<string, ProtoPowerDataType> kHelper = new Dictionary<string, ProtoPowerDataType>
		{
			{"HudUpSound", ProtoPowerDataType.Sound},
			{"HudAbortSound", ProtoPowerDataType.Sound},
			{"HudFireSound", ProtoPowerDataType.Sound},

			{"HudLastFireSound", ProtoPowerDataType.Sound},
			{"HudStartEnvSound", ProtoPowerDataType.Sound},
			{"HudStopEnvSound", ProtoPowerDataType.Sound},
		};

		public static Dictionary<string, ProtoPowerDataType> kCarpetBombing = new Dictionary<string,ProtoPowerDataType>
		{
			{"Projectile", ProtoPowerDataType.ProtoObject},
			{"Impact", ProtoPowerDataType.ProtoObject},
			{"Explosion", ProtoPowerDataType.ProtoObject},
			{"Bomber", ProtoPowerDataType.ProtoObject},
			{"InitialDelay", ProtoPowerDataType.Float},
			{"FuseTime", ProtoPowerDataType.Float},
			{"MaxBombs", ProtoPowerDataType.Int},
			{"MaxBombOffset", ProtoPowerDataType.Float},
			{"BombSpacing", ProtoPowerDataType.Float},
			{"LengthMultiplier", ProtoPowerDataType.Float},
			{"WedgeLengthMultiplier", ProtoPowerDataType.Float},
			{"WedgeMinOffset", ProtoPowerDataType.Float},
			{"NudgeMultiplier", ProtoPowerDataType.Float},
			{"BomberFlyinDistance", ProtoPowerDataType.Float},
			{"BomberFlyinHeight", ProtoPowerDataType.Float},
			{"BomberBombHeight", ProtoPowerDataType.Float},
			{"BomberSpeed", ProtoPowerDataType.Float},
			{"RequiresLOS", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kCleansing = new Dictionary<string, ProtoPowerDataType>
		{
			{"Beam", ProtoPowerDataType.ProtoObject},
			{"Projectile", ProtoPowerDataType.ProtoObject},
			{"TickLength", ProtoPowerDataType.Float},
			{"SuppliesPerTick", ProtoPowerDataType.Float},
			{"MinBeamDistance", ProtoPowerDataType.Float},
			{"MaxBeamDistance", ProtoPowerDataType.Float},
			{"CommandInterval", ProtoPowerDataType.Float},
			{"MaxBeamSpeed", ProtoPowerDataType.Float},
			{"RequiresLOS", ProtoPowerDataType.Bool},
		};

		public static Dictionary<string, ProtoPowerDataType> kCryo = new Dictionary<string, ProtoPowerDataType>
		{
			{"CryoObject", ProtoPowerDataType.ProtoObject},
			{"CryoRadius", ProtoPowerDataType.Float},
			{"MinCryoFalloff", ProtoPowerDataType.Float},
			{"FilterType", ProtoPowerDataType.ObjectType},
			{"TickDuration", ProtoPowerDataType.Float},
			{"NumTicks", ProtoPowerDataType.Int},
			{"CryoAmountPerTick", ProtoPowerDataType.Float},
			{"EffectStartTime", ProtoPowerDataType.Float},
			{"MaxKillHp", ProtoPowerDataType.Float},
			{"FreezingThawTime", ProtoPowerDataType.Float},
			{"FrozenThawTime", ProtoPowerDataType.Float},

			{"Bomber", ProtoPowerDataType.ProtoObject},
			{"BomberBombTime", ProtoPowerDataType.Float},
			{"BomberFlyinDistance", ProtoPowerDataType.Float},
			{"BomberFlyinHeight", ProtoPowerDataType.Float},
			{"BomberBombHeight", ProtoPowerDataType.Float},
			{"BomberSpeed", ProtoPowerDataType.Float},
			{"BomberFlyOutTime", ProtoPowerDataType.Float},

			{"AirImpactObject", ProtoPowerDataType.ProtoObject},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kDisruption = new Dictionary<string, ProtoPowerDataType>
		{
			{"DisruptionObject", ProtoPowerDataType.ProtoObject},
			{"PulseObject", ProtoPowerDataType.ProtoObject},
			{"StrikeObject", ProtoPowerDataType.ProtoObject},
			{"DisruptionRadius", ProtoPowerDataType.Float},
			{"DisruptionTimeSec", ProtoPowerDataType.Float},
			{"DisruptionStartTime", ProtoPowerDataType.Float},
			{"PulseSpacing", ProtoPowerDataType.Float},
			{"PulseSound", ProtoPowerDataType.Sound},

			{"Bomber", ProtoPowerDataType.ProtoObject},
			{"BomberBombTime", ProtoPowerDataType.Float},
			{"BomberFlyinDistance", ProtoPowerDataType.Float},
			{"BomberFlyinHeight", ProtoPowerDataType.Float},
			{"BomberBombHeight", ProtoPowerDataType.Float},
			{"BomberSpeed", ProtoPowerDataType.Float},
			{"BomberFlyOutTime", ProtoPowerDataType.Float},

			{"RequiresLOS", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kOdst = new Dictionary<string, ProtoPowerDataType>
		{
			{"Projectile", ProtoPowerDataType.ProtoObject},
			{"SquadSpawnDelay", ProtoPowerDataType.Float},

			{"RequiresLOS", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kOrbital = new Dictionary<string, ProtoPowerDataType>
		{
			{"TargetBeam", ProtoPowerDataType.ProtoObject},
			{"Projectile", ProtoPowerDataType.ProtoObject},
			{"Effect", ProtoPowerDataType.ProtoObject},
			{"RockSmall", ProtoPowerDataType.ProtoObject},
			{"RockMedium", ProtoPowerDataType.ProtoObject},
			{"RockLarge", ProtoPowerDataType.ProtoObject},
			{"NumShots", ProtoPowerDataType.Int},
			{"TargetingDelay", ProtoPowerDataType.Float},
			{"AutoShotDelay", ProtoPowerDataType.Float},
			{"AutoShotInnerRadius", ProtoPowerDataType.Float},
			{"AutoShotOuterRadius", ProtoPowerDataType.Float},
			{"XOffset", ProtoPowerDataType.Float},
			{"YOffset", ProtoPowerDataType.Float},
			{"ZOffset", ProtoPowerDataType.Float},
			{"FiredSound", ProtoPowerDataType.Sound},

			{"CommandInterval", ProtoPowerDataType.Float},
			{"ShotInterval", ProtoPowerDataType.Float},
			{"RequiresLOS", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kRage = new Dictionary<string, ProtoPowerDataType>
		{
			{"TickLength", ProtoPowerDataType.Float},
			{"SuppliesPerTick", ProtoPowerDataType.Float},
			{"SuppliesPerTickAttacking", ProtoPowerDataType.Float},
			{"SuppliesPerJump", ProtoPowerDataType.Float},
			{"DamageMultiplier", ProtoPowerDataType.Float},
			{"DamageTakenMultiplier", ProtoPowerDataType.Float},
			{"SpeedMultiplier", ProtoPowerDataType.Float},
			{"NudgeMultiplier", ProtoPowerDataType.Float},
			{"ScanRadius", ProtoPowerDataType.Float},
			{"TeleportTime", ProtoPowerDataType.Float},
			{"TeleportLateralDistance", ProtoPowerDataType.Float},
			{"TeleportJumpDistance", ProtoPowerDataType.Float},
			{"TimeBetweenRetarget", ProtoPowerDataType.Float},
			{"MotionBlurAmount", ProtoPowerDataType.Float},
			{"MotionBlurDistance", ProtoPowerDataType.Float},
			{"MotionBlurTime", ProtoPowerDataType.Float},
			{"DistanceVsAngleWeight", ProtoPowerDataType.Float},
			{"Projectile", ProtoPowerDataType.ProtoObject},
			{"HandAttachObject", ProtoPowerDataType.ProtoObject},
			{"TeleportAttachObject", ProtoPowerDataType.ProtoObject},
			{"AuraAttachFxSmall", ProtoPowerDataType.ProtoObject},
			{"AuraAttachFxMedium", ProtoPowerDataType.ProtoObject},
			{"AuraAttachFxLarge", ProtoPowerDataType.ProtoObject},
			{"HealAttachFx", ProtoPowerDataType.ProtoObject},
			{"AuraFilterType", ProtoPowerDataType.ObjectType},

			{"HealPerKillCombatValue", ProtoPowerDataType.Float},
			{"AuraRadius", ProtoPowerDataType.Float},
			{"AuraDamageBonus", ProtoPowerDataType.Float},
			{"AttackSound", ProtoPowerDataType.Sound},

			{"CommandInterval", ProtoPowerDataType.Float},
			{"HintTime", ProtoPowerDataType.Float},
			{"MovementProjectionMultiplier", ProtoPowerDataType.Float},
			{"CameraZoom", ProtoPowerDataType.Float},
		};

		public static Dictionary<string, ProtoPowerDataType> kRepair = new Dictionary<string, ProtoPowerDataType>
		{
			{"RepairObject", ProtoPowerDataType.ProtoObject},
			{"RepairAttachment", ProtoPowerDataType.ProtoObject},
			{"NeverStops", ProtoPowerDataType.Bool},
			{"RepairRadius", ProtoPowerDataType.Float},
			{"FilterType", ProtoPowerDataType.ObjectType},
			{"TickDuration", ProtoPowerDataType.Float},
			{"NumTicks", ProtoPowerDataType.Int},
			{"RepairCombatValuePerTick", ProtoPowerDataType.Float},
			{"SpreadAmongSquads", ProtoPowerDataType.Bool},
			{"AllowReinforce", ProtoPowerDataType.Bool},
			{"CooldownTimeIfDamaged", ProtoPowerDataType.Float},
			{"HealAny", ProtoPowerDataType.Bool},

			{"IgnorePlacement", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kTransport = new Dictionary<string, ProtoPowerDataType>
		{
			{"Base", ProtoPowerDataType.Texture},
			{"Mover", ProtoPowerDataType.Texture},

			{"MaxGroundVehicles", ProtoPowerDataType.Int},
			{"MaxInfantryUnits", ProtoPowerDataType.Int},

			{"MinTransportDistance", ProtoPowerDataType.Float},

			{"RequiresLOS", ProtoPowerDataType.Bool},

			//kHelper
		};

		public static Dictionary<string, ProtoPowerDataType> kWave = new Dictionary<string, ProtoPowerDataType>
		{
			{"TickLength", ProtoPowerDataType.Float},
			{"SuppliesPerTick", ProtoPowerDataType.Float},
			{"MaxBallSpeedStagnant", ProtoPowerDataType.Float},
			{"MaxBallSpeedPulling", ProtoPowerDataType.Float},
			{"ExplodeTime", ProtoPowerDataType.Float},
			{"PullingRange", ProtoPowerDataType.Float},
			{"ExplosionForceOnDebris", ProtoPowerDataType.Float},
			{"HealthToCapture", ProtoPowerDataType.Float},
			{"NudgeStrength", ProtoPowerDataType.Float},
			{"InitialLateralPullStrength", ProtoPowerDataType.Float},
			{"CapturedRadialSpacing", ProtoPowerDataType.Float},
			{"CapturedSpringStrength", ProtoPowerDataType.Float},
			{"CapturedSpringDampening", ProtoPowerDataType.Float},
			{"CapturedSpringRestLength", ProtoPowerDataType.Float},
			{"CapturedMinLateralSpeed", ProtoPowerDataType.Float},
			{"RipAttachmentChancePulling", ProtoPowerDataType.Float},
			{"DebrisAngularDamping", ProtoPowerDataType.Float},
			{"MaxExplosionDamageBankPerCaptured", ProtoPowerDataType.Float},
			{"CommandInterval", ProtoPowerDataType.Float},
			{"MinBallDistance", ProtoPowerDataType.Float},
			{"MaxBallDistance", ProtoPowerDataType.Float},
			{"PickupObjectRate", ProtoPowerDataType.Float},
			{"MaxCapturedObjects", ProtoPowerDataType.Int},
			{"LightningPerTick", ProtoPowerDataType.Int},
			{"NudgeChancePulling", ProtoPowerDataType.Int},
			{"ThrowPartChancePulling", ProtoPowerDataType.Int},
			{"LightningChancePulling", ProtoPowerDataType.Int},
			{"BallObject", ProtoPowerDataType.ProtoObject},
			{"LightningProjectile", ProtoPowerDataType.ProtoObject},
			{"ExplodeProjectile", ProtoPowerDataType.ProtoObject},
			{"DebrisProjectile", ProtoPowerDataType.ProtoObject},
			{"PickupAttachment", ProtoPowerDataType.ProtoObject},
			{"ThrowUnitsOnExplosion", ProtoPowerDataType.Bool},
			{"ExplodeSound", ProtoPowerDataType.Sound},
			{"MinDamageBankPercentToThrow", ProtoPowerDataType.Float},
			{"LightningBeamVisual", ProtoPowerDataType.ProtoObject},

			{"MinBallHeight", ProtoPowerDataType.Float},
			{"MaxBallHeight", ProtoPowerDataType.Float},
			{"CameraDistance", ProtoPowerDataType.Float},
			{"CameraHeight", ProtoPowerDataType.Float},
			{"CameraHoverPointDistance", ProtoPowerDataType.Float},
			{"CameraMaxBallAngle", ProtoPowerDataType.Float},
			{"PickupShakeDuration", ProtoPowerDataType.Float},
			{"PickupRumbleShakeStrength", ProtoPowerDataType.Float},
			{"PickupCameraShakeStrength", ProtoPowerDataType.Float},

			{"HintTime", ProtoPowerDataType.Float},
		};
	};
}
