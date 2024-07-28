using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed partial class BDatabase
		: IO.IEndianStreamSerializable
	{
		public List<string> Civs = new List<string>(); // max=0x64
		public List<string> Leaders = new List<string>(); // max=0x12C
		public List<string> Abilities = new List<string>(); // max=0x3E8
		public List<string> ProtoVisuals = new List<string>(); // max=0x2710
		public List<string> Models = new List<string>(); // max=0x2710
		public List<string> Animations = new List<string>(); // max=0x2710
		public List<string> TerrainEffects = new List<string>(); // max=0x1F4
		public List<string> ProtoImpactEffects = new List<string>(); // max=0x1F4
		public List<string> LightEffects = new List<string>(); // max=0x3E8
		public List<string> ParticleGateways = new List<string>(); // max=0x3E8
		public List<GenericProtoObjectEntry> GenericProtoObjects { get; private set; } // max=0x4E20
		public List<ProtoSquadEntry> ProtoSquads { get; private set; } // max=0x4E20
		public List<string> ProtoTechs { get; private set; } // max=0x2710
		public List<string> ProtoPowers { get; private set; } // max=0x3E8
		public List<string> ProtoObjects { get; private set; } // max=0x4E20 includes objecttypes
		public List<string> Resources { get; private set; } // max=0xC8
		public List<string> Rates { get; private set; } // max=0xC8
		public List<string> Populations { get; private set; } // max=0xC8
		public List<string> WeaponTypes { get; private set; } // max=0x2710
		public List<string> DamageTypes { get; private set; } // max=0xC8
		public List<TemplateEntry> Templates { get; private set; } // max=0x3E8
		public List<string> AnimTypes { get; private set; } // max=0x3E8
		public List<string> EffectTypes { get; private set; } // max=0x7D0
		public List<string> Actions { get; private set; } // max=0xFA
		public List<CondensedListItem16<Tactic>> Tactics { get; private set; }
		int NumUniqueProtoObjects; // max=0x64
		public List<CondensedListItemValue32<DataTagValue>> Shapes { get; private set; }
		public List<CondensedListItemValue32<DataTagValue>> PhysicsInfo { get; private set; }
		public List<ProtoIcon> ProtoIcons { get; private set; } // max=0x3E8

		public BDatabase()
		{
			this.Civs = new List<string>();
			this.Leaders = new List<string>();
			this.Abilities = new List<string>();
			this.ProtoVisuals = new List<string>();
			this.Models = new List<string>();
			this.Animations = new List<string>();
			this.TerrainEffects = new List<string>();
			this.ProtoImpactEffects = new List<string>();
			this.LightEffects = new List<string>();
			this.ParticleGateways = new List<string>();
			this.GenericProtoObjects = new List<GenericProtoObjectEntry>();
			this.ProtoSquads = new List<ProtoSquadEntry>();
			this.ProtoTechs = new List<string>();
			this.ProtoPowers = new List<string>();
			this.ProtoObjects = new List<string>();
			this.Resources = new List<string>();
			this.Rates = new List<string>();
			this.Populations = new List<string>();
			this.WeaponTypes = new List<string>();
			this.DamageTypes = new List<string>();
			this.Templates = new List<TemplateEntry>();
			this.AnimTypes = new List<string>();
			this.EffectTypes = new List<string>();
			this.Actions = new List<string>();
			this.Tactics = new List<CondensedListItem16<Tactic>>();
			this.Shapes = new List<CondensedListItemValue32<DataTagValue>>();
			this.PhysicsInfo = new List<CondensedListItemValue32<DataTagValue>>();
			this.ProtoIcons = new List<ProtoIcon>();
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamCollection(s, this.Civs);
			BSaveGame.StreamCollection(s, this.Leaders);
			BSaveGame.StreamCollection(s, this.Abilities);
			BSaveGame.StreamCollection(s, this.ProtoVisuals);
			BSaveGame.StreamCollection(s, this.Models);
			BSaveGame.StreamCollection(s, this.Animations);
			BSaveGame.StreamCollection(s, this.TerrainEffects);
			BSaveGame.StreamCollection(s, this.ProtoImpactEffects);
			BSaveGame.StreamCollection(s, this.LightEffects);
			BSaveGame.StreamCollection(s, this.ParticleGateways);
			BSaveGame.StreamCollection(s, this.GenericProtoObjects);
			BSaveGame.StreamCollection(s, this.ProtoSquads);
			BSaveGame.StreamCollection(s, this.ProtoTechs);
			BSaveGame.StreamCollection(s, this.ProtoPowers);
			BSaveGame.StreamCollection(s, this.ProtoObjects);
			BSaveGame.StreamCollection(s, this.Resources);
			BSaveGame.StreamCollection(s, this.Rates);
			BSaveGame.StreamCollection(s, this.Populations);
			BSaveGame.StreamCollection(s, this.WeaponTypes);
			BSaveGame.StreamCollection(s, this.DamageTypes);
			BSaveGame.StreamCollection(s, this.Templates);
			BSaveGame.StreamCollection(s, this.AnimTypes);
			BSaveGame.StreamCollection(s, this.EffectTypes);
			BSaveGame.StreamCollection(s, this.Actions);
			BSaveGame.StreamList(s, this.Tactics, kTacticsListInfo);

			s.Stream(ref this.NumUniqueProtoObjects);
			s.StreamSignature((uint) this.NumUniqueProtoObjects);

			BSaveGame.StreamList(s, this.Shapes, kDataTagsListInfo);
			BSaveGame.StreamList(s, this.PhysicsInfo, kDataTagsListInfo);

			BSaveGame.StreamCollection(s, this.ProtoIcons);

			s.StreamSignature(cSaveMarker.DB);
		}
		#endregion
	};
}