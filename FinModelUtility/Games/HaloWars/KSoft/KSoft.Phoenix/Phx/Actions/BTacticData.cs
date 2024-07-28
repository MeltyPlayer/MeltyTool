using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.TacticData)]
	public sealed class BTacticData
		: Collections.BListAutoIdObject
		, IProtoDataObjectDatabaseProvider
	{
		public const string kFileExt = ".tactics";

		#region Xml constants
		public const string kXmlRoot = "TacticData";

		public static Engine.XmlFileInfo CreateFileInfo(System.IO.FileAccess mode, string filename)
		{
			return new Engine.XmlFileInfo()
			{
				Location = Engine.ContentStorage.UpdateOrGame,
				Directory = Engine.GameDirectory.Tactics,

				RootName = kXmlRoot,
				FileName = filename,

				Writable = mode == System.IO.FileAccess.Write,
			};
		}
		#endregion

		public string SourceFileName { get; set; }
		public Engine.XmlFileInfo SourceXmlFile { get; set; }
		public bool SourceXmlFileIsXmb { get; set; }

		public Collections.BListAutoId<		BWeapon> Weapons { get; private set; }
			= new Collections.BListAutoId<	BWeapon>();
		public Collections.BListAutoId<		BProtoAction> Actions { get; private set; }
			= new Collections.BListAutoId<	BProtoAction>();

		public BTactic Tactic { get; private set; }
			= new BTactic();

		public BTacticData()
		{
			this.InitializeDatabaseInterfaces();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			this.Weapons.SetupDatabaseInterface();
			//TacticStates.SetupDatabaseInterface();
			this.Actions.SetupDatabaseInterface();
		}

		internal Collections.IBTypeNames GetNamesInterface(TacticDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != TacticDataObjectKind.None);

			switch (kind)
			{
			case TacticDataObjectKind.Weapon:		return this.Weapons;
			//case TacticDataObjectKind.TacticState:	return TacticStates;
			case TacticDataObjectKind.Action:		return this.Actions;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(TacticDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != TacticDataObjectKind.None);

			switch (kind)
			{
			case TacticDataObjectKind.Weapon:		return this.Weapons;
			//case TacticDataObjectKind.TacticState:	return TacticStates;
			case TacticDataObjectKind.Action:		return this.Actions;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region BListAutoIdObject Members
		internal bool StreamID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, ref int dbid,
			TacticDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.kSourceElement)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.kNoXmlName));
			Contract.Requires(kind != TacticDataObjectKind.None);

			string id_name = null;
			bool was_streamed = true;
			bool to_lower = false;

			if (s.IsReading)
			{
				if (isOptional)
					was_streamed = s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);

				if (was_streamed)
				{
					IProtoDataObjectDatabaseProvider provider = this;
					dbid = provider.GetId((int)kind, id_name);
					Contract.Assert(dbid.IsNotNone());
				}
				else
					dbid = TypeExtensions.kNone;
			}
			else if (s.IsWriting && dbid.IsNotNone())
			{
				IProtoDataObjectDatabaseProvider provider = this;
				id_name = provider.GetName((int)kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(id_name));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref id_name, to_lower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref id_name, to_lower, xmlSource, intern: true);
			}

			return was_streamed;
		}
		internal static void StreamWeaponID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int id)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName, ref id, TacticDataObjectKind.Weapon, false, XML.XmlUtil.kSourceCursor);
		}
		internal static void StreamProtoActionID<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int protoActionId)
			where TDoc : class
			where TCursor : class
		{
			td.StreamID(s, XML.XmlUtil.kNoXmlName, ref protoActionId, TacticDataObjectKind.Action, false, XML.XmlUtil.kSourceCursor);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			using (s.EnterUserDataBookmark(this))
			{
				XML.XmlUtil.Serialize(s, this.Weapons, BWeapon.kBListXmlParams);
				//XML.XmlUtil.Serialize(s, TacticStates, BTacticState.kBListXmlParams);
				XML.XmlUtil.Serialize(s, this.Actions, BProtoAction.kBListXmlParams);
				this.Tactic.Serialize(s);
			}
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return this.SourceXmlFile; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};
}