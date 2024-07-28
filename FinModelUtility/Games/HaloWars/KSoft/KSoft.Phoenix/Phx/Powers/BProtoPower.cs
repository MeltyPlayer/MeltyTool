using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoPower
		: DatabaseNamedObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams("Power")
		{
			DataName = kXmlAttrName,
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
			get { return this.mUIRadius; }
			set { this.mUIRadius = value; }
		}
		#endregion
		#region PowerType
		BPowerType mPowerType = BPowerType.Invalid;
		public BPowerType PowerType
		{
			get { return this.mPowerType; }
			set { this.mPowerType = value; }
		}
		#endregion
		#region AutoRecharge
		int mAutoRecharge;
		/// <remarks>In milliseconds</remarks>
		public int AutoRecharge
		{
			get { return this.mAutoRecharge; }
			set { this.mAutoRecharge = value; }
		}
		#endregion
		#region UseLimit
		int mUseLimit;
		public int UseLimit
		{
			get { return this.mUseLimit; }
			set { this.mUseLimit = value; }
		}
		#endregion
		public Collections.BBitSet Flags { get; private set; }
		public Collections.BBitSet Flags2 { get; private set; }
		#region IconTextureName
		string mIconTextureName;
		[Meta.TextureReference]
		public string IconTextureName
		{
			get { return this.mIconTextureName; }
			set { this.mIconTextureName = value; }
		}
		#endregion
		public List<int> IconLocations { get; private set; }
		[Meta.BProtoTechReference]
		public List<int> TechPrereqs { get; private set; }
		#region ActionType
		BActionType mActionType = BActionType.Invalid;
		public BActionType ActionType
		{
			get { return this.mActionType; }
			set { this.mActionType = value; }
		}
		#endregion
		#region MinigameType
		BMinigameType mMinigameType;
		public BMinigameType MinigameType
		{
			get { return this.mMinigameType; }
			set { this.mMinigameType = value; }
		}
		#endregion
		#region CameraZoomMin
		float mCameraZoomMin;
		public float CameraZoomMin
		{
			get { return this.mCameraZoomMin; }
			set { this.mCameraZoomMin = value; }
		}
		#endregion
		#region CameraZoomMax
		float mCameraZoomMax;
		public float CameraZoomMax
		{
			get { return this.mCameraZoomMax; }
			set { this.mCameraZoomMax = value; }
		}
		#endregion
		#region CameraPitchMin
		float mCameraPitchMin;
		public float CameraPitchMin
		{
			get { return this.mCameraPitchMin; }
			set { this.mCameraPitchMin = value; }
		}
		#endregion
		#region CameraPitchMax
		float mCameraPitchMax;
		public float CameraPitchMax
		{
			get { return this.mCameraPitchMax; }
			set { this.mCameraPitchMax = value; }
		}
		#endregion
		#region CameraEffectIn
		string mCameraEffectIn;
		[Meta.CameraEffectReference]
		public string CameraEffectIn
		{
			get { return this.mCameraEffectIn; }
			set { this.mCameraEffectIn = value; }
		}
		#endregion
		#region CameraEffectOut
		string mCameraEffectOut;
		[Meta.CameraEffectReference]
		public string CameraEffectOut
		{
			get { return this.mCameraEffectOut; }
			set { this.mCameraEffectOut = value; }
		}
		#endregion
		#region MinDistanceToSquad
		float mMinDistanceToSquad = PhxUtil.kInvalidSingle;
		public float MinDistanceToSquad
		{
			get { return this.mMinDistanceToSquad; }
			set { this.mMinDistanceToSquad = value; }
		}
		#endregion
		#region MaxDistanceToSquad
		float mMaxDistanceToSquad = PhxUtil.kInvalidSingle;
		public float MaxDistanceToSquad
		{
			get { return this.mMaxDistanceToSquad; }
			set { this.mMaxDistanceToSquad = value; }
		}
		#endregion
		#region ShowTargetHighlight

		int mShowTargetHighlightObjectType = TypeExtensions.kNone;
		[Meta.ObjectTypeReference]
		public int ShowTargetHighlightObjectType
		{
			get { return this.mShowTargetHighlightObjectType; }
			set { this.mShowTargetHighlightObjectType = value; }
		}

		BRelationType mShowTargetHighlightRelation = BRelationType.Any;
		public BRelationType ShowTargetHighlightRelation
		{
			get { return this.mShowTargetHighlightRelation; }
			set { this.mShowTargetHighlightRelation = value; }
		}

		bool HasShowTargetHighlightData { get {
			return this.mShowTargetHighlightObjectType.IsNotNone()
				||
				this.mShowTargetHighlightRelation != BRelationType.Any;
		} }
		#endregion
		public List<int> ChildObjectIDs { get; private set; }
		#region BaseDataLevel
		BProtoPowerDataLevel mBaseDataLevel;
		public BProtoPowerDataLevel BaseDataLevel
		{
			get { return this.mBaseDataLevel; }
			set { this.mBaseDataLevel = value; }
		}
		#endregion
		public Collections.BListExplicitIndex<BProtoPowerDataLevel> LevelData { get; private set; }
		#region TriggerScript
		string mTriggerScript;
		[Meta.TriggerScriptReference]
		public string TriggerScript
		{
			get { return this.mTriggerScript; }
			set { this.mTriggerScript = value; }
		}
		#endregion
		#region CommandTriggerScript
		string mCommandTriggerScript;
		[Meta.TriggerScriptReference]
		public string CommandTriggerScript
		{
			get { return this.mCommandTriggerScript; }
			set { this.mCommandTriggerScript = value; }
		}
		#endregion

		public BProtoPower()
		{
			var textData = this.CreateDatabaseObjectUserInterfaceTextData();
			textData.HasDisplayNameID = true;
			textData.HasRolloverTextID = true;
			textData.HasPrereqTextID = true;
			textData.HasChooseTextID = true;

			this.Cost = new Collections.BTypeValuesSingle(BResource.kBListTypeValuesParams);
			this.DynamicCosts = new Collections.BListArray<BPowerDynamicCost>();
			this.TargetEffectiveness = new Collections.BListArray<BPowerTargetEffectiveness>();
			this.Populations = new Collections.BTypeValuesSingle(BPopulation.kBListParamsSingle);

			this.Flags = new Collections.BBitSet(kFlagsParams);
			this.Flags2 = new Collections.BBitSet(kFlags2Params);

			this.IconLocations = new List<int>();
			this.TechPrereqs = new List<int>();

			this.ChildObjectIDs = new List<int>();
			this.LevelData = new Collections.BListExplicitIndex<BProtoPowerDataLevel>(BProtoPowerDataLevel.kBListExplicitIndexParams);
		}

		#region ITagElementStreamable<string> Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			var xs = s.GetSerializerInterface();

			using (s.EnterCursorBookmark("Attributes"))
			{
				base.Serialize(s);

				using (var bm = s.EnterCursorBookmarkOpt("Cost", this.Cost, x => x.HasNonZeroItems)) if (bm.IsNotNull)
					XML.XmlUtil.SerializeCostHack(s, this.Cost);
				XML.XmlUtil.Serialize(s, this.DynamicCosts, BPowerDynamicCost.kBListXmlParams);
				XML.XmlUtil.Serialize(s, this.TargetEffectiveness, BPowerTargetEffectiveness.kBListXmlParams);
				XML.XmlUtil.Serialize(s, this.Populations, BPopulation.kBListXmlParamsSingle_LowerCase);
				s.StreamElementOpt("UIRadius", ref this.mUIRadius, Predicates.IsNotZero);
				s.StreamElementEnumOpt("PowerType", ref this.mPowerType, e => e != BPowerType.Invalid);
				s.StreamElementOpt("AutoRecharge", ref this.mAutoRecharge, Predicates.IsNotZero);
				s.StreamElementOpt("UseLimit", ref this.mUseLimit, Predicates.IsNotZero);
				XML.XmlUtil.Serialize(s, this.Flags, XML.BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
				XML.XmlUtil.Serialize(s, this.Flags2, XML.BBitSetXmlParams.kFlagsAreElementNamesThatMeanTrue);
				s.StreamElementOpt("Icon", ref this.mIconTextureName, Predicates.IsNotNullOrEmpty);
				s.StreamElements("IconLocation", this.IconLocations, xs, StreamIconLocation);
				s.StreamElements("TechPrereq", this.TechPrereqs, xs, XML.BXmlSerializerInterface.StreamTechID);
				s.StreamElementEnumOpt("Action", ref this.mActionType, BProtoAction.kNotInvalidActionType);
				s.StreamElementEnumOpt("Minigame", ref this.mMinigameType, e => e != BMinigameType.None);
				s.StreamElementOpt("CameraZoomMin", ref this.mCameraZoomMin, Predicates.IsNotZero);
				s.StreamElementOpt("CameraZoomMax", ref this.mCameraZoomMax, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMin", ref this.mCameraPitchMin, Predicates.IsNotZero);
				s.StreamElementOpt("CameraPitchMax", ref this.mCameraPitchMax, Predicates.IsNotZero);
				s.StreamElementOpt("CameraEffectIn", ref this.mCameraEffectIn, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("CameraEffectOut", ref this.mCameraEffectOut, Predicates.IsNotNullOrEmpty);
				s.StreamElementOpt("MinDistanceToSquad", ref this.mMinDistanceToSquad, PhxPredicates.IsNotInvalid);
				s.StreamElementOpt("MaxDistanceToSquad", ref this.mMaxDistanceToSquad, PhxPredicates.IsNotInvalid);
				using (var bm = s.EnterCursorBookmarkOpt("ShowTargetHighlight", this, x => x.HasShowTargetHighlightData)) if (bm.IsNotNull)
				{
					xs.StreamDBID(s, "ObjectType", ref this.mShowTargetHighlightObjectType, DatabaseObjectKind.ObjectType, xmlSource: XML.XmlUtil.kSourceAttr);
					s.StreamAttributeEnumOpt("Relation", ref this.mShowTargetHighlightRelation, e => e != BRelationType.Any);
				}
				using (var bm = s.EnterCursorBookmarkOpt("ChildObjects", this.ChildObjectIDs, Predicates.HasItems)) if (bm.IsNotNull)
				{
					s.StreamElements("Object", this.ChildObjectIDs, xs, XML.BXmlSerializerInterface.StreamObjectID);
				}
				using (var bm = s.EnterCursorBookmarkOpt("BaseDataLevel", this, x => x.BaseDataLevel != null)) if (bm.IsNotNull)
				{
					if (s.IsReading)
						this.mBaseDataLevel = new BProtoPowerDataLevel();

					this.BaseDataLevel.Serialize(s);
				}
				XML.XmlUtil.Serialize(s, this.LevelData, BProtoPowerDataLevel.kBListExplicitIndexXmlParams);
			}
			s.StreamElementOpt("TriggerScript", ref this.mTriggerScript, Predicates.IsNotNullOrEmpty);
			s.StreamElementOpt("CommandTriggerScript", ref this.mCommandTriggerScript, Predicates.IsNotNullOrEmpty);
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
			get { return this.mObjectType; }
			set { this.mObjectType = value; }
		}
		#endregion

		#region Multiplier
		float mMultiplier = 1.0f;
		public float Multiplier
		{
			get { return this.mMultiplier; }
			set { this.mMultiplier = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "ObjectType", ref this.mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref this.mMultiplier);
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
			get { return this.mObjectType; }
			set { this.mObjectType = value; }
		}
		#endregion

		#region Effectiveness
		int mEffectiveness;
		public int Effectiveness
		{
			get { return this.mEffectiveness; }
			set { this.mEffectiveness = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "ObjectType", ref this.mObjectType, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceAttr);
			s.StreamCursor(ref this.mEffectiveness);
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

			XML.XmlUtil.Serialize(s, this.Data, BProtoPowerData.kBListXmlParams);
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
			get { return this.mName; }
			set { this.mName = value; }
		}
		#endregion

		#region DataType
		ProtoPowerDataType mDataType = ProtoPowerDataType.Invalid;
		public ProtoPowerDataType DataType
		{
			get { return this.mDataType; }
			set { this.mDataType = value; }
		}
		#endregion

		#region Data
		float mDataFloat;
		int mDataInt = TypeExtensions.kNone;
		bool mDataBool;
		string mDataString;

		public float Float
		{
			get { return this.mDataFloat; }
			set { this.mDataFloat = value; }
		}

		public int Int
		{
			get { return this.mDataInt; }
			set { this.mDataInt = value; }
		}
		[Meta.BProtoObjectReference]
		public int ObjectID
		{
			get { return this.mDataInt; }
			set { this.mDataInt = value; }
		}
		[Meta.BProtoSquadReference]
		public int SquadID
		{
			get { return this.mDataInt; }
			set { this.mDataInt = value; }
		}
		[Meta.BProtoTechReference]
		public int TechID
		{
			get { return this.mDataInt; }
			set { this.mDataInt = value; }
		}
		[Meta.ObjectTypeReference]
		public int ObjectType
		{
			get { return this.mDataInt; }
			set { this.mDataInt = value; }
		}

		public bool Bool
		{
			get { return this.mDataBool; }
			set { this.mDataBool = value; }
		}

		[Meta.SoundCueReference]
		public string SoundCue
		{
			get { return this.mDataString; }
			set { this.mDataString = value; }
		}
		[Meta.TextureReference]
		public string TextureName
		{
			get { return this.mDataString; }
			set { this.mDataString = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("type", ref this.mDataType);
			s.StreamAttribute("name", ref this.mName);

			switch (this.DataType)
			{
			case ProtoPowerDataType.Float:
				s.StreamCursor(ref this.mDataFloat);
				break;

			case ProtoPowerDataType.Int:
				s.StreamCursor(ref this.mDataInt);
				break;
			case ProtoPowerDataType.ProtoObject:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDataInt, DatabaseObjectKind.Object, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.ProtoSquad:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDataInt, DatabaseObjectKind.Squad, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.Tech:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDataInt, DatabaseObjectKind.Tech, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;
			case ProtoPowerDataType.ObjectType:
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDataInt, DatabaseObjectKind.ObjectType, isOptional: false, xmlSource: XML.XmlUtil.kSourceCursor);
				break;

			case ProtoPowerDataType.Bool:
				s.StreamCursor(ref this.mDataBool);
				break;

			case ProtoPowerDataType.Sound:
				s.StreamCursor(ref this.mDataString);
				break;
			case ProtoPowerDataType.Texture:
				s.StreamCursor(ref this.mDataString);
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
