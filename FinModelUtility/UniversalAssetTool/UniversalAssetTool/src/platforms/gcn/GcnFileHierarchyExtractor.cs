using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using fin.archives;
using fin.common;
using fin.compression;
using fin.config;
using fin.io;
using fin.io.archive;
using fin.util.asserts;

using gx.archives.rarc;

using uni.games;
using uni.platforms.gcn.tools;

namespace uni.platforms.gcn;

public sealed class GcnFileHierarchyExtractor {
  private readonly RarcDump rarcDump_ = new();
  private readonly Yay0Dec yay0Dec_ = new();
  private readonly Yaz0Dec yaz0Dec_ = new();

  // List of supported file types for GameCube ROMs
  private static readonly string[] ROM_FILE_TYPES
      = [".ciso", ".nkit.iso", ".iso", ".gcm"];

  private const int CISO_HEADER_SIZE = 0x8000;
  private const int DISC_HEADER_SIZE = 0x20;
  private const int DISC_MAGIC_OFFSET = 0x1C;
  private static readonly byte[] GCN_DISC_MAGIC = [0xC2, 0x33, 0x9F, 0x3D];

  public bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy)
    => this.TryToExtractFromGame(gameName,
                                 Options.Standard(),
                                 out fileHierarchy);

  public bool TryToExtractFromGame(
      string gameName,
      Options options,
      out IFileHierarchy fileHierarchy)
    => this.TryToExtractFromGame(gameName,
                                 ImmutableHashSet<string>.Empty,
                                 options,
                                 out fileHierarchy);

  public bool TryToExtractFromGame(
      string gameName,
      IReadOnlySet<string> gameIDs,
      Options options,
      out IFileHierarchy fileHierarchy) {
    if (!TryToFindRom(gameName, gameIDs, out IReadOnlyTreeFile? romFile)) {
      fileHierarchy = null;
      return false;
    }

    fileHierarchy = this.ExtractFromRom_(gameName, romFile, options);
    return true;
  }

  public static bool HasRomOrExtractedDirectory(
      string gameName,
      IReadOnlySet<string> gameIDs)
    => ExtractorUtil.HasBeenExtracted(gameName) ||
       TryToFindRom(gameName, gameIDs, out _);

  public static bool TryToFindRom(
      string gameName,
      IReadOnlySet<string> gameIDs,
      [NotNullWhen(true)] out IReadOnlyTreeFile? romFile) {
    // First look for a ROM with the expected game name
    if (DirectoryConstants.ROMS_DIRECTORY.TryToGetExistingFileWithFileType(
            gameName,
            out ISystemFile? namedRomFile,
            ROM_FILE_TYPES)) {
      romFile = namedRomFile;
      return true;
    }

    // Fall back to the first ROM with one of the expected game IDs, in
    // case it wasn't renamed properly.
    romFile = gameIDs.Count == 0
        ? null
        : DirectoryConstants.ROMS_DIRECTORY
                            .GetExistingFiles()
                            .WithFileTypes(ROM_FILE_TYPES)
                            .FirstOrDefault(
                                f => TryToReadGameId_(f, out string? gameID) &&
                                     gameIDs.Contains(gameID));
    return romFile != null;
  }

  // Reads the 6-character game ID from the ROM's disc header. This is the 
  // same ID that Dolphin shows if you right-click -> Properties.
  private static bool TryToReadGameId_(
      IReadOnlySystemFile romFile,
      [NotNullWhen(true)] out string? gameID) {
    gameID = null;

    try {
      using Stream stream = romFile.OpenRead();
      Span<byte> magic = stackalloc byte[4];
      if (stream.ReadAtLeast(magic, magic.Length, false) < magic.Length) {
        // File isn't big enough
        return false;
      }

      long headerOffset = magic.SequenceEqual("CISO"u8) ? CISO_HEADER_SIZE : 0;
      if (stream.Length < headerOffset + DISC_HEADER_SIZE) {
        // File isn't big enough
        return false;
      }

      stream.Position = headerOffset;
      Span<byte> header = stackalloc byte[DISC_HEADER_SIZE];
      stream.ReadExactly(header);

      if (!header.Slice(DISC_MAGIC_OFFSET, GCN_DISC_MAGIC.Length)
                 .SequenceEqual(GCN_DISC_MAGIC)) {
        // File isn't a GameCube ROM
        return false;
      }

      gameID = Encoding.ASCII.GetString(header[..6]);
      return true;
    } catch (IOException) {
      return false;
    } catch (UnauthorizedAccessException) {
      return false;
    }
  }

  public IFileHierarchy ExtractFromRom_(
      string gameName,
      IReadOnlyTreeFile romFile,
      Options options) {
    // Uses the game name rather than the ROM's file name, since the ROM may
    // have been found via its game ID instead.
    ISystemDirectory directory = ExtractorUtil.GetOrCreateExtractedDirectory(gameName);
    if (new GcmArchiveImporter().ExtractInto(
            new GcmArchiveFileBundle(romFile),
            directory) ==
        ArchiveExtractionResult.FAILED) {
      Asserts.Fail($"Failed to extract files from {romFile}!");
    }

    var fileHierarchy
        = ExtractorUtil.GetFileHierarchy(gameName, directory);
    var hasChanged = false;

    // Decompresses all of the archives,
    foreach (var subdir in fileHierarchy) {
      var didDecompress = false;

      // Decompresses files
      foreach (var file in subdir.FilesWithExtensions(
                   options.Yay0DecExtensions)) {
        didDecompress |=
            this.yay0Dec_.Run(file,
                              file.Impl.CloneWithFileType(".rarc"),
                              options.ContainerCleanupEnabled);
      }

      foreach (var file in subdir.FilesWithExtensions(
                   options.Yaz0DecExtensions)) {
        didDecompress |=
            this.yaz0Dec_.Run(file, options.ContainerCleanupEnabled);
      }

      // Updates to see any new decompressed files.
      if (didDecompress) {
        hasChanged = true;
        subdir.Refresh();
      }

      var didDump = false;

      // Dumps any ARC/RARC files.
      var arcFiles =
          subdir.GetExistingFiles()
                .Where(file => options.RarcDumpExtensions.Contains(
                           file.FileType))
                .ToArray();
      foreach (var arcFile in arcFiles) {
        didDump |=
            this.rarcDump_.Run(arcFile,
                               options.ContainerCleanupEnabled,
                               options.RarcDumpPruneNames);
      }

      // Updates to see any new dumped directories.
      if (didDump) {
        hasChanged = true;
        subdir.Refresh();
      }
    }

    if (hasChanged) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    return fileHierarchy;
  }

  public sealed class Options {
    private readonly HashSet<string> rarcDumpExtensions_ = [];
    private readonly HashSet<string> rarcDumpPruneNames_ = [];
    private readonly HashSet<string> yay0DecExtensions_ = [];
    private readonly HashSet<string> yaz0DecExtensions_ = [];

    private Options() {
      this.RarcDumpExtensions = this.rarcDumpExtensions_;
      this.RarcDumpPruneNames = this.rarcDumpPruneNames_;
      this.Yay0DecExtensions = this.yay0DecExtensions_;
      this.Yaz0DecExtensions = this.yaz0DecExtensions_;
    }

    public static Options Empty() => new();

    public static Options Standard()
      => new Options().UseYaz0DecForExtensions(".szs")
                      .UseRarcDumpForExtensions(".rarc")
                      .EnableContainerCleanup(FinConfig.CleanUpArchives);

    public IReadOnlySet<string> RarcDumpExtensions { get; }
    public IReadOnlySet<string> RarcDumpPruneNames { get; }
    public IReadOnlySet<string> Yaz0DecExtensions { get; }
    public IReadOnlySet<string> Yay0DecExtensions { get; }
    public bool ContainerCleanupEnabled { get; private set; }

    public Options UseRarcDumpForExtensions(
        string first,
        params string[] rest) {
      this.rarcDumpExtensions_.Add(first);
      foreach (var o in rest) {
        this.rarcDumpExtensions_.Add(o);
      }

      return this;
    }

    public Options PruneRarcDumpNames(
        string first,
        params string[] rest) {
      this.rarcDumpPruneNames_.Add(first);
      foreach (var o in rest) {
        this.rarcDumpPruneNames_.Add(o);
      }

      return this;
    }

    public Options UseYay0DecForExtensions(
        string first,
        params string[] rest) {
      this.yay0DecExtensions_.Add(first);
      foreach (var o in rest) {
        this.yay0DecExtensions_.Add(o);
      }

      return this;
    }

    public Options UseYaz0DecForExtensions(
        string first,
        params string[] rest) {
      this.yaz0DecExtensions_.Add(first);
      foreach (var o in rest) {
        this.yaz0DecExtensions_.Add(o);
      }

      return this;
    }

    public Options EnableContainerCleanup(bool enabled) {
      this.ContainerCleanupEnabled = enabled;
      return this;
    }
  }
}