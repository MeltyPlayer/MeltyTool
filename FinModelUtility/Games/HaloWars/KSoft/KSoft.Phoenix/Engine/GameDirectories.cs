using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public enum GetXmlOrXmbFileResult
	{
		FileNotFound,
		Xml,
		Xmb,

		kNumberOf
	};

	public class GameDirectories
	{
		#region Art
		const string kArtPath = @"art\";

		const string kParticleEffectPath = @"effects\";
		const string kSkyBoxPath = @"environment\sky\";
		const string kTerrainTexturesPath = @"terrain\";
		const string kFlashUIPath = @"ui\flash\";
		const string kMinimapPath = @"ui\flash\minimaps\";
		const string kLoadmapPath = @"ui\flash\pregame\textures\";
		const string kClipArtPath = @"clipart\";
		const string kRoadsPath = @"roads\";
		const string kFoliagePath = @"foliage\";
		#endregion
		#region Data
		const string kDataPath = @"data\";

		const string kAbilitiesPath = @"abilities\";
		const string kAIPath = @"ai\";
		const string kPowersPath = @"powers\";
		const string kTacticsPath = @"tactics\";
		const string kTriggerScriptsPath = @"triggerscripts\";
		#endregion
		const string kPhysicsPath = @"physics\";
		const string kScenariosPath = @"scenario\";
		const string kSoundPath = @"sound\";

	/*public*/
	string RootDirectory { get; /*private*/ set; }
		/*public*/ string UpdateDirectory { get; /*private*/ set; }
		bool UpdateDirectoryIsValid { get { return UpdateDirectory != null; } }
		public bool UseTitleUpdates { get; set; }

		public GameDirectories(string root, string updateRoot = null)
		{
			RootDirectory = root;
			UpdateDirectory = updateRoot;
			UseTitleUpdates = true;

			// Leave some breadcrumbs for the programmer in the event that they're confused as why an update file isn't loading.
			if (!UpdateDirectoryIsValid)
				Debug.Trace.Engine.TraceInformation("GameDirectories: No matching update directory for '{0}'", updateRoot);

			ArtPath = kArtPath;//Path.Combine(RootDirectory, kArtPath);
			ParticleEffectPath = Path.Combine(ArtPath, kParticleEffectPath);
			SkyBoxPath = Path.Combine(ArtPath, kSkyBoxPath);
			TerrainTexturesPath = Path.Combine(ArtPath, kTerrainTexturesPath);
			FlashUIPath = Path.Combine(ArtPath, kFlashUIPath);
			MinimapPath = Path.Combine(ArtPath, kMinimapPath);
			LoadmapPath = Path.Combine(ArtPath, kLoadmapPath);
			ClipArtPath = Path.Combine(ArtPath, kClipArtPath);
			RoadsPath = Path.Combine(ArtPath, kRoadsPath);
			FoliagePath = Path.Combine(ArtPath, kFoliagePath);

			DataPath = kDataPath;//Path.Combine(RootDirectory, kDataPath);
			AbilityScriptsPath = Path.Combine(DataPath, kAbilitiesPath);
			AIDataPath = Path.Combine(DataPath, kAIPath);
			PowerScriptsPath = Path.Combine(DataPath, kPowersPath);
			TacticsPath = Path.Combine(DataPath, kTacticsPath);
			TriggerScriptsPath = Path.Combine(DataPath, kTriggerScriptsPath);

			PhysicsPath = kPhysicsPath;
			ScenarioPath = kScenariosPath;
			SoundPath = kSoundPath;
		}

		#region Art
		public string ArtPath { get; protected set; }

		public string ParticleEffectPath { get; protected set; }
		public string SkyBoxPath { get; protected set; }
		public string TerrainTexturesPath { get; protected set; }
		public string FlashUIPath { get; protected set; }
		public string MinimapPath { get; protected set; }
		public string LoadmapPath { get; protected set; }
		public string ClipArtPath { get; protected set; }
		public string RoadsPath { get; protected set; }
		public string FoliagePath { get; protected set; }
		#endregion
		#region Data
		public string DataPath { get; protected set; }

		public string AbilityScriptsPath { get; protected set; }
		public string AIDataPath { get; protected set; }
		public string PowerScriptsPath { get; protected set; }
		public string TacticsPath { get; protected set; }
		public string TriggerScriptsPath { get; protected set; }
		#endregion
		public string PhysicsPath { get; protected set; }
		public string ScenarioPath { get; protected set; }
		public string SoundPath { get; protected set; }

		public string GetContentLocation(ContentStorage location)
		{
			switch (location)
			{
			case ContentStorage.Game: return RootDirectory;
			case ContentStorage.Update:
				return UpdateDirectoryIsValid ? UpdateDirectory : RootDirectory;

			default: throw new NotImplementedException();
			}
		}
		public string GetDirectory(GameDirectory dir)
		{
			switch (dir)
			{
			#region Art
			case GameDirectory.Art: return ArtPath;

			#endregion
			#region Data
			case GameDirectory.Data: return DataPath;

			case GameDirectory.AbilityScripts: return AbilityScriptsPath;
			case GameDirectory.AIData: return AIDataPath;
			case GameDirectory.PowerScripts: return PowerScriptsPath;
			case GameDirectory.Tactics: return TacticsPath;
			case GameDirectory.TriggerScripts: return TriggerScriptsPath;
			#endregion
			case GameDirectory.Physics: return PhysicsPath;
			case GameDirectory.Scenario: return ScenarioPath;
			case GameDirectory.Sound: return SoundPath;

			default: throw new NotImplementedException();
			}
		}
		public string GetAbsoluteDirectory(ContentStorage loc, GameDirectory gameDir)
		{
			string root = GetContentLocation(loc);
			string dir = GetDirectory(gameDir);
			return Path.Combine(root, dir);
		}

		bool TryGetFileImpl(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			string root = GetContentLocation(loc);
			string dir = GetDirectory(gameDir);
			string file_path = Path.Combine(root, dir, filename.ToLowerInvariant());
			if (!string.IsNullOrEmpty(ext))
				file_path += ext;

			return (file = new FileInfo(file_path)).Exists;
		}
		bool TryGetFileFromUpdateOrGame(GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			if (!UseTitleUpdates)
				return TryGetFileImpl(ContentStorage.Game, gameDir, filename, out file, ext);

			//////////////////////////////////////////////////////////////////////////
			// Try to get the file from the TU storage first
			string dir = GetDirectory(gameDir);
			string file_path = Path.Combine(dir, filename.ToLowerInvariant());
			if (!string.IsNullOrEmpty(ext))
				file_path += ext;

			string full_path;

			if (UpdateDirectoryIsValid)
			{
				full_path = Path.Combine(UpdateDirectory, file_path);
				file = new FileInfo(full_path);
			}

			//////////////////////////////////////////////////////////////////////////
			// No update file exists, fall back to regular game storage
			if (file == null || !file.Exists)
			{
				full_path = Path.Combine(RootDirectory, file_path);
				file = new FileInfo(full_path);
				return file.Exists;
			}
			return true;
		}
		public bool TryGetFile(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(filename));
			file = null;

			return loc == ContentStorage.UpdateOrGame
				? TryGetFileFromUpdateOrGame(gameDir, filename, out file, ext)
				: TryGetFileImpl(loc, gameDir, filename, out file, ext);
		}
		public GetXmlOrXmbFileResult TryGetXmlOrXmbFile(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(filename));
			file = null;

			if (TryGetFile(loc, gameDir, filename, out file, ext))
				return GetXmlOrXmbFileResult.Xml;

			if (ext.IsNotNullOrEmpty())
				filename += ext;

			filename += Xmb.XmbFile.kFileExt;

			// purposely don't pass ext through in the XMB round
			bool xmb_found = loc == ContentStorage.UpdateOrGame
				? TryGetFileFromUpdateOrGame(gameDir, filename, out file, ext: null)
				: TryGetFileImpl(loc, gameDir, filename, out file, ext: null);
			if (xmb_found)
				return GetXmlOrXmbFileResult.Xmb;

			return GetXmlOrXmbFileResult.FileNotFound;
		}

		public IEnumerable<string> GetFiles(ContentStorage loc, GameDirectory gameDir, string searchPattern)
		{
			Contract.Requires(loc != ContentStorage.UpdateOrGame, "Must iterate storages separately");
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));

			string dir = GetAbsoluteDirectory(loc, gameDir);

			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(dir);

			return Directory.EnumerateFiles(dir, searchPattern);
		}
	};
}