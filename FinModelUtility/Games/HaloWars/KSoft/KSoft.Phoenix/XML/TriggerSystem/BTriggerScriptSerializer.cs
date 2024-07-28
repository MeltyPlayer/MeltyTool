#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	sealed class BTriggerScriptSerializer : BXmlSerializerInterface
	{
		static Engine.XmlFileInfo GetFileInfo(FA mode, Phx.BTriggerScriptType type, string filename = null)
		{
			string root_name = Phx.BTriggerSystem.kXmlRootName;
			Engine.GameDirectory dir;
			var location = Engine.ContentStorage.Game;

			switch (type)
			{
			case Phx.BTriggerScriptType.TriggerScript:
				dir = Engine.GameDirectory.TriggerScripts;
				location = Engine.ContentStorage.UpdateOrGame; // TUs have only included updated TS files only
				break;
			case Phx.BTriggerScriptType.Scenario:
				dir = Engine.GameDirectory.Scenario;
				break;
			case Phx.BTriggerScriptType.Ability:
				dir = Engine.GameDirectory.AbilityScripts;
				break;
			case Phx.BTriggerScriptType.Power:
				dir = Engine.GameDirectory.PowerScripts;
				break;

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}

			return new Engine.XmlFileInfo()
			{
				Location = location,
				Directory = dir,

				RootName = root_name,
				FileName = filename,

				Writable = mode == FA.Write,
			};
		}

		Phx.BDatabaseBase mDatabase;
		internal override Phx.BDatabaseBase Database { get { return this.mDatabase; } }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		public Phx.BScenario Scenario { get; private set; }

		public BTriggerScriptSerializer(Engine.PhxEngine phx, Phx.BScenario scnr = null)
		{
			Contract.Requires(phx != null);

			this.mDatabase = phx.Database;
			this.TriggerDb = phx.TriggerDb;
			this.Scenario = scnr;
		}

		#region IDisposable Members
		public override void Dispose()
		{
		}
		#endregion

		public class StreamTriggerScriptContext
		{
			public Engine.XmlFileInfo FileInfo { get; set; }

			public Phx.BTriggerSystem Script { get; set; }

			public Phx.BTriggerSystem[] Scripts { get; set; }
		};
		public StreamTriggerScriptContext StreamTriggerScriptGetContext(FA mode, Phx.BTriggerScriptType type, string name)
		{
			return new StreamTriggerScriptContext
			{
				FileInfo = GetFileInfo(mode, type, name),
			};
		}
		public void StreamTriggerScript<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, StreamTriggerScriptContext ctxt)
			where TDoc : class
			where TCursor : class
		{
			s.SetSerializerInterface(this);

			var ts = ctxt.Script = new Phx.BTriggerSystem();
			ts.Serialize(s);
		}
		public void LoadScenarioScripts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, StreamTriggerScriptContext ctxt)
			where TDoc : class
			where TCursor : class
		{
			s.SetSerializerInterface(this);

			foreach (var e in s.ElementsByName(Phx.BTriggerSystem.kXmlRootName))
			{
				using (s.EnterCursorBookmark(e))
					new Phx.BTriggerSystem().Serialize(s);
			}
		}
	};
}