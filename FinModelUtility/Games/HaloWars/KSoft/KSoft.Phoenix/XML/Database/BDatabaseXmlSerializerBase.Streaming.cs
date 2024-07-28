using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	partial class BDatabaseXmlSerializerBase
	{
		public bool ForceNoRootElementStreaming = true;

		protected virtual void PreStreamXml(FA mode)
		{
			if (mode == FA.Read)
			{
				if (this.mIsPreloading)
					this.PreloadTactics();
			}
		}
		protected virtual void PostStreamXml(FA mode)
		{
		}

		#region Tactics
		void PreloadTactics()
		{
			if (this.Database.Tactics.Count > 0)
				return;

			foreach (string tactic_filename in this.GameEngine.Directories.GetFiles(Engine.ContentStorage.Game, Engine.GameDirectory.Tactics,
			                                                                     "*" + Phx.BTacticData.kFileExt))
			{
				string tactic_name = System.IO.Path.GetFileNameWithoutExtension(tactic_filename);

				var td = new Phx.BTacticData();
				td.SourceFileName = tactic_filename;

				this.Database.Tactics.DynamicAdd(td, tactic_name);
			}

			foreach (string tactic_filename in this.GameEngine.Directories.GetFiles(Engine.ContentStorage.Game, Engine.GameDirectory.Tactics,
			                                                                     "*" + Phx.BTacticData.kFileExt + Xmb.XmbFile.kFileExt))
			{
				// get rid of .xmb, then .tactics
				string tactic_name = System.IO.Path.GetFileNameWithoutExtension(tactic_filename);
				tactic_name = System.IO.Path.GetFileNameWithoutExtension(tactic_name);
				if (this.Database.Tactics.TryGetId(tactic_name).IsNotNone())
					continue;

				var td = new Phx.BTacticData();
				td.SourceFileName = tactic_filename;
				td.SourceXmlFileIsXmb = true;

				this.Database.Tactics.DynamicAdd(td, tactic_name);
			}
		}

		void StreamTactic(IO.XmlElementStream s, Phx.BTacticData tactic)
		{
			if (s.IsReading && this.IsNotPreloading)
				this.FixTacticsXml(s, tactic.Name);
			tactic.Serialize(s);
		}
		#endregion

		/// <remarks>For streaming directly from gamedata.xml</remarks>
		void StreamXmlGameData(IO.XmlElementStream s)
		{
			if(s.IsReading)
				this.FixGameDataXml(s);
			this.Database.GameData.StreamGameData(s);
		}

		/// <remarks>For streaming directly from hpbars.xml</remarks>
		void StreamXmlHPBars(IO.XmlElementStream s)
		{
			this.Database.HPBars.StreamHPBarData(s);
		}

		void PreloadStringTable(IO.XmlElementStream s)
		{
			this.Database.EnglishStringTable.Serialize(s);
		}

		#region DatabaseObjectKind stuff
		// #NOTE place new DatabaseObjectKind code here

		void PreloadDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mDamageTypesSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from damagetypes.xml</remarks>
		void StreamXmlDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.mDamageTypesSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from impacteffects.xml</remarks>
		void StreamXmlImpactEffects(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.mImpactEffectsSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from terraintiletypes.xml</remarks>
		void StreamXmlTerrainTileTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.TerrainTileTypes, Phx.TerrainTileType.kBListXmlParams, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from weapontypes.xml</remarks>
		void StreamXmlWeaponTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.WeaponTypes, Phx.BWeaponType.kBListXmlParams, this.ForceNoRootElementStreaming);
			if (s.IsReading)
				this.FixWeaponTypes();
		}
		/// <remarks>For streaming directly from UserClasses.xml</remarks>
		void StreamXmlUserClasses(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.UserClasses, Phx.BUserClass.kBListXmlParams, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objecttypes.xml</remarks>
		void StreamXmlObjectTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.ObjectTypes, Phx.BDatabaseBase.kObjectTypesXmlParams, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from abilities.xml</remarks>
		void StreamXmlAbilities(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Abilities, Phx.BAbility.kBListXmlParams, this.ForceNoRootElementStreaming);
		}

		void PreloadObjects(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mObjectsSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objects.xml</remarks>
		void StreamXmlObjects(IO.XmlElementStream s)
		{
			if (s.IsReading)
			{
				this.FixObjectsXml(s);
			}

			XmlUtil.Serialize(s, this.mObjectsSerializer, this.ForceNoRootElementStreaming);
		}

		void PreloadSquads(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mSquadsSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from squads.xml</remarks>
		void StreamXmlSquads(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixSquadsXml(s);
			XmlUtil.Serialize(s, this.mSquadsSerializer, this.ForceNoRootElementStreaming);

			XmlUtil.Serialize(s, this.Database.MergedSquads, Phx.BProtoMergedSquads.kBListXmlParams);
			this.Database.ShieldBubbleTypes.Serialize(s);
		}

		void PreloadPowers(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mPowersSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from powers.xml</remarks>
		void StreamXmlPowers(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixPowersXml(s);
			XmlUtil.Serialize(s, this.mPowersSerializer, this.ForceNoRootElementStreaming);
		}

		void PreloadTechs(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mTechsSerializer, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from techs.xml</remarks>
		void StreamXmlTechs(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixTechsXml(s);
			XmlUtil.Serialize(s, this.mTechsSerializer, this.ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from civs.xml</remarks>
		void StreamXmlCivs(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Civs, Phx.BCiv.kBListXmlParams, this.ForceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from leaders.xml</remarks>
		void StreamXmlLeaders(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Leaders, Phx.BLeader.kBListXmlParams, this.ForceNoRootElementStreaming);
		}

		#region Update
		/// <remarks>For streaming directly from objects_update.xml</remarks>
		void StreamXmlObjectsUpdate(IO.XmlElementStream s)
		{
			//if(s.IsReading) FixObjectsXml(s);
			XmlUtil.SerializeUpdate(s, this.mObjectsSerializer, this.ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from squads_update.xml</remarks>
		void StreamXmlSquadsUpdate(IO.XmlElementStream s)
		{
			//if (s.IsReading) FixSquadsXml(s);
			XmlUtil.SerializeUpdate(s, this.mSquadsSerializer, this.ForceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from techs_update.xml</remarks>
		void StreamXmlTechsUpdate(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixTechsXml(s);
			XmlUtil.SerializeUpdate(s, this.mTechsSerializer, this.ForceNoRootElementStreaming);
		}
		#endregion
		#endregion

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = this.Database;

			this.PreStreamXml(s.StreamMode);

			// #NOTE place new DatabaseObjectKind code here
			XmlUtil.Serialize(s, db.DamageTypes, Phx.BDamageType.kBListXmlParams);
			XmlUtil.Serialize(s, db.WeaponTypes, Phx.BWeaponType.kBListXmlParams);
			XmlUtil.Serialize(s, db.ImpactEffects, Phx.BProtoImpactEffect.kBListXmlParams);
			XmlUtil.Serialize(s, db.TerrainTileTypes, Phx.TerrainTileType.kBListXmlParams);
			XmlUtil.Serialize(s, db.UserClasses, Phx.BUserClass.kBListXmlParams);
			XmlUtil.Serialize(s, db.ObjectTypes, Phx.BDatabaseBase.kObjectTypesXmlParams);
			db.HPBars.Serialize(s);
			// #NOTE since we don't preload, the Infection squad map won't properly resolve squads
			db.GameData.Serialize(s);
			XmlUtil.Serialize(s, db.Abilities, Phx.BAbility.kBListXmlParams);
			XmlUtil.Serialize(s, db.Objects, Phx.BProtoObject.kBListXmlParams);
			XmlUtil.Serialize(s, db.Squads, Phx.BProtoSquad.kBListXmlParams);
			XmlUtil.Serialize(s, db.Powers, Phx.BProtoPower.kBListXmlParams);
			XmlUtil.Serialize(s, db.Techs, Phx.BProtoTech.kBListXmlParams);
			XmlUtil.Serialize(s, db.Civs, Phx.BCiv.kBListXmlParams);
			XmlUtil.Serialize(s, db.Leaders, Phx.BLeader.kBListXmlParams);

			XmlUtil.Serialize(s, db.MergedSquads, Phx.BProtoMergedSquads.kBListXmlParams);
			db.ShieldBubbleTypes.Serialize(s);

			using (s.EnterCursorBookmark(Phx.LocStringTable.kBListXmlParams.RootName))
			{
				db.EnglishStringTable.Serialize(s);
			}

			this.PostStreamXml(s.StreamMode);
		}
	};
}