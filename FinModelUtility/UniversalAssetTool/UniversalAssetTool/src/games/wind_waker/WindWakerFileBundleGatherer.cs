using fin.io;
using fin.io.bundles;
using fin.log;
using fin.util.asserts;
using fin.util.lists;
using fin.util.progress;
using fin.util.strings;

using jsystem.api;

using uni.platforms.gcn;
using uni.platforms.gcn.tools;

#pragma warning disable CS8604


namespace uni.games.wind_waker;

public class WindWakerFileBundleGatherer : IAnnotatedFileBundleGatherer {
  private readonly ILogger logger_ =
      Logging.Create<WindWakerFileBundleGatherer>();

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!new GcnFileHierarchyExtractor().TryToExtractFromGame(
            "wind_waker",
            out var fileHierarchy)) {
      return;
    }

    var objectDirectory
        = fileHierarchy.Root.AssertGetExistingSubdir(@"res\Object");
    {
      var yaz0Dec = new Yaz0Dec();
      var didDecompress = false;
      foreach (var arcFile in objectDirectory.FilesWithExtension(".arc")) {
        didDecompress |= yaz0Dec.Run(arcFile, true);
      }

      if (didDecompress) {
        objectDirectory.Refresh();
      }

      var rarcDump = new RarcDump();
      var didDump = false;
      foreach (var rarcFile in objectDirectory.FilesWithExtension(".rarc")) {
        didDump |= rarcDump.Run(rarcFile,
                                true,
                                new HashSet<string>(["archive"]));
      }

      if (didDump) {
        objectDirectory.Refresh(true);
      }
    }

    this.ExtractObjects_(organizer, objectDirectory);

    /*{
      var relsDirectory = fileHierarchy.Root.GetExistingSubdir("rels");
      var mapFiles = fileHierarchy.Root.GetExistingSubdir("maps").Files;

      var yaz0Dec = new Yaz0Dec();
      var didDecompress = false;
      foreach (var relFile in relsDirectory.FilesWithExtension(".rel")) {
        didDecompress |= yaz0Dec.Run(relFile, false);
      }

      if (didDecompress) {
        relsDirectory.Refresh();
      }

      var didDump = false;
      var relDump = new RelDump();
      foreach (var rarcFile in relsDirectory.FilesWithExtension(".rarc")) {
        var mapFile = mapFiles.Single(
            file => file.NameWithoutExtension ==
                    rarcFile.NameWithoutExtension);

        didDump |= relDump.Run(rarcFile, mapFile, true);
      }

      if (didDump) {
        relsDirectory.Refresh(true);
      }
    }*/
  }

  private void ExtractObjects_(IFileBundleOrganizer organizer,
                               IFileHierarchyDirectory directory) {
    foreach (var subdir in directory.GetExistingSubdirs()) {
      this.ExtractObject_(organizer, subdir);
    }
  }

  private void ExtractObject_(IFileBundleOrganizer organizer,
                              IFileHierarchyDirectory directory) {
    // TODO: What the heck is the difference between these directories?
    // Is there any besides the model type within?
    var bdlSubdir =
        directory.GetExistingSubdirs()
                 .SingleOrDefault(subdir => subdir.Name == "bdl");
    var bdlmSubdir =
        directory.GetExistingSubdirs()
                 .SingleOrDefault(subdir => subdir.Name == "bdlm");
    var bmdSubdir =
        directory.GetExistingSubdirs()
                 .SingleOrDefault(subdir => subdir.Name == "bmd");
    var bmdcSubdir =
        directory.GetExistingSubdirs()
                 .SingleOrDefault(subdir => subdir.Name == "bmdc");
    var bmdmSubdir
        = directory.GetExistingSubdirs().SingleOrDefaultByName("bmdm");

    var bmdOrBdlFiles = ListUtil.ReadonlyConcat(
        //bdlSubdir?.FilesWithExtension(".bdl").ToArray(),
        //bdlmSubdir?.FilesWithExtension(".bdl").ToArray(),
        bmdSubdir?.FilesWithExtension(".bmd").ToArray(),
        bmdcSubdir?.FilesWithExtension(".bmd").ToArray(),
        bmdmSubdir?.FilesWithExtension(".bmd").ToArray());

    var bckSubdir =
        directory.GetExistingSubdirs()
                 .SingleOrDefault(
                     subdir => subdir.Name == "bck" || subdir.Name == "bcks");
    var bckFiles = bckSubdir?.FilesWithExtension(".bck").ToList();

    if (bmdOrBdlFiles.Count == 1 ||
        (bckFiles == null && bmdOrBdlFiles.Count > 0)) {
      this.ExtractModels_(organizer, bmdOrBdlFiles, bckFiles);
    } else if (bmdOrBdlFiles.Count > 0) {
      IOrganizeMethod organizeMethod;
      switch (directory.Name) {
        case "Sh": {
          organizeMethod = new SuffixOrganizeMethod(1);
          break;
        }
        case "Oq": {
          organizeMethod = new NameMatchOrganizeMethod(directory.Name);
          break;
        }
        case "Ylesr00": {
          organizeMethod = new PrefixOrganizeMethod();
          break;
        }
        default:
          throw new NotImplementedException();
      }

      this.ExtractFilesByOrganizing_(organizer,
                                     bmdOrBdlFiles.ToArray(),
                                     bckFiles,
                                     organizeMethod);
    }
  }

  public interface IOrganizeMethod {
    IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
        IFileHierarchyFile bmdFile,
        IReadOnlyList<IFileHierarchyFile> bckFiles);
  }

  public class PrefixOrganizeMethod : IOrganizeMethod {
    public IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
        IFileHierarchyFile bmdFile,
        IReadOnlyList<IFileHierarchyFile> bckFiles) {
      var prefix = bmdFile.NameWithoutExtension.SubstringUpTo("_");
      return bckFiles.Where(file => file.Name.StartsWith(prefix)).ToArray();
    }
  }

  public class NameMatchOrganizeMethod(string name) : IOrganizeMethod {
    private string name_ = name.ToLower();

    public IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
        IFileHierarchyFile bmdFile,
        IReadOnlyList<IFileHierarchyFile> bckFiles) {
      if (bmdFile.NameWithoutExtension.ToLower().Contains(this.name_)) {
        return bckFiles;
      }

      return Array.Empty<IFileHierarchyFile>();
    }
  }

  public class SuffixOrganizeMethod(int suffixLength) : IOrganizeMethod {
    public IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
        IFileHierarchyFile bmdFile,
        IReadOnlyList<IFileHierarchyFile> bckFiles) {
      var suffix =
          bmdFile.NameWithoutExtension.Substring(
              bmdFile.NameWithoutExtension.Length -
              suffixLength);

      return bckFiles.Where(file => file.Name.StartsWith(suffix)).ToArray();
    }
  }

  private void ExtractFilesByOrganizing_(
      IFileBundleOrganizer organizer,
      IReadOnlyList<IFileHierarchyFile> bmdFiles,
      IReadOnlyList<IFileHierarchyFile> bckFiles,
      IOrganizeMethod organizeMethod) {
    if (organizeMethod is PrefixOrganizeMethod) {
      bmdFiles.OrderByDescending(
          file => file.NameWithoutExtension
                      .SubstringUpTo("_")
                      .Length);
    }

    var unclaimedBckFiles = new HashSet<IFileHierarchyFile>(bckFiles);

    var bmdFileToBckFiles =
        new Dictionary<IFileHierarchyFile,
            IReadOnlyList<IFileHierarchyFile>>();
    foreach (var bmdFile in bmdFiles) {
      var claimedBckFiles = organizeMethod.GetBcksForBmd(bmdFile, bckFiles);
      bmdFileToBckFiles[bmdFile] = claimedBckFiles;

      foreach (var bckFile in claimedBckFiles) {
        unclaimedBckFiles.Remove(bckFile);
      }
    }

    Asserts.Equal(0, unclaimedBckFiles.Count);

    foreach (var (bmdFile, claimedBckFiles) in bmdFileToBckFiles) {
      this.ExtractModel_(organizer, bmdFile, claimedBckFiles);
    }
  }

  private void ExtractModels_(
      IFileBundleOrganizer organizer,
      IReadOnlyList<IFileHierarchyFile> bmdFiles,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
      IReadOnlyList<IFileHierarchyFile>? btiFiles = null
  ) {
    foreach (var bmdFile in bmdFiles) {
      this.ExtractModel_(organizer, bmdFile, bcxFiles, btiFiles);
    }
  }

  private void ExtractModel_(
      IFileBundleOrganizer organizer,
      IFileHierarchyFile bmdFile,
      IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
      IReadOnlyList<IFileHierarchyFile>? btiFiles = null
  ) {
    organizer.Add(new BmdModelFileBundle {
        GameName = "wind_waker",
        BmdFile = bmdFile,
        BcxFiles = bcxFiles,
        BtiFiles = btiFiles,
        FrameRate = 60
    }.Annotate(bmdFile));
  }
}