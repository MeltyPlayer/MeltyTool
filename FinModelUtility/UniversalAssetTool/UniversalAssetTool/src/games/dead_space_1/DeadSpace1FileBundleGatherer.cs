using fin.io;
using fin.io.bundles;

using uni.platforms.desktop;

using visceral.api;

namespace uni.games.dead_space_1 {
  public class DeadSpace1FileBundleGatherer : IAnnotatedFileBundleGatherer {
    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles() {
      if (!SteamUtils.TryGetGameDirectory("Dead Space", out var deadSpaceDir)) {
        yield break;
      }

      ExtractorUtil.GetOrCreateRomDirectories(
          "dead_space_1",
          out var prereqsDir,
          out var extractedDir);
      if (extractedDir.IsEmpty) {
        var strExtractor = new StrExtractor();
        foreach (var strFile in
                 deadSpaceDir.GetFilesWithFileType(".str", true)) {
          strExtractor.Extract(strFile, extractedDir);
        }
      }

      var assetFileHierarchy
          = FileHierarchy.From("dead_space_1", extractedDir);
      var mtlbFileIdsDictionary = new MtlbFileIdsDictionary(
          extractedDir,
          new FinFile(Path.Join(prereqsDir.FullPath, "mtlbs.ids")));

      foreach (var charSubdir in
               new[] { "animated_props", "chars", "weapons" }
                   .Select(assetFileHierarchy.Root.AssertGetExistingSubdir)
                   .SelectMany(subdir => subdir.GetExistingSubdirs())) {
        IFileHierarchyFile[] geoFiles = Array.Empty<IFileHierarchyFile>();
        if (charSubdir.TryToGetExistingSubdir("rigged/export",
                                              out var riggedSubdir)) {
          geoFiles =
              riggedSubdir.GetExistingFiles()
                          .Where(file => file.Name.EndsWith(".geo"))
                          .ToArray();
        }

        IFileHierarchyFile? rcbFile = null;
        IReadOnlyTreeFile[] bnkFiles = Array.Empty<IReadOnlyTreeFile>();
        if (charSubdir.TryToGetExistingSubdir("cct/export",
                                              out var cctSubdir)) {
          rcbFile =
              cctSubdir.GetExistingFiles()
                       .Single(file => file.Name.EndsWith(".rcb.WIN"));
          bnkFiles =
              cctSubdir.GetExistingFiles()
                       .Where(file => file.Name.EndsWith(".bnk.WIN"))
                       .ToArray();
        }

        Tg4ImageFileBundle[]? textureFiles = null;
        if (charSubdir.TryToGetExistingSubdir("rigged/textures",
                                              out var textureDir)) {
          var textureDirFiles = textureDir.GetExistingFiles().ToArray();
          var tg4hFiles =
              textureDirFiles.Where(file => file.FileType == ".tg4h")
                             .ToDictionary(file => file.NameWithoutExtension);
          textureFiles =
              textureDirFiles.Where(file => file.FileType == ".tg4d")
                             .Select(tg4dFile => new Tg4ImageFileBundle {
                                 Tg4dFile = tg4dFile,
                                 Tg4hFile =
                                     tg4hFiles[
                                         tg4dFile.NameWithoutExtension]
                             })
                             .ToArray();
        }

        if (geoFiles.Length > 0 || rcbFile != null) {
          yield return new GeoModelFileBundle {
              GameName = "dead_space_1",
              GeoFiles = geoFiles,
              BnkFiles = bnkFiles,
              RcbFile = rcbFile,
              MtlbFileIdsDictionary = mtlbFileIdsDictionary,
              Tg4ImageFileBundles = textureFiles
          }.Annotate(geoFiles.FirstOrDefault() ?? rcbFile!);
        } else {
          ;
        }
      }

      /*return assetFileHierarchy
       .SelectMany(dir => dir.Files.Where(file => file.Name.EndsWith(".rcb.WIN")))
       .Select(
           rcbFile => new GeoModelFileBundle { RcbFile = rcbFile });*/
    }
  }
}