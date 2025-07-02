using fin.io;
using fin.io.bundles;
using fin.util.progress;

using visceral.api;

namespace uni.games.dead_space_1 {
  public class DeadSpace1FileBundleGatherer : BDesktopFileBundleGatherer {
    public override string Name => "dead_space_1";
    public override string SteamName => "Dead Space";
    public override string EpicName => "Dead Space";

    protected override void GatherFileBundlesImpl(
        IFileBundleOrganizer organizer,
        IMutablePercentageProgress mutablePercentageProgress,
        ISystemDirectory gameDir,
        ISystemDirectory cacheDir,
        ISystemDirectory extractedDir) {
      if (extractedDir.IsEmpty) {
        var strExtractor = new StrExtractor();
        foreach (var strFile in gameDir.GetFilesWithFileType(".str", true)) {
          strExtractor.Extract(strFile, extractedDir);
        }
      }

      var assetFileHierarchy
          = ExtractorUtil.GetFileHierarchy("dead_space_1", extractedDir);
      var bnkFileIdsDictionary = new BnkFileIdsDictionary(
          extractedDir,
          new FinFile(Path.Join(cacheDir.FullPath, "bnks.ids")));
      var mtlbFileIdsDictionary = new MtlbFileIdsDictionary(
          extractedDir,
          new FinFile(Path.Join(cacheDir.FullPath, "mtlbs.ids")));
      var tg4hFileIdDictionary = new Tg4hFileIdDictionary(
          extractedDir,
          new FinFile(Path.Join(cacheDir.FullPath, "tg4hs.ids")));

      foreach (var charSubdir in
               new[] { "animated_props", "chars", "weapons" }
                   .Select(f => assetFileHierarchy.Root.AssertGetExistingSubdir(
                               f))
                   .SelectMany(subdir => subdir.GetExistingSubdirs())) {
        IFileHierarchyFile[] geoFiles = [];
        if (charSubdir.TryToGetExistingSubdir("rigged/export",
                                              out var riggedSubdir)) {
          geoFiles =
              riggedSubdir.GetExistingFiles()
                          .Where(file => file.Name.EndsWith(".geo"))
                          .ToArray();
        }

        IFileHierarchyFile? rcbFile = null;
        IReadOnlyTreeFile[] bnkFiles = [];
        if (charSubdir.TryToGetExistingSubdir("cct/export",
                                              out var cctSubdir)) {
          rcbFile =
              cctSubdir.GetExistingFiles()
                       .Single(file => file.Name.EndsWith(".rcb.WIN"));
        }

        if (geoFiles.Length > 0 || rcbFile != null) {
          organizer.Add(new GeoModelFileBundle {
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