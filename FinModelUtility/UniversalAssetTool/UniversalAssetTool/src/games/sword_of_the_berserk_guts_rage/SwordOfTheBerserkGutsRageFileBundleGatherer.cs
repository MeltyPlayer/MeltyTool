using berserkDc.api;

using fin.archives;
using fin.io.archive;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.sonic_adventure_dx;

public sealed class SwordOfTheBerserkGutsRageFileBundleGatherer
    : INamedFileBundleGatherer {
  public string Name => "sword_of_the_berserk_guts_rage";
  public string Title => "Sword of the Berserk: Guts' Rage";

  public FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DREAMCAST;

  public bool IsAvailable => ExtractorUtil.HasBeenExtracted(this.Name);

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    if (!this.IsAvailable) {
      return;
    }

    var extractedDir = ExtractorUtil.GetOrCreateExtractedDirectory(this.Name);
    var fileHierarchy = ExtractorUtil.GetFileHierarchy(this.Name, extractedDir);

    var didExtractAnything = false;
    foreach (
        var bmlFile in
        fileHierarchy.Root.FilesWithExtensionRecursive(".bml")) {
      var result = new BmlArchiveImporter().ExtractIntoAndMaybeCleanUp(
          new BmlArchiveFileBundle(bmlFile),
          bmlFile.Parent.Impl.GetOrCreateSubdir(bmlFile.NameWithoutExtension));
      didExtractAnything |= result == ArchiveExtractionResult.NEWLY_EXTRACTED;
    }

    if (didExtractAnything) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    foreach (var pacFile in
             fileHierarchy.Root.FilesWithExtensionRecursive(".pac")) {
      organizer.Add(new PacModelFileBundle(pacFile.Impl));
    }
  }
}