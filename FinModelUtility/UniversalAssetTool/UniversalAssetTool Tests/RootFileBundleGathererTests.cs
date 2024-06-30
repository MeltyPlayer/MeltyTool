using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

using fin.io;
using fin.util.progress;

using uni.games;

namespace uni {
  public class RootFileBundleGathererTests {
    [TearDown]
    public void Setup() {
      FinFileSystem.FileSystem = new FileSystem();
    }

    private const string CONFIG_JSON_ = @"
{
  ""ExporterSettings"": {
    ""ExportedFormats"": [
      "".fbx"",
      "".glb""
    ],
    ""ExportAllTextures"": true,
    ""ExportedModelScaleSource"": ""GAME_CONFIG""
  },
  ""ExtractorSettings"": {
    ""UseMultithreadingToExtractRoms"": true
  },
  ""ViewerSettings"": {
    ""AutomaticallyPlayGameAudioForModel"": false,
    ""RotateLight"": false,
    ""ShowGrid"": true,
    ""ShowSkeleton"": false,
    ""ViewerModelScaleSource"": ""MIN_MAX_BOUNDS""
  },
  ""ThirdPartySettings"": {
    ""ExportBoneScaleAnimationsSeparately"": false
  },
  ""DebugSettings"": {
    ""VerboseConsole"": false
  }
}";

    [Test]
    public void TestEmpty() {
      {
        var mockFileSystem = new MockFileSystem();

        mockFileSystem.AddDirectory("cli");
        mockFileSystem.Directory.SetCurrentDirectory("cli");

        mockFileSystem.AddDirectory("config");
        mockFileSystem.AddDirectory("out");
        mockFileSystem.AddDirectory("roms");
        mockFileSystem.AddDirectory("tools");

        mockFileSystem.AddFile("config.json", new MockFileData(CONFIG_JSON_));

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