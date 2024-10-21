using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

using visceral.api;

namespace uni.games.dead_space_1 {
  public class DeadSpace1FileBundleGatherer : IAnnotatedFileBundleGatherer {
    public void GatherFileBundles(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress) {
      if (!SteamUtils.TryGetGameDirectory("Dead Space", out var deadSpaceDir)) {
        return;
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

      var assetFileHierarchy = ExtractorUtil.GetFileHierarchy("dead_space_1", extractedDir);
      var bnkFileIdsDictionary = new BnkFileIdsDictionary(
          extractedDir,
          new FinFile(Path.Join(prereqsDir.FullPath, "bnks.ids")));
      var mtlbFileIdsDictionary = new MtlbFileIdsDictionary(
          extractedDir,
          new FinFile(Path.Join(prereqsDir.FullPath, "mtlbs.ids")));
      var tg4hFileIdDictionary = new Tg4hFileIdDictionary(
          extractedDir,
          new FinFile(Path.Join(prereqsDir.FullPath, "tg4hs.ids")));

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
        }

        if (geoFiles.Length > 0 || rcbFile != null) {
          organizer.Add(new GeoModelFileBundle {
              GameName = "dead_space_1",
              GeoFiles = geoFiles,
              RcbFile = rcbFile,
              BnkFileIdsDictionary = bnkFileIdsDictionary,
              MtlbFileIdsDictionary = mtlbFileIdsDictionary,
              Tg4hFileIdDictionary = tg4hFileIdDictionary,
          }.Annotate(geoFiles.FirstOrDefault() ?? rcbFile!));
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