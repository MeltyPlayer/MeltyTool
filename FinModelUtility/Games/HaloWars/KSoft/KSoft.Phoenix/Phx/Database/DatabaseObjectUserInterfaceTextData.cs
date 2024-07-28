using System;
using KSoft.Collections;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	// #TODO BGameMode also needs this

	/// <summary>
	/// UX string references common to many objects. Up to implementing objects to declare
	/// which strings they use.
	/// </summary>
	public sealed class DatabaseObjectUserInterfaceTextData
		: IO.ITagElementStringNameStreamable
	{
		#region Flags
		private enum Flags
		{
			HasNameID,
			HasDisplayNameID,
			HasDisplayName2ID, // BAbility only
			HasDescriptionID,
			HasLongDescriptionID,
			HasPrereqTextID,
			HasStatsNameID,
			HasRoleTextID,
			HasRolloverTextID,

			// ProtoObject specific:
			HasEnemyRolloverTextID,
			HasGaiaRolloverTextID,

			// ProtoPower specific:
			HasChooseTextID,

			kNumberOf
		};
		private BitVector32 mFlags;

		public bool HasNameID
		{
			get { return this.mFlags.Test(Flags.HasNameID); }
			set { this.mFlags.Set(Flags.HasNameID, value); }
		}

		public bool HasDisplayNameID
		{
			get { return this.mFlags.Test(Flags.HasDisplayNameID); }
			set { this.mFlags.Set(Flags.HasDisplayNameID, value); }
		}

		public bool HasDisplayName2ID
		{
			get { return this.mFlags.Test(Flags.HasDisplayName2ID); }
			set { this.mFlags.Set(Flags.HasDisplayName2ID, value); }
		}

		public bool HasDescriptionID
		{
			get { return this.mFlags.Test(Flags.HasDescriptionID); }
			set { this.mFlags.Set(Flags.HasDescriptionID, value); }
		}

		public bool HasLongDescriptionID
		{
			get { return this.mFlags.Test(Flags.HasLongDescriptionID); }
			set { this.mFlags.Set(Flags.HasLongDescriptionID, value); }
		}

		public bool HasPrereqTextID
		{
			get { return this.mFlags.Test(Flags.HasPrereqTextID); }
			set { this.mFlags.Set(Flags.HasPrereqTextID, value); }
		}

		public bool HasStatsNameID
		{
			get { return this.mFlags.Test(Flags.HasStatsNameID); }
			set { this.mFlags.Set(Flags.HasStatsNameID, value); }
		}

		public bool HasRoleTextID
		{
			get { return this.mFlags.Test(Flags.HasRoleTextID); }
			set { this.mFlags.Set(Flags.HasRoleTextID, value); }
		}

		public bool HasRolloverTextID
		{
			get { return this.mFlags.Test(Flags.HasRolloverTextID); }
			set { this.mFlags.Set(Flags.HasRolloverTextID, value); }
		}

		public bool HasEnemyRolloverTextID
		{
			get { return this.mFlags.Test(Flags.HasEnemyRolloverTextID); }
			set { this.mFlags.Set(Flags.HasEnemyRolloverTextID, value); }
		}

		public bool HasGaiaRolloverTextID
		{
			get { return this.mFlags.Test(Flags.HasGaiaRolloverTextID); }
			set {
				this.mFlags.Set(Flags.HasGaiaRolloverTextID, value);

				if (value)
					this.GaiaRolloverText = new BListArray<GaiaRolloverTextData>();
				else
					this.GaiaRolloverText = null;
			}
		}

		public bool HasChooseTextID
		{
			get { return this.mFlags.Test(Flags.HasChooseTextID); }
			set { this.mFlags.Set(Flags.HasChooseTextID, value); }
		}
		#endregion

		#region NameID
		int mNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int NameID
		{
			get {
				if (!this.HasNameID)
					return TypeExtensions.kNone;

				return this.mNameID;
			}
			set {
				Contract.Requires(this.HasNameID);
				this.mNameID = value;
			}
		}
		#endregion

		#region DisplayNameID
		int mDisplayNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int DisplayNameID
		{
			get {
				if (!this.HasDisplayNameID)
					return TypeExtensions.kNone;

				return this.mDisplayNameID;
			}
			set {
				Contract.Requires(this.HasDisplayNameID);
				this.mDisplayNameID = value;
			}
		}
		#endregion

		#region DisplayName2ID
		int mDisplayName2ID = TypeExtensions.kNone;
		[Meta.UnusedData("unused in code")]
		[Meta.LocStringReference]
		public int DisplayName2ID
		{
			get {
				if (!this.HasDisplayName2ID)
					return TypeExtensions.kNone;

				return this.mDisplayName2ID;
			}
			set {
				Contract.Requires(this.HasDisplayName2ID);
				this.mDisplayName2ID = value;
			}
		}
		#endregion

		#region DescriptionID
		int mDescriptionID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int DescriptionID
		{
			get {
				if (!this.HasDescriptionID)
					return TypeExtensions.kNone;

				return this.mDescriptionID;
			}
			set {
				Contract.Requires(this.HasDescriptionID);
				this.mDescriptionID = value;
			}
		}
		#endregion

		#region LongDescriptionID
		int mLongDescriptionID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int LongDescriptionID
		{
			get {
				if (!this.HasLongDescriptionID)
					return TypeExtensions.kNone;

				return this.mLongDescriptionID;
			}
			set {
				Contract.Requires(this.HasLongDescriptionID);
				this.mLongDescriptionID = value;
			}
		}
		#endregion

		#region PrereqTextID
		int mPrereqTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int PrereqTextID
		{
			get {
				if (!this.HasPrereqTextID)
					return TypeExtensions.kNone;

				return this.mPrereqTextID;
			}
			set {
				Contract.Requires(this.HasPrereqTextID);
				this.mPrereqTextID = value;
			}
		}
		#endregion

		#region StatsNameID
		int mStatsNameID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int StatsNameID
		{
			get {
				if (!this.HasStatsNameID)
					return TypeExtensions.kNone;

				return this.mStatsNameID;
			}
			set {
				Contract.Requires(this.HasStatsNameID);
				this.mStatsNameID = value;
			}
		}
		#endregion

		#region RoleTextID
		int mRoleTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int RoleTextID
		{
			get {
				if (!this.HasRoleTextID)
					return TypeExtensions.kNone;

				return this.mRoleTextID;
			}
			set {
				Contract.Requires(this.HasRoleTextID);
				this.mRoleTextID = value;
			}
		}
		#endregion

		#region RolloverTextID
		int mRolloverTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int RolloverTextID
		{
			get {
				if (!this.HasRolloverTextID)
					return TypeExtensions.kNone;

				return this.mRolloverTextID;
			}
			set {
				Contract.Requires(this.HasRolloverTextID);
				this.mRolloverTextID = value;
			}
		}
		#endregion

		#region EnemyRolloverTextID
		int mEnemyRolloverTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int EnemyRolloverTextID
		{
			get {
				if (!this.HasEnemyRolloverTextID)
					return TypeExtensions.kNone;

				return this.mEnemyRolloverTextID;
			}
			set {
				Contract.Requires(this.HasEnemyRolloverTextID);
				this.mEnemyRolloverTextID = value;
			}
		}
		#endregion

		public BListArray<GaiaRolloverTextData> GaiaRolloverText { get; private set; }

		#region ChooseTextID
		int mChooseTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int ChooseTextID
		{
			get {
				if (!this.HasChooseTextID)
					return TypeExtensions.kNone;

				return this.mChooseTextID;
			}
			set {
				Contract.Requires(this.HasChooseTextID);
				this.mChooseTextID = value;
			}
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			if (this.HasNameID)
				xs.StreamStringID(s, "NameID", ref this.mNameID);
			if (this.HasDisplayNameID)
				xs.StreamStringID(s, "DisplayNameID", ref this.mDisplayNameID);
			if (this.HasDisplayName2ID)
				xs.StreamStringID(s, "DisplayName2ID", ref this.mDisplayName2ID);
			if (this.HasDescriptionID)
				xs.StreamStringID(s, "DescriptionID", ref this.mDescriptionID);
			if (this.HasLongDescriptionID)
				xs.StreamStringID(s, "LongDescriptionID", ref this.mLongDescriptionID);
			if (this.HasPrereqTextID)
				xs.StreamStringID(s, "PrereqTextID", ref this.mPrereqTextID);
			if (this.HasRoleTextID)
				xs.StreamStringID(s, "RoleTextID", ref this.mRoleTextID);
			if (this.HasRolloverTextID)
				xs.StreamStringID(s, "RolloverTextID", ref this.mRolloverTextID);
			if (this.HasEnemyRolloverTextID)
				xs.StreamStringID(s, "EnemyRolloverTextID", ref this.mEnemyRolloverTextID);
			if (this.HasGaiaRolloverTextID)
				XML.XmlUtil.Serialize(s, this.GaiaRolloverText, GaiaRolloverTextData.kBListXmlParams);
			if (this.HasStatsNameID)
				xs.StreamStringID(s, "StatsNameID", ref this.mStatsNameID);
			if (this.HasChooseTextID)
				xs.StreamStringID(s, "ChooseTextID", ref this.mChooseTextID);
		}
		#endregion
	};

	public sealed class GaiaRolloverTextData
		: IO.ITagElementStringNameStreamable
		, IComparable<GaiaRolloverTextData>
		, IEquatable<GaiaRolloverTextData>
	{
		/// <summary>HW1 is hard coded to only support 4 (for the first four Civs)</summary>
		public const int cMaxGaiaRolloverTextIndices = 4;

		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "GaiaRolloverTextID",
		};
		#endregion

		#region CivID
		int mCivID = TypeExtensions.kNone;
		/// <summary>No CivID means it applies to all Civs</summary>
		[Meta.BCivReference]
		public int CivID
		{
			get { return this.mCivID; }
			set { this.mCivID = value; }
		}
		#endregion

		#region TextID
		int mTextID = TypeExtensions.kNone;
		[Meta.LocStringReference]
		public int TextID
		{
			get { return this.mTextID; }
			set { this.mTextID = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, "civ", ref this.mCivID, DatabaseObjectKind.Civ, true, XML.XmlUtil.kSourceAttr);
			xs.StreamStringID(s, XML.XmlUtil.kNoXmlName, ref this.mTextID, XML.XmlUtil.kSourceCursor);
		}
		#endregion

		#region IComparable Members
		public int CompareTo(GaiaRolloverTextData other)
		{
			if (this.CivID != other.CivID)
				this.CivID.CompareTo(other.CivID);

			return this.TextID.CompareTo(other.TextID);
		}
		#endregion

		#region IEquatable Members
		public bool Equals(GaiaRolloverTextData other)
		{
			return this.CivID == other.CivID
				&&
				this.TextID == other.TextID;
		}
		#endregion
	};
}