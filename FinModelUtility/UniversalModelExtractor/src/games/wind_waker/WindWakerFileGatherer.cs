using fin.io;
using fin.io.bundles;
using fin.log;
using fin.util.asserts;
using fin.util.data;
using fin.util.strings;

using j3d.exporter;

using uni.msg;
using uni.platforms;
using uni.platforms.gcn;
using uni.platforms.gcn.tools;

#pragma warning disable CS8604


namespace uni.games.wind_waker {
  public class WindWakerFileGatherer : IFileBundleGatherer<BmdModelFileBundle> {
    private readonly ILogger logger_ =
        Logging.Create<WindWakerFileGatherer>();

    public IEnumerable<BmdModelFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "wind_waker.gcm",
              assert,
              out var windWakerRom)) {
        return Enumerable.Empty<BmdModelFileBundle>();
      }

      var options = GcnFileHierarchyExtractor.Options.Standard();
      var fileHierarchy =
          new GcnFileHierarchyExtractor()
              .ExtractFromRom(options, windWakerRom);

      var objectDirectory = fileHierarchy.Root.GetExistingSubdir(@"res\Object");
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
                                  new HashSet<string>(new[] { "archive" }));
        }

        if (didDump) {
          objectDirectory.Refresh(true);
        }
      }

      return this.ExtractObjects_(objectDirectory);

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

    private IEnumerable<BmdModelFileBundle> ExtractObjects_(
        IFileHierarchyDirectory directory)
      => directory.Subdirs.SelectMany(this.ExtractObject_);

    private IEnumerable<BmdModelFileBundle> ExtractObject_(
        IFileHierarchyDirectory directory) {
      // TODO: What the heck is the difference between these directories?
      // Is there any besides the model type within?
      var bdlSubdir =
          directory.Subdirs.SingleOrDefault(subdir => subdir.Name == "bdl");
      var bdlmSubdir =
          directory.Subdirs.SingleOrDefault(subdir => subdir.Name == "bdlm");
      var bmdSubdir =
          directory.Subdirs.SingleOrDefault(subdir => subdir.Name == "bmd");
      var bmdcSubdir =
          directory.Subdirs.SingleOrDefault(subdir => subdir.Name == "bmdc");
      var bmdmSubdir =
          directory.Subdirs.SingleOrDefault(subdir => subdir.Name == "bmdm");

      var bmdOrBdlFiles = ListUtil.ReadonlyConcat(
          //bdlSubdir?.FilesWithExtension(".bdl").ToArray(),
          //bdlmSubdir?.FilesWithExtension(".bdl").ToArray(),
          bmdSubdir?.FilesWithExtension(".bmd").ToArray(),
          bmdcSubdir?.FilesWithExtension(".bmd").ToArray(),
          bmdmSubdir?.FilesWithExtension(".bmd").ToArray());

      var bckSubdir =
          directory.Subdirs.SingleOrDefault(
              subdir => subdir.Name == "bck" || subdir.Name == "bcks");
      var bckFiles = bckSubdir?.FilesWithExtension(".bck").ToList();

      if (bmdOrBdlFiles.Count == 1 ||
          (bckFiles == null && bmdOrBdlFiles.Count > 0)) {
        return this.ExtractModels_(bmdOrBdlFiles, bckFiles);
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

        return this.ExtractFilesByOrganizing_(bmdOrBdlFiles.ToArray(),
                                              bckFiles,
                                              organizeMethod);
      }

      return Enumerable.Empty<BmdModelFileBundle>();
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
        var prefix = StringUtil.UpTo(bmdFile.NameWithoutExtension, "_");
        return bckFiles.Where(file => file.Name.StartsWith(prefix)).ToArray();
      }
    }

    public class NameMatchOrganizeMethod : IOrganizeMethod {
      private string name_;

      public NameMatchOrganizeMethod(string name) {
        this.name_ = name.ToLower();
      }

      public IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
          IFileHierarchyFile bmdFile,
          IReadOnlyList<IFileHierarchyFile> bckFiles) {
        if (bmdFile.NameWithoutExtension.ToLower().Contains(this.name_)) {
          return bckFiles;
        }

        return Array.Empty<IFileHierarchyFile>();
      }
    }

    public class SuffixOrganizeMethod : IOrganizeMethod {
      private readonly int suffixLength_;

      public SuffixOrganizeMethod(int suffixLength) {
        this.suffixLength_ = suffixLength;
      }

      public IReadOnlyList<IFileHierarchyFile> GetBcksForBmd(
          IFileHierarchyFile bmdFile,
          IReadOnlyList<IFileHierarchyFile> bckFiles) {
        var suffix =
            bmdFile.NameWithoutExtension.Substring(
                bmdFile.NameWithoutExtension.Length -
                this.suffixLength_);

        return bckFiles.Where(file => file.Name.StartsWith(suffix)).ToArray();
      }
    }

    private IEnumerable<BmdModelFileBundle> ExtractFilesByOrganizing_(
        IReadOnlyList<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFileHierarchyFile> bckFiles,
        IOrganizeMethod organizeMethod) {
      if (organizeMethod is PrefixOrganizeMethod) {
        bmdFiles.OrderByDescending(
            file => StringUtil
                    .UpTo(file.NameWithoutExtension, "_")
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
        foreach (var bundle in this.ExtractModels_(new[] { bmdFile },
                                                   claimedBckFiles)) {
          yield return bundle;
        }
      }
    }

    private IEnumerable<BmdModelFileBundle> ExtractModels_(
        IReadOnlyList<IFileHierarchyFile> bmdFiles,
        IReadOnlyList<IFileHierarchyFile>? bcxFiles = null,
        IReadOnlyList<IFileHierarchyFile>? btiFiles = null
    ) {
      bcxFiles ??= new List<IFileHierarchyFile>();
      btiFiles ??= new List<IFileHierarchyFile>();

      foreach (var bmdFile in bmdFiles) {
        yield return new BmdModelFileBundle {
            GameName = "wind_waker",
            BmdFile = bmdFile,
            BcxFiles = bcxFiles,
            BtiFiles = btiFiles,
            FrameRate = 60
        };
      }
    }
  }
}