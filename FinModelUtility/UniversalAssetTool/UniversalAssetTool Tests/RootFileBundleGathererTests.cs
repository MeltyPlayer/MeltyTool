using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using fin.io;
using fin.util.progress;

using uni.games;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace uni {
  public class RootFileBundleGathererTests {
    [TearDown]
    public void Setup() {
      FinFileSystem.FileSystem = new FileSystem();
    }

    private const string CONFIG_JSON_ = @"
{
  ""Exporter"": {
    ""ExportedFormats"": [
      "".fbx"",
      "".glb""
    ],
    ""ExportAllTextures"": true,
    ""ExportedModelScaleSource"": ""GAME_CONFIG""
  },
  ""Extractor"": {
    ""UseMultithreadingToExtractRoms"": true
  },
  ""Viewer"": {
    ""AutomaticallyPlayGameAudioForModel"": false,
    ""RotateLight"": false,
    ""ShowGrid"": true,
    ""ShowSkeleton"": false,
    ""ViewerModelScaleSource"": ""MIN_MAX_BOUNDS""
  },
  ""ThirdParty"": {
    ""ExportBoneScaleAnimationsSeparately"": false
  },
  ""Debug"": {
    ""VerboseConsole"": false
  }
}";

    [Test]
    public void TestEmptyFromVisualStudio() {
      {
        var mockFileSystem = new MockFileSystem();

        mockFileSystem.AddDirectory("FinModelUtility");
        mockFileSystem.AddDirectory("cli/config");
        mockFileSystem.AddDirectory("cli/out");
        mockFileSystem.AddDirectory("cli/roms");
        mockFileSystem.AddDirectory("cli/tools/dll");
        mockFileSystem.AddDirectory("cli/tools/universal_asset_tool");
        mockFileSystem.AddFile("cli/config.json", new MockFileData(CONFIG_JSON_));

        mockFileSystem.Directory.SetCurrentDirectory("FinModelUtility/some/game");

        FinFileSystem.FileSystem = mockFileSystem;
      }

      var percentageProgress = new PercentageProgress();
      var root
          = new RootFileBundleGatherer().GatherAllFiles(percentageProgress);
      Assert.AreEqual(0, root.Subdirs.Count);
      Assert.AreEqual(0, root.FileBundles.Count);
    }

    [Test]
    public void TestEmptyFromBundled() {
      {
        var mockFileSystem = new MockFileSystem();

        mockFileSystem.AddDirectory("cli/config");
        mockFileSystem.AddDirectory("cli/out");
        mockFileSystem.AddDirectory("cli/roms");
        mockFileSystem.AddDirectory("cli/tools/dll");
        mockFileSystem.AddDirectory("cli/tools/universal_asset_tool");
        mockFileSystem.AddFile("cli/config.json", new MockFileData(CONFIG_JSON_));

        mockFileSystem.Directory.SetCurrentDirectory("cli/tools/universal_asset_tool");

        FinFileSystem.FileSystem = mockFileSystem;
      }

      var percentageProgress = new PercentageProgress();
      var root
          = new RootFileBundleGatherer().GatherAllFiles(percentageProgress);
      Assert.AreEqual(0, root.Subdirs.Count);
      Assert.AreEqual(0, root.FileBundles.Count);
    }

    [Test]
    public void TestEmptyFromGithub() {
      {
        var mockFileSystem = new MockFileSystem();

        mockFileSystem.AddDirectory("config");
        mockFileSystem.AddDirectory("out");
        mockFileSystem.AddDirectory("roms");
        mockFileSystem.AddDirectory("tools/dll");
        mockFileSystem.AddDirectory("tools/universal_asset_tool");
        mockFileSystem.AddFile("config.json", new MockFileData(CONFIG_JSON_));

        mockFileSystem.Directory.SetCurrentDirectory("tools/universal_asset_tool");

        FinFileSystem.FileSystem = mockFileSystem;
      }

      var percentageProgress = new PercentageProgress();
      var root
          = new RootFileBundleGatherer().GatherAllFiles(percentageProgress);
      Assert.AreEqual(0, root.Subdirs.Count);
      Assert.AreEqual(0, root.FileBundles.Count);
    }
  }
}