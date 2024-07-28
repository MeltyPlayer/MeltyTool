#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	sealed class TriggerDatabaseBuilderSerializer
		: BXmlSerializerInterface
	{
		Phx.BDatabaseBase mDatabase;
		internal override Phx.BDatabaseBase Database { get { return this.mDatabase; } }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		public TriggerDatabaseBuilderSerializer(Engine.PhxEngine phx)
		{
			Contract.Requires(phx != null);

			this.mDatabase = phx.Database;
			this.TriggerDb = phx.TriggerDb;
		}

		#region IDisposable Members
		public override void Dispose() {}
		#endregion

		void ParseTriggerScript<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			var ts = new Phx.BTriggerSystem();
			ts.Serialize(s);
		}
		void ParseTriggerScriptSansSkrimishAI<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// This HW script has all the debug info stripped :o
			if (s.StreamName.EndsWith("skirmishai.triggerscript"))
				return;

			this.ParseTriggerScript(s);
		}
		void ParseScenarioScripts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			foreach (var e in s.ElementsByName(Phx.BTriggerSystem.kXmlRootName))
			{
				using (s.EnterCursorBookmark(e))
					new Phx.BTriggerSystem().Serialize(s);
			}
		}

		void ParseTriggerScripts(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.TriggerScripts,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
			                        this.ParseTriggerScriptSansSkrimishAI, out result);

			this.ReadDataFilesAsync(Engine.ContentStorage.Update, Engine.GameDirectory.TriggerScripts,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TriggerScript),
			                        this.ParseTriggerScript, out result);
		}
		void ParseAbilities(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.AbilityScripts,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Ability),
			                        this.ParseTriggerScript, out result);
		}
		void ParsePowers(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.Game,   Engine.GameDirectory.PowerScripts,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.Power),
			                        this.ParseTriggerScript, out result);
		}
		void ParseScenarios(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.Game, Engine.GameDirectory.Scenario,
			                        "*.scn",
			                        this.ParseScenarioScripts, out result);
		}

		public void ParseScriptFiles()
		{
			var e = this.GameEngine;

			this.ParseTriggerScripts(e);
			this.ParseAbilities(e);
			this.ParsePowers(e);
			this.ParseScenarios(e);
		}
	};
}