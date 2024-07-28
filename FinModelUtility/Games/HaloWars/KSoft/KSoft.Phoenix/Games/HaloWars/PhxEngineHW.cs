
namespace KSoft.Phoenix.Engine
{
	partial class PhxEngine
	{
		void InitializeEngine(GameDirectories dirs)
		{
			this.Directories = dirs;
			this.Database = new HaloWars.BDatabase(this);
			this.TriggerDb = new Phx.TriggerDatabase();

			this.Database.InitializeTriggerScriptSerializer();
		}
		public static PhxEngine CreateForHaloWars(string gameRoot, string updateRoot
			, bool targets360 = false)
		{
			var e = new PhxEngine();
			e.Build = PhxEngineBuild.Release;
			e.TargetsXbox360 = targets360;
			e.InitializeEngine(new GameDirectories(gameRoot, updateRoot));

			return e;
		}
		public static PhxEngine CreateForHaloWarsAlpha(string gameRoot)
		{
			var e = new PhxEngine();
			e.Build = PhxEngineBuild.Alpha;
			e.TargetsXbox360 = true;
			e.InitializeEngine(new GameDirectories(gameRoot));

			return e;
		}
	};
}