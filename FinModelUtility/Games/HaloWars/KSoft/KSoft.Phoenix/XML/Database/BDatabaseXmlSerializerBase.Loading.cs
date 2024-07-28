using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	using PhxEngineBuild = Engine.PhxEngineBuild;

	partial class BDatabaseXmlSerializerBase
	{
		public enum StreamXmlStage
		{
			Preload,
			Stream,
			StreamUpdates,

			kNumberOf
		};
		public delegate void StreamXmlCallback(IO.XmlElementStream s);
		public sealed class StreamXmlContextData
		{
			public Engine.ProtoDataXmlFileInfo ProtoFileInfo;
			public Engine.XmlFileInfo FileInfo { get { return this.ProtoFileInfo.FileInfo; } }
			public Engine.XmlFileInfo FileInfoWithUpdates { get { return this.ProtoFileInfo.FileInfoWithUpdates; } }
			public Action<IO.XmlElementStream> Preload;
			public Action<IO.XmlElementStream> Stream;
			public Action<IO.XmlElementStream> StreamUpdates;

			public StreamXmlContextData(Engine.ProtoDataXmlFileInfo protoFileInfo)
			{
				this.ProtoFileInfo = protoFileInfo;
			}
		};
		private List<StreamXmlContextData> mStreamXmlContexts;
		private void SetupStreamXmlContexts()
		{
			if (this.mStreamXmlContexts != null)
				return;

			// #NOTE place new DatabaseObjectKind code here

			this.mStreamXmlContexts = new List<StreamXmlContextData>()
			{
				#region Lists
				new StreamXmlContextData(Phx.LocStringTable.kProtoFileInfoEnglish)
				{
					Preload= this.PreloadStringTable,
				},
				new StreamXmlContextData(Phx.BDatabaseBase.kObjectTypesProtoFileInfo)
				{
					Preload= this.StreamXmlObjectTypes,
				},
				new StreamXmlContextData(Phx.BDamageType.kProtoFileInfo)
				{
					Preload= this.PreloadDamageTypes,
					Stream= this.StreamXmlDamageTypes,
				},
				new StreamXmlContextData(Phx.BProtoImpactEffect.kProtoFileInfo)
				{
					Preload= this.StreamXmlImpactEffects,
				},
				new StreamXmlContextData(Phx.TerrainTileType.kProtoFileInfo)
				{
					Preload= this.StreamXmlTerrainTileTypes,
				},
				new StreamXmlContextData(Phx.BWeaponType.kProtoFileInfo)
				{
					Preload= this.StreamXmlWeaponTypes,
				},
				new StreamXmlContextData(Phx.BUserClass.kProtoFileInfo)
				{
					Preload= this.StreamXmlUserClasses,
				},
				#endregion

				#region GameData
				new StreamXmlContextData(Phx.HPBarData.kProtoFileInfo)
				{
					Stream= this.StreamXmlHPBars,
				},
				new StreamXmlContextData(Phx.BGameData.kProtoFileInfo)
				{
					Stream= this.StreamXmlGameData,
				},
				new StreamXmlContextData(Phx.BAbility.kProtoFileInfo)
				{
					Stream= this.StreamXmlAbilities,
				},
				#endregion

				#region ProtoData
				new StreamXmlContextData(Phx.BProtoObject.kProtoFileInfo)
				{
					Preload= this.PreloadObjects,
					Stream= this.StreamXmlObjects,
					StreamUpdates= this.StreamXmlObjectsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoSquad.kProtoFileInfo)
				{
					Preload= this.PreloadSquads,
					Stream= this.StreamXmlSquads,
					StreamUpdates= this.StreamXmlSquadsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoPower.kProtoFileInfo)
				{
					Preload= this.PreloadPowers,
					Stream= this.StreamXmlPowers,
				},
				new StreamXmlContextData(Phx.BProtoTech.kProtoFileInfo)
				{
					Preload= this.PreloadTechs,
					Stream= this.StreamXmlTechs,
					StreamUpdates= this.StreamXmlTechsUpdate,
				},

				new StreamXmlContextData(Phx.BCiv.kProtoFileInfo)
				{
					//Preload=PreloadCivs,
					Stream= this.StreamXmlCivs,
				},
				new StreamXmlContextData(Phx.BLeader.kProtoFileInfo)
				{
					//Preload=PreloadLeaders,
					Stream= this.StreamXmlLeaders,
				},
				#endregion
			};
		}
		private void ProcessStreamXmlContexts(ref bool r, FA mode)
		{
			this.ProcessStreamXmlContexts(ref r, mode
			                              , StreamXmlStage.Preload, StreamXmlStage.kNumberOf
			                              , Engine.XmlFilePriority.Lists, Engine.XmlFilePriority.kNumberOf);
		}

		private struct ProcessStreamXmlContextStageArgs
		{
			public FA Mode;
			public StreamXmlStage Stage;
			public Engine.XmlFilePriority FirstPriority;
			public Engine.XmlFilePriority LastPriorityPlusOne;

			public List<Task<bool>> Tasks;
			public List<Exception> TaskExceptions;

			public ProcessStreamXmlContextStageArgs(FA mode, StreamXmlStage stage
				, Engine.XmlFilePriority firstPriority
				, Engine.XmlFilePriority lastPriorityPlusOne)
			{
				this.Mode = mode;
				this.Stage = stage;
				this.FirstPriority = firstPriority;
				this.LastPriorityPlusOne = lastPriorityPlusOne;

				this.Tasks = null;
				this.TaskExceptions = null;
				this.Tasks = new List<Task<bool>>();
				this.TaskExceptions = new List<Exception>();
			}

			public bool UpdateResultWithTaskResults(ref bool r)
			{
				PhxUtil.UpdateResultWithTaskResults(ref r, this.Tasks, this.TaskExceptions);

				return r;
			}

			public void ClearTaskData()
			{
				this.Tasks.Clear();
				this.TaskExceptions.Clear();
			}
		};

		private void ProcessStreamXmlContexts(ref bool r, FA mode
			, StreamXmlStage firstStage// = StreamXmlStage.Preload
			, StreamXmlStage lastStagePlusOne// = StreamXmlStage.kNumberOf
			, Engine.XmlFilePriority firstPriority = Engine.XmlFilePriority.Lists
			, Engine.XmlFilePriority lastPriorityPlusOne = Engine.XmlFilePriority.kNumberOf
			)
		{
			this.SetupStreamXmlContexts();

			for (var s = firstStage; s < lastStagePlusOne; s++)
			{
				var args = new ProcessStreamXmlContextStageArgs(mode, s, firstPriority, lastPriorityPlusOne);
				this.ProcessStreamXmlContextStage(ref r, args);
			}
		}

		private void ProcessStreamXmlContextStage(ref bool r, ProcessStreamXmlContextStageArgs args)
		{
			var mode = args.Mode;

			for (var p = args.FirstPriority; p < args.LastPriorityPlusOne; p++)
			{
				foreach (var ctxt in this.mStreamXmlContexts)
				{
					if (ctxt.ProtoFileInfo.Priority != p)
						continue;

					switch (args.Stage)
					{
						#region Preload
						case StreamXmlStage.Preload:
						{
							if (ctxt.Preload == null)
								break;

							var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfo, mode, ctxt.Preload));
							args.Tasks.Add(task);
						} break;
						#endregion
						#region Stream
						case StreamXmlStage.Stream:
						{
							if (ctxt.Stream == null)
								break;

								var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfo, mode, ctxt.Stream));
								args.Tasks.Add(task);
						} break;
						#endregion
						#region StreamUpdates
						case StreamXmlStage.StreamUpdates:
						{
							if (ctxt.FileInfoWithUpdates == null)
								break;
							if (ctxt.StreamUpdates == null)
								break;

							var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.StreamUpdates));
							args.Tasks.Add(task);
						} break;
						#endregion
					}
				}

				if (!args.UpdateResultWithTaskResults(ref r))
				{
					var inner = args.TaskExceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate();

					throw new InvalidOperationException(string.Format(
						"Failed to process one or more files for priority={0}",
						p),
						inner);
				}

				args.ClearTaskData();
			}
		}

		void StreamTacticsAsync(ref bool r, FA mode)
		{
			var tactics = this.Database.Tactics;
			var tasks = new List<Task<bool>>(tactics.Count);
			var task_exceptions = new List<Exception>(tactics.Count);

			foreach (var tactic in tactics)
			{
				if (mode == FA.Read)
				{
					if (tactic.SourceXmlFile != null)
						continue;

					tactic.SourceXmlFile = Phx.BTacticData.CreateFileInfo(mode, tactic.Name);
				}
				else if (mode == FA.Write)
				{
					Contract.Assert(tactic.SourceXmlFile != null, tactic.Name);
				}

				var engine = this.GameEngine;
				if (mode == FA.Read)
					engine.UpdateFileLoadStatus(tactic.SourceXmlFile, Engine.XmlFileLoadState.Loading);

				var arg = tactic;
				var task = Task<bool>.Factory.StartNew((state) =>
				{
					var _tactic = state as Phx.BTacticData;
					return this.TryStreamData(_tactic.SourceXmlFile, mode, this.StreamTactic, _tactic, Phx.BTacticData.kFileExt);
				}, arg);
				tasks.Add(task);
			}
			PhxUtil.UpdateResultWithTaskResults(ref r, tasks, task_exceptions);

			if (!r)
			{
				Debug.Trace.XML.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone,
					"Failed to " + mode + " tactics",
					task_exceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate());
			}
		}
		bool StreamTactics(FA mode)
		{
			if (this.GameEngine.Build == PhxEngineBuild.Alpha)
			{
				Debug.Trace.XML.TraceInformation("BDatabaseXmlSerializer: Alpha build detected, skipping Tactics streaming");
				return false;
			}

			bool r = true;
			this.StreamTacticsAsync(ref r, mode);
			return r;
		}

	private bool mIsPreloading;
		protected bool IsNotPreloading { get { return !this.mIsPreloading; } }
		public bool Preload()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not preloading Database because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState >= Phx.DatabaseLoadState.Preloading)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping preloading of Database because it already is at a later stage");
				return true;
			}

			const FA k_mode = FA.Read;

			this.mIsPreloading = true;
			this.Database.LoadState = Phx.DatabaseLoadState.Preloading;

			this.AutoIdSerializersInitialize();
			this.PreStreamXml(k_mode);

			bool r = true;
			this.ProcessStreamXmlContexts(ref r, k_mode
			                              , StreamXmlStage.Preload
			                              , StreamXmlStage.Stream);

			this.PostStreamXml(k_mode);
			//AutoIdSerializersDispose();

			this.mIsPreloading = false;
			this.Database.LoadState = r
				? Phx.DatabaseLoadState.Preloaded
				: Phx.DatabaseLoadState.Failed;

			return r;
		}

		public bool Load()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Database because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState >= Phx.DatabaseLoadState.Loading)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping loading of Database because it already is at a later stage");
				return true;
			}

			const FA k_mode = FA.Read;

			this.Database.LoadState = Phx.DatabaseLoadState.Loading;
			this.AutoIdSerializersInitialize();
			this.PreStreamXml(k_mode);

			bool r = true;
			this.ProcessStreamXmlContexts(ref r, k_mode
			                              , StreamXmlStage.Stream
			                              , StreamXmlStage.kNumberOf);

			this.PostStreamXml(k_mode);
			//AutoIdSerializersDispose();

			this.Database.LoadState = r
				? Phx.DatabaseLoadState.Loaded
				: Phx.DatabaseLoadState.Failed;

			return r;
		}

		public bool LoadAllTactics()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.Failed)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState < Phx.DatabaseLoadState.Preloaded)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier the database is not at least preloaded");
				return true;
			}

			const FA k_mode = FA.Read;

			this.PreStreamXml(k_mode);

			bool r = this.StreamTactics(k_mode);

			this.PostStreamXml(k_mode);

			if (!r)
				this.Database.LoadState = Phx.DatabaseLoadState.Failed;

			return r;
		}
	};
}