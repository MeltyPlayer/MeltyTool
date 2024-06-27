using fin.audio.io.importers.ogg;
using fin.io;
using fin.io.bundles;

using glo.api;

using uni.platforms.desktop;

namespace uni.games.glover;

public class GloverFileBundleGatherer : IAnnotatedFileBundleGatherer {
  public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!SteamUtils.TryGetGameDirectory("Glover",
                                          out var gloverSteamDirectory)) {
        yield break;
      }

      var gloverFileHierarchy
          = FileHierarchy.From("glover", gloverSteamDirectory);

      var dataDirectory =
          gloverFileHierarchy.Root.AssertGetExistingSubdir("data");
      var topLevelBgmDirectory = dataDirectory.AssertGetExistingSubdir("bgm");
      foreach (var bgmFile in topLevelBgmDirectory.GetExistingFiles()) {
        yield return new OggAudioFileBundle(bgmFile).Annotate(bgmFile);
      }

      var topLevelObjectDirectory =
          dataDirectory.AssertGetExistingSubdir("objects");
      foreach (var objectDirectory in
               topLevelObjectDirectory.GetExistingSubdirs()) {
        foreach (var fileBundle in this.AddObjectDirectory_(
                     gloverFileHierarchy,
                     objectDirectory)) {
          yield return fileBundle;
        }
      }
    }

  private IEnumerable<IAnnotatedFileBundle> AddObjectDirectory_(
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
          gloverSteamDirectory.AssertGetExistingSubdir("data/textures/ootw/chars"),
          gloverSteamDirectory.AssertGetExistingSubdir("data/textures/ootw/notused"),
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
        yield return new GloModelFileBundle(objectFile, textureDirectories)
            .Annotate(objectFile);
      }
    }
}