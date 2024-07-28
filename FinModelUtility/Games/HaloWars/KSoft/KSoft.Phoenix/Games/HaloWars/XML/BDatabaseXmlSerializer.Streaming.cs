using System.Xml;

namespace KSoft.Phoenix.HaloWars
{
	partial class BDatabaseXmlSerializer
	{
		static bool gRemoveUndefined = false;

		#region Utils
		static void RemoveAllButTheLastElement(IO.XmlElementStream s, XmlElement node, string elementName)
		{
			if (node == null)
				return;

			XmlElement prevNode = null;
			foreach (XmlNode n in node.ChildNodes)
			{
				if (!(n is XmlElement) || n.Name != elementName)
					continue;

				if (prevNode == null)
				{
					prevNode = (XmlElement)n;
					continue;
				}

				FixXmlTraceFixEvent(s, node, "Removing duplicate node {0}",
					n.Name);

				node.RemoveChild(prevNode);
			}
		}

		static bool RemoveAllButTheFirstElement(XmlNodeList elements)
		{
			bool removed = false;

			int x = 0;
			foreach (XmlElement e in elements)
			{
				if (x != 0)
				{
					e.ParentNode.RemoveChild(e);
					removed = true;
				}

				x++;
			}

			return removed;
		}

		static bool RemoveAllElements(XmlNodeList elements)
		{
			bool removed = false;
			foreach (XmlElement e in elements)
			{
				e.ParentNode.RemoveChild(e);
				removed = true;
			}

			return removed;
		}

		static bool RemoveFloatText(IO.XmlElementStream s, XmlElement element, string elementName)
		{
			bool removed = false;
			if (element == null)
				return removed;

			string value = element.InnerText;

			if (!removed)
			{
				int index = value.IndexOf('.');
				if (index >= 0)
				{
					removed = true;
					element.InnerText = value.Substring(0, index);
				}
			}

			if (!removed)
			{
				if (value.Length > 0 && value[value.Length-1] == 'f')
				{
					removed = true;
					element.InnerText = value.Substring(0, value.Length - 1);
				}
			}

			if (removed)
			{
				FixXmlTraceFixEvent(s, element, "Removed floating point data from integer field {0}. {1} -> {2}",
					elementName, value, element.InnerText);
			}

			return removed;
		}
		#endregion

		protected override void FixWeaponTypes()
		{
			// Don't add the types if we're not removing undefined data
			// as we assume the UndefinedHandle/ProtoEnum shit is in use
			if (!gRemoveUndefined) return;

			Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
				"Fixing WeaponTypes with missing types");
			this.Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "Cannon");
			this.Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "needler");
			this.Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "HeavyNeedler");
			this.Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "Plasma");
			this.Database.WeaponTypes.DynamicAdd(new Phx.BWeaponType(), "HeavyPlasma");
		}

		#region Fix GameData
		static void FixGameDataXmlInfectionMapEntryInfected(IO.XmlElementStream s, string infected)
		{
			string xpath = string.Format("InfectionMap/InfectionMapEntry[contains(@infected, '{0}')]", infected);
			var elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0) foreach (XmlElement e in elements)
			{
				var attr = e.Attributes["infected"];
				attr.Value = attr.Value.Replace("_Inf", "_inf");
			}
		}
		static void FixGameDataXmlInfectionMap(Engine.PhxEngineBuild build, IO.XmlElementStream s)
		{
			string xpath;
			XmlNodeList elements;

			if (!ToLowerName(Phx.DatabaseObjectKind.Object))
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@base, 'needlergrunt')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var attr = e.Attributes["base"];
					attr.Value = attr.Value.Replace("needlergrunt", "needlerGrunt");
				}

				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedBrute_01");
				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedJackal_01");
				FixGameDataXmlInfectionMapEntryInfected(s, "fld_inf_InfectedGrunt_01");
			}

			if (!ToLowerName(Phx.DatabaseObjectKind.Squad))
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@infectedSquad, '_Inf')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var attr = e.Attributes["infectedSquad"];
					attr.Value = attr.Value.Replace("_Inf", "_inf");
				}
			}

			#region Alpha only
			if (build == Engine.PhxEngineBuild.Alpha)
			{
				xpath = "InfectionMap/InfectionMapEntry[contains(@base, 'unsc_inf_heavymarine_01')]";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					e.ParentNode.RemoveChild(e);
				}
			}
			#endregion
		}
		static void FixGameDataAmbientLife(IO.XmlElementStream s)
		{
			XmlElement element;
			string elementName;
			// data provides float, engine expects DWORD

			element = XPathSelectElementByName(s, Phx.BGameData.kXmlFileInfo.RootName,
				elementName="ALMaxWanderFrequency");
			RemoveFloatText(s, element, elementName);

			element = XPathSelectElementByName(s, Phx.BGameData.kXmlFileInfo.RootName,
				elementName="ALPredatorCheckFrequency");
			RemoveFloatText(s, element, elementName);

			element = XPathSelectElementByName(s, Phx.BGameData.kXmlFileInfo.RootName,
				elementName="ALPreyCheckFrequency");
			RemoveFloatText(s, element, elementName);
		}
		protected override void FixGameDataXml(IO.XmlElementStream s)
		{
			string xpath = null;
			XmlNodeList elements = null;
			#region Fix LeaderPowerChargeResource
			if (gRemoveUndefined)
			{
				xpath = "LeaderPowerChargeResource";
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					if (e.InnerText != "LeaderPowerCharge")
						continue;

					Debug.Trace.XML.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.kNone,
						"Fixing GameData XPath={0}");
					e.InnerText = "";
				}
			}
			#endregion

			FixGameDataXmlInfectionMap(this.Database.Engine.Build, s);
			FixGameDataAmbientLife(s);
		}

	protected override void FixGameData()
		{
			//FixGameDataResources(Database.GameData);
		}
		#endregion

		#region Fix Objects
		// Fix float values which are in an invalid format for .NET's parsing
		static void FixObjectsXmlInvalidSinglesCobra(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "unsc_veh_cobra_01");
			if (node == null) return;

			var element = node[Phx.BProtoObject.kXmlElementAttackGradeDPS] as XmlElement;

			string txt = element.InnerText;
			int idx = txt.IndexOf('.');
			if (idx != -1 && (idx = txt.IndexOf('.', idx)) != -1)
				element.InnerText = txt.Remove(idx, txt.Length - idx);
		}
		static void FixObjectsXmlInvalidSinglesAlpha(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "cpgn8_air_strategicMissile_01");
			if (node == null) return;

			// <AttackGradeDPS>.</AttackGradeDPS>
			var element = node[Phx.BProtoObject.kXmlElementAttackGradeDPS] as XmlElement;
			element.ParentNode.RemoveChild(element);
		}
		static void FixObjectsXmlInvalidSingles(Engine.PhxEngineBuild build, IO.XmlElementStream s)
		{
			if (build == Engine.PhxEngineBuild.Alpha)
				FixObjectsXmlInvalidSinglesAlpha(s);
			else
				FixObjectsXmlInvalidSinglesCobra(s);
		}
		static void FixObjectsXmlInvalidFlags(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "fx_proj_fldbomb_01");
			if (node == null) return;

			var nodes = node.ChildNodes;
			foreach (XmlNode element in nodes)
				if (element.Name == "Flag" && element.InnerText == "NonCollidable")
				{
					var fc = element.FirstChild;
					fc.Value = "NonCollideable";
				}
		}
		static void FixObjectsXmlInvalidSoundsPowGpWave(IO.XmlElementStream s, XmlNode node)
		{
			//Birth->Exist

			var nodes = node.ChildNodes;
			foreach (XmlNode element in nodes)
				if (element.Name == "Sound")
				{
					var en = (XmlElement)element;
					if (!en.HasAttribute("Type"))
						continue;
					var typeAttr = en.GetAttributeNode("Type");
					if (typeAttr.Value != "Birth")
						continue;

					typeAttr.Value = "Exist";
				}
		}
		static void FixObjectsXmlInvalidSounds(IO.XmlElementStream s)
		{
			XmlNode node;

			node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "pow_gp_wave_01");
			if (node != null)
				FixObjectsXmlInvalidSoundsPowGpWave(s, node);

			node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "pow_gp_wave_02");
			if (node != null)
				FixObjectsXmlInvalidSoundsPowGpWave(s, node);

			node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "pow_gp_wave_03");
			if (node != null)
				FixObjectsXmlInvalidSoundsPowGpWave(s, node);
		}
		static void FixObjectsXmlInvalidCommandId(IO.XmlElementStream s)
		{
			XmlNode node;

			node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "cov_bldg_megaTurret_01");
			if (node != null)
			{
				string xpath;
				XmlNodeList elements;

				xpath = "./Command[text()='CovUnitMegaTurret']";
				elements = node.SelectNodes(xpath);
				if (elements.Count > 0)
				{
					foreach (XmlElement e in elements)
					{
						var fc = e.FirstChild;
						fc.Value = "HookMegaTurret";
					}
				}
			}
		}

		static void FixObjectsXmlInvalidLifeSpan(IO.XmlElementStream s, params string[] objectNames)
		{
			foreach (string name in objectNames)
			{
				var node = XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, name);
				FixObjectXmlInvalidLifeSpan(s.Document, node);
			}
		}
		static void FixObjectXmlInvalidLifeSpan(XmlDocument doc, XmlNode node)
		{
			if (node == null)
				return;

			var badLifeSpan = node["LifeSpan"];
			if (badLifeSpan == null)
				return;

			var lifespan = doc.CreateElement("Lifespan");
			foreach (XmlNode srcNodes in badLifeSpan.ChildNodes)
				lifespan.AppendChild(srcNodes.CloneNode(true));

			node.ReplaceChild(lifespan, badLifeSpan);
		}

		static void FixObjectsXml_fld_air_bomber_01(IO.XmlElementStream s, XmlElement node)
		{
			// remove duplicate FlightLevel values, preferring the last entry
			RemoveAllButTheLastElement(s, node, "FlightLevel");
		}

		static void FixObjectsXml_hook_spawner_FloodRelease(IO.XmlElementStream s, XmlElement node)
		{
			// remove duplicate MaxContained values, preferring the last entry
			RemoveAllButTheLastElement(s, node, "MaxContained");
		}

		static void FixObjectsXml_for_air_monitor(IO.XmlElementStream s, XmlElement node)
		{
			// remove duplicate CombatValue values, preferring the last entry
			RemoveAllButTheLastElement(s, node, "CombatValue");
		}

		static void FixObjectsXml_for_air_attractor_01(IO.XmlElementStream s, XmlElement node)
		{
			// remove duplicate LOS values, preferring the last entry
			RemoveAllButTheLastElement(s, node, "LOS");
		}

		protected override void FixObjectsXml(IO.XmlElementStream s)
		{
			var build = this.Database.Engine.Build;
			FixObjectsXmlInvalidSingles(build, s);
			if (build == Engine.PhxEngineBuild.Release)
			{
				FixObjectsXmlInvalidFlags(s);
				FixObjectsXmlInvalidSounds(s); // #TODO does this need to be done for Alpha too?
				FixObjectsXmlInvalidCommandId(s);
				FixObjectsXmlInvalidLifeSpan(s,
					"fx_hijacked",
					"fx_unitLevelUp",
					"fx_unitLevelUpHigh",
					"fx_unitLevelUpLow");
				FixObjectsXml_fld_air_bomber_01(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "fld_air_bomber_01"));
				FixObjectsXml_hook_spawner_FloodRelease(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "hook_spawner_FloodRelease_01"));
				FixObjectsXml_hook_spawner_FloodRelease(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "hook_spawner_FloodRelease_02"));
				FixObjectsXml_for_air_monitor(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "for_air_monitor_01"));
				FixObjectsXml_for_air_monitor(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "for_air_monitor_02"));
				FixObjectsXml_for_air_monitor(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "for_air_monitor_04"));
				FixObjectsXml_for_air_attractor_01(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoObject.kBListXmlParams, "for_air_attractor_01"));
			}
		}
		#endregion

		#region Fix Squads
		static void FixSquadsXmlAlphaCostElements(IO.XmlElementStream s)
		{
			const string kAttrNameOld = "ResourceType";
			const string kAttrName = "resourcetype";

			string xpath = "Squad/Cost[@" + kAttrNameOld + "]";

			var elements = s.Cursor.SelectNodes(xpath);

			foreach (XmlElement e in elements)
			{
				var attr_old = e.Attributes[kAttrNameOld];
				var attr = s.Document.CreateAttribute(kAttrName);
				attr.Value = attr_old.Value;
				e.Attributes.InsertBefore(attr, attr_old);
				e.RemoveAttribute(kAttrNameOld);
			}
		}
		static void FixSquadsXmlAphaUndefinedObjects(IO.XmlElementStream s, params string[] squadNames)
		{
			foreach (string name in squadNames)
			{
				var node = XPathSelectNodeByName(s, Phx.BProtoSquad.kBListXmlParams, name);
				if (node != null)
					node.ParentNode.RemoveChild(node);
			}
		}
		static void FixSquadsXmlAlpha(IO.XmlElementStream s)
		{
			if (gRemoveUndefined) FixSquadsXmlAphaUndefinedObjects(s,
				"unsc_air_shortsword_01", "unsc_con_turret_01", "unsc_con_base_01",
				"cov_inf_kamikazeGrunt_01", // needs to be 'cpgn_inf_kamikazegrunt_01', but fuck updating it
				"cov_con_turret_01", "cov_con_node_01", "cov_con_base_01"
				);
			FixSquadsXmlAlphaCostElements(s);
		}

		static void FixSquadsXmlSounds(IO.XmlElementStream s)
		{
			XmlNode node;

			node = XPathSelectNodeByName(s, Phx.BProtoSquad.kBListXmlParams, "cov_veh_bruteChopper_01");
			FixSquadsXmlSoundsKillEnemy(node);
			node = XPathSelectNodeByName(s, Phx.BProtoSquad.kBListXmlParams, "cov_veh_ghost_01");
			FixSquadsXmlSoundsKillEnemy(node);
		}
		static void FixSquadsXmlSoundsKillEnemy(XmlNode node)
		{
			if (node == null)
				return;

			var xpath = "./Sound[@Type='KillEnemy']";
			var elements = node.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var typeAttr = e.GetAttributeNode("Type");
					typeAttr.Value = Phx.BSquadSoundType.KilledEnemy.ToString();
				}
			}
		}

		static void FixSquadsXml_for_air_sentinel_03(IO.XmlElementStream s, XmlElement node)
		{
			// remove duplicate BuildPoints values, preferring the last entry
			RemoveAllButTheLastElement(s, node, "BuildPoints");
		}

		protected override void FixSquadsXml(IO.XmlElementStream s)
		{
			if (this.Database.Engine.Build == Engine.PhxEngineBuild.Alpha)
				FixSquadsXmlAlpha(s);
			else
			{
				FixSquadsXmlSounds(s);
				FixSquadsXml_for_air_sentinel_03(s, (XmlElement)XPathSelectNodeByName(s, Phx.BProtoSquad.kBListXmlParams, "for_air_sentinel_03"));
			}
		}
		#endregion

		#region Fix Techs
		static void FixTechsXmlBadNames(IO.XmlElementStream s, XML.BListXmlParams op, Engine.PhxEngineBuild build)
		{
			const string k_attr_command_data = "CommandData";
			const string k_element_target = "Target";

			string invalid_command_data_format = string.Format(
				"/{0}/{1}/Effects/Effect[@{2}='",
				op.RootName, op.ElementName, k_attr_command_data) + "{0}']";
			string invalid_target_format = string.Format(
				"/{0}/{1}/Effects/Effect[Target='",
				op.RootName, op.ElementName) + "{0}']";

			string xpath;
			XmlNodeList elements;

			if (!ToLowerName(Phx.DatabaseObjectKind.Unit))
			{
				#region Alpha only
				if (build == Engine.PhxEngineBuild.Alpha)
				{
					xpath = string.Format(invalid_target_format, "cov_inf_eliteleader_01");
					elements = s.Cursor.SelectNodes(xpath);
					if (elements.Count > 0) foreach (XmlElement e in elements)
					{
						var fc = e[k_element_target].FirstChild;
						fc.Value = "cov_inf_eliteLeader_01";
					}
				}
				#endregion
			}
			#region Alpha only
			if (build == Engine.PhxEngineBuild.Alpha)
			{
				xpath = string.Format(invalid_target_format, "cov_inf_elite_leader01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "cov_inf_eliteLeader_01";
				}
			}
			#endregion

			if (!ToLowerName(Phx.DatabaseObjectKind.Tech))
			{
				#region unsc_MAC_upgrade
				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade1");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade1";

				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade2");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade2";

				xpath = string.Format(invalid_command_data_format, "unsc_mac_upgrade3");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
						e.Attributes[k_attr_command_data].Value = "unsc_MAC_upgrade3";
				#endregion

				#region unsc_flameMarine_upgrade
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade1");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade1";
				}
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade2");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade2";
				}
				xpath = string.Format(invalid_target_format, "unsc_flamemarine_upgrade3");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_flameMarine_upgrade3";
				}
				#endregion
			}

			if (!ToLowerName(Phx.DatabaseObjectKind.Squad))
			{
				xpath = string.Format(invalid_target_format, "unsc_inf_flamemarine_01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_inf_flameMarine_01";
				}
				xpath = string.Format(invalid_target_format, "unsc_inf_Marine_01");
				elements = s.Cursor.SelectNodes(xpath);
				if (elements.Count > 0) foreach (XmlElement e in elements)
				{
					var fc = e[k_element_target].FirstChild;
					fc.Value = "unsc_inf_marine_01";
				}
			}
		}
		// Rename the SubType attribute to be all lowercase (subType is only uppercase for 'TurretYawRate'...)
		static void FixTechsXmlEffectsDataSubType(XmlDocument doc, XmlNode n)
		{
			const string kAttrNameOld = "subType";
			const string kAttrName = "subtype";

			string xpath = "Effects/Effect[@" + kAttrNameOld + "]";

			var elements = n.SelectNodes(xpath);

			foreach (XmlElement e in elements)
			{
				var attr_old = e.Attributes[kAttrNameOld];
				var attr = doc.CreateAttribute(kAttrName);
				attr.Value = attr_old.Value;
				e.Attributes.InsertBefore(attr, attr_old);
				e.RemoveAttribute(kAttrNameOld);
			}
		}
		// Remove non-existent ProtoTechs that are referenced by effects
		static void FixTechsXmlEffectsInvalid(IO.XmlElementStream s, XML.BListXmlParams op, Engine.PhxEngineBuild build)
		{
			string xpath_target = string.Format(
				"/{0}/{1}/Effects/Effect/Target",
				op.RootName, op.ElementName);
			XmlNodeList elements;

			if (build == Engine.PhxEngineBuild.Release)
			{
				elements = s.Document.SelectNodes(xpath_target);

				foreach (XmlElement e in elements)
				{
					if (e.InnerText != "unsc_turret_upgrade3") continue;

					FixXmlTraceFixEvent(s, e, "Removing undefined Target from Tech Effect",
						e.InnerText);

					var p = e.ParentNode;
					p.ParentNode.RemoveChild(p);
				}
			}
		}
		protected override void FixTechsXml(IO.XmlElementStream s)
		{
			var node = XPathSelectNodeByName(s, Phx.BProtoTech.kBListXmlParams, "unsc_scorpion_upgrade3");
			if (node != null) FixTechsXmlEffectsDataSubType(s.Document, node);

			node = XPathSelectNodeByName(s, Phx.BProtoTech.kBListXmlParams, "unsc_grizzly_upgrade0");
			if (node != null) FixTechsXmlEffectsDataSubType(s.Document, node);

			FixTechsXmlEffectsInvalid(s, Phx.BProtoTech.kBListXmlParams, this.Database.Engine.Build);
			if(gRemoveUndefined)
				FixTechsXmlBadNames(s, Phx.BProtoTech.kBListXmlParams, this.Database.Engine.Build);
		}
		#endregion

		#region Fix Powers
		protected override void FixPowersXml(IO.XmlElementStream s)
		{
			FixPowersXmlUndefinedTechPrereqs(s, Phx.BProtoPower.kBListXmlParams, this.Database.Engine.Build);
		}
		static void FixPowersXmlUndefinedTechPrereqs(IO.XmlElementStream s, XML.BListXmlParams op, Engine.PhxEngineBuild build)
		{
			string xpath_target = string.Format(
				"/{0}/{1}/Attributes/TechPrereq",
				op.RootName, op.ElementName);
			XmlNodeList elements;

			if (build == Engine.PhxEngineBuild.Release)
			{
				elements = s.Document.SelectNodes(xpath_target);

				foreach (XmlElement e in elements)
				{
					if ( // UnscOdstDrop
						e.InnerText != "unsc_odst_upgrade1" &&
						// CpgnOdstDrop
						e.InnerText != "cpgn_odst_upgrade" &&
						// UnscCpgn13OrbitalBombard
						e.InnerText != "unsc_age4")
						continue;

					FixXmlTraceFixEvent(s, e, "Removing undefined TechPrereq from Power '{0}'",
						e.InnerText);

					var p = e.ParentNode;
					p.RemoveChild(e);
				}
			}
		}
		#endregion

		#region Fix Tactics
		static void FixTacticsXmlBadWeapons(IO.XmlElementStream s, string name)
		{
			string xpath;
			XmlNodeList elements;

			// see: fx_proj_beam_01
			xpath = "Weapon[WeaponType='ForunnerBeam']";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["WeaponType"].FirstChild;
					fc.Value = "Beam";
				}
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}

			// see: pow_gp_orbitalbombardment
			xpath = "Weapon/DamageRatingOverride[@type='TurretBuilding']";
			elements = s.Cursor.SelectNodes(xpath);
			if (RemoveAllElements(elements))
			{
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}

			// see: unsc_veh_cobra_01, cpgn_scn10_warthog_01
			xpath = "Weapon/DamageRatingOverride[@type='Unarmored']"
				+ " | Weapon/DamageRatingOverride[@type='Air']"
				+ " | Weapon/DamageRatingOverride[@type='Vehicle']";
			elements = s.Cursor.SelectNodes(xpath);
			if (RemoveAllElements(elements))
			{
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}
		}
		static void FixTacticsXmlBadActionWeaponNames(IO.XmlElementStream s, string name)
		{
			string xpath;
			XmlNodeList elements;

			// see: fx_proj_rocket_01,02,03
			xpath = "Action[contains(Weapon, '>')]";
			elements = s.Cursor.SelectNodes(xpath);
			if (elements.Count > 0)
			{
				foreach (XmlElement e in elements)
				{
					var fc = e["Weapon"].FirstChild;
					fc.Value = fc.Value.Substring(1);
				}
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}

			// see: pow_gp_rage_impact
			if (gRemoveUndefined)
			{
				xpath = "Action[Weapon='RageShockwave']";
				elements = s.Cursor.SelectNodes(xpath);
				if (RemoveAllElements(elements))
				{
					FixTacticsTraceFixEvent(s, name, xpath);
					return;
				}
			}
		}
		static void FixTacticsXmlBadActions(IO.XmlElementStream s, string name)
		{
			FixTacticsXmlBadActionWeaponNames(s, name);
			string xpath;
			XmlNodeList elements;

			// see: cov_inf_elitecommando_01
			xpath = "Action[Name='GatherSupplies']";
			elements = s.Cursor.SelectNodes(xpath);
			// I'm going to assume the first Action supersedes all proceeding Actions with the same name
			if (RemoveAllButTheFirstElement(elements))
			{
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}

			// see: cpgn_scn10_warthog_01
			xpath = "Action[Name='Capture']";
			elements = s.Cursor.SelectNodes(xpath);
			// I'm going to assume the first Action supersedes all proceeding Actions with the same name
			if (RemoveAllButTheFirstElement(elements))
			{
				// #TODO: Should this be added? It appears in the 2nd instance
				// <Anim>Build</Anim>
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}

			if (!gRemoveUndefined)
				return;

			// see: cov_inf_grunt_01, cov_inf_needlergrunt_01, cov_inf_elite_01,
			// creep_inf_grunt_01, creep_inf_needlergrunt_01
			xpath = "Action[BaseDPSWeapon='InCoverPlasmaPistolAttackAction']"
				+ " | Action[BaseDPSWeapon='InCoverNeedlerAttackAction']"
				+ " | Action[BaseDPSWeapon='IcCoverPlasmaRifleAttackAction']"
				+ " | Action[BaseDPSWeapon='PlasmaPistolAttackAction']";
			elements = s.Cursor.SelectNodes(xpath);
			if (RemoveAllElements(elements))
			{
				FixTacticsTraceFixEvent(s, name, xpath);
				return;
			}
		}
		protected override void FixTacticsXml(IO.XmlElementStream s, string name)
		{
			FixTacticsXmlBadWeapons(s, name);
			FixTacticsXmlBadActions(s, name);
		}
		#endregion
	};
}