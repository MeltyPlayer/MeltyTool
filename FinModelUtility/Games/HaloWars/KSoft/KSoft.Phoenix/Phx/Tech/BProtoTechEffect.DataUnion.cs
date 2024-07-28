#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Phx
{
	partial class BProtoTechEffect
	{
		[Interop.StructLayout(Interop.LayoutKind.Explicit, Size = kSizeOf)]
		struct DataUnion
		{
			internal const int kSizeOf = 12 // type, 1st and 2nd param
				+ 4 // pad
				+ sizeof(ulong) // object reference
				;
			/// <summary>Offset of the first parameter</summary>
			const int kFirstParam = 4;
			/// <summary>Offset of the second parameter</summary>
			const int kSecondParam = 8;
			const int kStringParam = 16;

			[Interop.FieldOffset(0)] public BObjectDataType SubType;
			[Interop.FieldOffset(kFirstParam)] public int ID;
			[Interop.FieldOffset(kSecondParam)] public int ID2;
			[Interop.FieldOffset(kStringParam)] public string StringValue;

			[Interop.FieldOffset(kFirstParam)] public int Cost_Type;
			[Interop.FieldOffset(kSecondParam)] public int Cost_UnitType; // proto object or type ID

			[Interop.FieldOffset(kFirstParam)] public BProtoObjectCommandType CommandType;
			[Interop.FieldOffset(kSecondParam)] public int CommandData;
			[Interop.FieldOffset(kSecondParam)] public BSquadMode CommandDataSM;

			[Interop.FieldOffset(kFirstParam)] public int DmgMod_WeapType;
			[Interop.FieldOffset(kSecondParam)] public int DmgMod_DmgType;

			[Interop.FieldOffset(kSecondParam)] public int TrainLimitType; // proto object or squad ID

			[Interop.FieldOffset(kFirstParam)] public int FromTypeID;
			[Interop.FieldOffset(kSecondParam)] public int ToTypeID;

			[Interop.FieldOffset(kFirstParam)] public BProtoTechEffectSetAgeLevel SetAgeLevel;

			[Interop.FieldOffset(kStringParam)] public string TurretRate_HardpointName;

			[Interop.FieldOffset(kFirstParam)] public BObjectDataIconType Icon_Type;
			[Interop.FieldOffset(kStringParam)] public string Icon_Name;

			[Interop.FieldOffset(kStringParam)] public string HPBar_Name;

			public void Initialize()
			{
				this.SubType = BObjectDataType.Invalid;
				this.ID = this.ID2 = TypeExtensions.kNone;
				this.StringValue = null;
			}

			public void StreamCost<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs, bool isResourceOptional = false)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamTypeName(s, "Resource", ref this.Cost_Type, GameDataObjectKind.Cost, isResourceOptional, XML.XmlUtil.kSourceAttr);
				bool streamedUnitType = xs.StreamDBID(s, "UnitType", ref this.Cost_UnitType, DatabaseObjectKind.Object, true, XML.XmlUtil.kSourceAttr);
				// #HACK deal with hand edited data in Halo Wars
				if (!streamedUnitType && s.IsReading)
					xs.StreamDBID(s, "unitType", ref this.Cost_UnitType, DatabaseObjectKind.Object, true, XML.XmlUtil.kSourceAttr);
			}
			void StreamCommandData<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				const string attr_name = "CommandData";

				switch (this.CommandType)
				{
				case BProtoObjectCommandType.Research: // proto tech
					xs.StreamDBID(s, attr_name, ref this.CommandData, DatabaseObjectKind.Tech, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.TrainUnit: // proto object
				case BProtoObjectCommandType.Build:
				case BProtoObjectCommandType.BuildOther:
					xs.StreamDBID(s, attr_name, ref this.CommandData, DatabaseObjectKind.Object, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.TrainSquad: // proto squad
					xs.StreamDBID(s, attr_name, ref this.CommandData, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceAttr);
					break;

				case BProtoObjectCommandType.ChangeMode: // unused
					s.StreamAttributeEnum(attr_name, ref this.CommandDataSM);
					break;

				case BProtoObjectCommandType.Ability:
					xs.StreamDBID(s, attr_name, ref this.CommandData, DatabaseObjectKind.Ability, false, XML.XmlUtil.kSourceAttr);
					break;
				case BProtoObjectCommandType.Power:
					xs.StreamDBID(s, attr_name, ref this.CommandData, DatabaseObjectKind.Power, false, XML.XmlUtil.kSourceAttr);
					break;
				}
			}
			public void StreamCommand<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses this as "CommandType", but its parser ignores case
				if (s.StreamAttributeEnumOpt("commandType", ref this.CommandType, e => e != BProtoObjectCommandType.Invalid))
					this.StreamCommandData(s, xs);
			}
			public void StreamDamageModifier<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				xs.StreamDBID(s, "WeaponType", ref this.DmgMod_WeapType, DatabaseObjectKind.WeaponType, false, XML.XmlUtil.kSourceAttr);
				xs.StreamDBID(s, "DamageType", ref this.DmgMod_DmgType, DatabaseObjectKind.DamageType, false, XML.XmlUtil.kSourceAttr);
			}
			public void StreamTrainLimit<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs, DatabaseObjectKind kind)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses these as "UnitType" and "SquadType", but its parser ignores case

				if (kind == DatabaseObjectKind.Object)
					xs.StreamDBID(s, "unitType", ref this.TrainLimitType, kind, false, XML.XmlUtil.kSourceAttr);
				else if (kind == DatabaseObjectKind.Squad)
					xs.StreamDBID(s, "squadType", ref this.TrainLimitType, kind, false, XML.XmlUtil.kSourceAttr);
			}
			public void StreamIcon<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, XML.BXmlSerializerInterface xs)
				where TDoc : class
				where TCursor : class
			{
				// #NOTE engine parses this as "IconType", but its parser ignores case
				s.StreamAttributeEnum("iconType", ref this.Icon_Type);
				// #NOTE engine parses this as "IconType", but its parser ignores case
				s.StreamString("iconName", ref this.Icon_Name, false);
			}
		};

		#region ID variants
		[Meta.BWeaponTypeReference]
		public int WeaponTypeID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.DamageModifier);
			return this.mDU.DmgMod_WeapType;
		} }
		[Meta.BDamageTypeReference]
		public int DamageTypeID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.DamageModifier);
			return this.mDU.DmgMod_DmgType;
		} }

		[Meta.RateReference]
		public int RateID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.RateAmount || this.SubType == BObjectDataType.RateMultiplier);
			return this.mDU.ID;
		} }

		[Meta.PopulationReference]
		public int PopID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.PopCap || this.SubType == BObjectDataType.PopMax);
			return this.mDU.ID;
		} }

		[Meta.BProtoPowerReference]
		public int PowerID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Data);
			Contract.Requires(this.SubType == BObjectDataType.PowerRechargeTime || this.SubType == BObjectDataType.PowerUseLimit || this.SubType == BObjectDataType.PowerLevel);
			return this.mDU.ID;
		} }

		public int TransformUnitID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TransformUnit);
			return this.mDU.ToTypeID;
		} }
		public int TransformProtoFromID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TransformProtoUnit || this.Type == BProtoTechEffectType.TransformProtoSquad);
			return this.mDU.FromTypeID;
		} }
		public int TransformProtoToID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TransformProtoUnit || this.Type == BProtoTechEffectType.TransformProtoSquad);
			return this.mDU.ToTypeID;
		} }
		[Meta.BProtoObjectReference]
		public int BuildObjectID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Build);
			return this.mDU.ToTypeID;
		} }
		[Meta.BProtoPowerReference]
		public int GodPowerID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.GodPower);
			return this.mDU.ID;
		} }
		[Meta.BProtoTechReference]
		public int TechStatusTechID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.TechStatus);
			return this.mDU.ID;
		} }
		[Meta.BAbilityReference]
		public int AbilityID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.Ability);
			return this.mDU.ID;
		} }
		public int AttachSquadTypeObjectID { get {
			Contract.Requires(this.Type == BProtoTechEffectType.AttachSquad);
			return this.mDU.ID;
		} }
		#endregion
	};
}