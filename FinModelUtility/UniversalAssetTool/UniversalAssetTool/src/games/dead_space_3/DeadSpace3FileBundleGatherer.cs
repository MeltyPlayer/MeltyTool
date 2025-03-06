using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.desktop;

using visceral.api;

namespace uni.games.dead_space_3;

public class DeadSpace3FileBundleGatherer : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    ISystemDirectory deadSpace3Dir;
    if (!(SteamUtils.TryGetGameDirectory("Dead Space 3", out deadSpace3Dir) ||
          EaUtils.TryGetGameDirectory("Dead Space 3", out deadSpace3Dir))) {
      return;
    }

    ExtractorUtil.GetOrCreateRomDirectoriesWithPrereqsAndCache(
        "dead_space_3",
        out var prereqsDir,
        out var cacheDir,
        out var extractedDir);
    if (extractedDir.IsEmpty) {
      var bighExtractor = new BighExtractor();
      foreach (var vivFile in
               deadSpace3Dir.GetFilesWithFileType(".viv", true)) {
        var filelistFile = prereqsDir.AssertGetExistingFile(
            $"{vivFile.NameWithoutExtension}.filelist");

        bighExtractor.Extract(vivFile, filelistFile, extractedDir);
      }

      var strExtractor = new StrExtractor();
      foreach (var strFile in
               extractedDir.GetFilesWithFileType(".str", true)) {
        strExtractor.ExtractAndDelete(strFile, extractedDir);
      }
    }

    var assetFileHierarchy
        = ExtractorUtil.GetFileHierarchy("dead_space_3", extractedDir);
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
                 .Select(f => assetFileHierarchy.Root.AssertGetExistingSubdir(f))
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
      if (charSubdir.TryToGetExistingSubdir("cct/export",
                                            out var cctSubdir)) {
        rcbFile =
            cctSubdir.GetExistingFiles()
                     .Single(file => file.Name.EndsWith(".rcb.win"));
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
  }
}