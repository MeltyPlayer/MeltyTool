using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public partial class PhxEngine
	{
		public PhxEngineBuild Build { get; private set; }

		public bool TargetsXbox360 { get; private set; }

		public GameDirectories Directories { get; private set; }

		public Phx.BDatabaseBase Database { get; private set; }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		internal Dictionary<XmlFileInfo, XmlFileLoadState> XmlFileLoadStatus { get; private set; }
			= new Dictionary<XmlFileInfo, XmlFileLoadState>();

		public event EventHandler<XmlFileLoadStateChangedArgs> XmlFileLoadStateChanged;

		internal void UpdateFileLoadStatus(XmlFileInfo file, XmlFileLoadState state)
		{
			Contract.Requires(file != null);

			lock (this.XmlFileLoadStatus)
				this.XmlFileLoadStatus[file] = state;

			var handler = this.XmlFileLoadStateChanged;
			if (handler != null)
			{
				var args = new XmlFileLoadStateChangedArgs(file, state);
				handler(this, args);
			}
		}

		public XmlFileLoadState GetFileLoadStatus(XmlFileInfo file)
		{
			Contract.Requires(file != null);

			XmlFileLoadState state;
			lock (this.XmlFileLoadStatus)
				if (!this.XmlFileLoadStatus.TryGetValue(file, out state))
					return XmlFileLoadState.NotLoaded;

			return state;
		}

		public bool HasAlreadyPreloaded { get; private set; }
		public virtual bool Preload()
		{
			if (this.HasAlreadyPreloaded)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load preload data: " + "preload has already be performed");
				return false;
			}

			bool success = this.Database.Preload();

			if (success)
			{
				this.HasAlreadyPreloaded = true;
			}

			return success;
		}

		public bool HasAlreadyLoaded { get; private set; }
		public virtual bool Load()
		{
			Exception exception = null;
			bool success = false;

			try
			{
				do
				{
					if (this.HasAlreadyLoaded)
					{
						exception = new Exception("Load has already been performed");
						break;
					}

					if (!this.Database.Load())
					{
						exception = new Exception("Database.Load failed");
						break;
					}

					if (!this.Database.LoadAllTactics())
					{
						exception = new Exception("Database.LoadAllTactics failed");
						break;
					}

					success = true;

				} while (false);
			} catch (Exception ex)
			{
				exception = ex;
			}

			if (!success)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load engine data",
					exception);
			}
			else
			{
				this.HasAlreadyLoaded = true;
			}

			return success;
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(string fileName)
		{
			string ext = System.IO.Path.GetFileNameWithoutExtension(fileName);

			var xml_or_xmb = ext.Equals(Xmb.XmbFile.kFileExt, StringComparison.OrdinalIgnoreCase)
				? GetXmlOrXmbFileResult.Xmb
				: GetXmlOrXmbFileResult.Xml;

			return this.OpenXmlOrXmbForRead(xml_or_xmb, fileName);
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(GetXmlOrXmbFileResult xmlOrXmb, string fileName)
		{
			switch (xmlOrXmb)
			{
				case GetXmlOrXmbFileResult.Xml:
					return new IO.XmlElementStream(fileName, System.IO.FileAccess.Read);

				case GetXmlOrXmbFileResult.Xmb:
					return this.OpenXmbForRead(fileName);
			}

			return null;
		}
		public IO.XmlElementStream OpenXmbForRead(string xmbFile)
		{
			var va_size = this.TargetsXbox360
				? Shell.ProcessorSize.x32
				: Shell.ProcessorSize.x64;
			var endian_format = Shell.EndianFormat.Big;

			byte[] file_bytes = System.IO.File.ReadAllBytes(xmbFile);

			using (var xmb_ms = new System.IO.MemoryStream(file_bytes, false))
			using (var xmb = new IO.EndianStream(xmb_ms, endian_format, System.IO.FileAccess.Read))
			using (var xml_ms = new System.IO.MemoryStream(IntegerMath.kMega * 1))
			{
				xmb.StreamMode = System.IO.FileAccess.Read;

				Resource.ResourceUtils.XmbToXml(xmb, xml_ms, va_size);
				// need to do this else we'll get a Root element is missing exception
				xml_ms.Position = 0;

				var xml = new IO.XmlElementStream(xml_ms, System.IO.FileAccess.Read, streamNameOverride: xmbFile);
				return xml;
			}
		}

		public ObjectDatabaseForFileResult GetObjectDatabase(XmlFileInfo file)
		{
			if (file == null)
				return ObjectDatabaseForFileResult.Null;

			var status = this.GetFileLoadStatus(file);
			if (status < XmlFileLoadState.Preloaded)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} when its load status was {1}",
					file, status));
			}

			var kvp = this.GetObjectDatabaseForFile(file);

			if (kvp.Key == null)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} which didn't resolve to a DB"));
			}

			return new ObjectDatabaseForFileResult(file, kvp.Key, kvp.Value);
		}

		protected virtual KeyValuePair<Phx.ProtoDataObjectDatabase, int> GetObjectDatabaseForFile(XmlFileInfo file)
		{
			Phx.ProtoDataObjectDatabase db = null;
			int specificObjectKind = TypeExtensions.kNone;

			if (file == Phx.BGameData.kXmlFileInfo)
			{
				db = this.Database.GameData.ObjectDatabase;
				specificObjectKind = PhxUtil.kObjectKindNone;
			}
			else if (file == Phx.HPBarData.kXmlFileInfo)
			{
				db = this.Database.HPBars.ObjectDatabase;
				specificObjectKind = PhxUtil.kObjectKindNone;
			}
			else if (file == Phx.BAbility.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Ability;
			}
			else if (file == Phx.BCiv.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Civ;
			}
			else if (file == Phx.BDamageType.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.DamageType;
			}
			else if (file == Phx.BProtoImpactEffect.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.ImpactEffect;
			}
			else if (file == Phx.BLeader.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Leader;
			}
			else if (
				file == Phx.BProtoObject.kXmlFileInfo ||
				file == Phx.BProtoObject.kXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Object;
			}
			else if (file == Phx.BDatabaseBase.kObjectTypesXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.ObjectType;
			}
			else if (file == Phx.BProtoPower.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Power;
			}
			else if (
				file == Phx.BProtoSquad.kXmlFileInfo ||
				file == Phx.BProtoSquad.kXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Squad;
			}
			else if (file.Directory == GameDirectory.Tactics)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Tactic;
			}
			else if (
				file == Phx.BProtoTech.kXmlFileInfo ||
				file == Phx.BProtoTech.kXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.Tech;
			}
			else if (file == Phx.TerrainTileType.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.TerrainTileType;
			}
			else if (file == Phx.BUserClass.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.UserClass;
			}
			else if (file == Phx.BWeaponType.kXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.WeaponType;
			}
			else if (
				file.Directory == GameDirectory.AbilityScripts ||
				file.Directory == GameDirectory.PowerScripts ||
				file.Directory == GameDirectory.TriggerScripts)
			{
				throw new NotImplementedException(file.ToString());
			}

			return new KeyValuePair<Phx.ProtoDataObjectDatabase, int>(db, specificObjectKind);
		}
	};

	public struct ObjectDatabaseForFileResult
	{
		public XmlFileInfo File { get; private set; }
		public Phx.ProtoDataObjectDatabase Database { get; private set; }
		public int SpecificObjectKind { get; private set; }

		public ObjectDatabaseForFileResult(XmlFileInfo file, Phx.ProtoDataObjectDatabase db, int objectKind)
		{
			this.File = file;
			this.Database = db;
			this.SpecificObjectKind = objectKind;
		}

		public bool IsNull { get { return this.Database == null; } }

		public static ObjectDatabaseForFileResult Null { get {
			return new ObjectDatabaseForFileResult(null, null, TypeExtensions.kNone);
		} }
	};
}