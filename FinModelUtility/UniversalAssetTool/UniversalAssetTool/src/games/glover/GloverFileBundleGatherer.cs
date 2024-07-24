using fin.audio.io.importers.ogg;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using glo.api;

using uni.platforms.desktop;

namespace uni.games.glover;

public class GloverFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!SteamUtils.TryGetGameDirectory("Glover",
                                        out var gloverSteamDirectory)) {
      return;
    }

    var gloverFileHierarchy
        = ExtractorUtil.GetFileHierarchy("glover", gloverSteamDirectory);

    var dataDirectory =
        gloverFileHierarchy.Root.AssertGetExistingSubdir("data");
    var topLevelBgmDirectory = dataDirectory.AssertGetExistingSubdir("bgm");
    foreach (var bgmFile in topLevelBgmDirectory.GetExistingFiles()) {
      organizer.Add(new OggAudioFileBundle(bgmFile).Annotate(bgmFile));
    }

    var topLevelObjectDirectory =
        dataDirectory.AssertGetExistingSubdir("objects");
    foreach (var objectDirectory in
             topLevelObjectDirectory.GetExistingSubdirs()) {
      this.AddObjectDirectory_(organizer, gloverFileHierarchy, objectDirectory);
    }
  }

  private void AddObjectDirectory_(IFileBundleOrganizer organizer,
                                   IFileHierarchy gloverFileHierarchy,
                                   IFileHierarchyDirectory objectDirectory) {
    var objectFiles = objectDirectory.FilesWithExtension(".glo");

    var gloverSteamDirectory = gloverFileHierarchy.Root;
    var textureDirectories = gloverSteamDirectory
                             .AssertGetExistingSubdir("data/textures/generic")
                             .GetExistingSubdirs()
                             .ToList();

    textureDirectories.AddRange([
        gloverSteamDirectory.AssertGetExistingSubdir("data/textures/hub"),
        gloverSteamDirectory.AssertGetExistingSubdir("data/textures/ootw"),
        gloverSteamDirectory.AssertGetExistingSubdir(
            "data/textures/ootw/chars"),
        gloverSteamDirectory.AssertGetExistingSubdir(
            "data/textures/ootw/notused"),
    ]);

    try {
      var levelTextureDirectory =
          gloverSteamDirectory.AssertGetExistingSubdir(
              objectDirectory.LocalPath.Replace("data\\objects",
                                                "data\\textures"));
      textureDirectories.Add(levelTextureDirectory);
      textureDirectories.AddRange(levelTextureDirectory.GetExistingSubdirs());
    } catch {
      // ignored
    }

    foreach (var objectFile in objectFiles) {
      organizer.Add(new GloModelFileBundle(objectFile, textureDirectories)
                        .Annotate(objectFile));
    }
  }
}