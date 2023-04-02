using fin.io.bundles;

using mod.cli;

using uni.platforms;
using uni.platforms.gcn;


namespace uni.games.pikmin_1 {
  public class Pikmin1ModelFileGatherer
      : IFileBundleGatherer<ModModelFileBundle> {
    public IEnumerable<ModModelFileBundle> GatherFileBundles(bool assert) {
      if (!DirectoryConstants.ROMS_DIRECTORY.PossiblyAssertExistingFile(
              "pikmin_1.gcm",
              assert,
              out var pikmin1Rom)) {
        return Enumerable.Empty<ModModelFileBundle>();
      }

      var options = GcnFileHierarchyExtractor.Options.Empty();
      var fileHierarchy =
          new GcnFileHierarchyExtractor().ExtractFromRom(options, pikmin1Rom);

      return fileHierarchy.SelectMany(directory => {
        // TODO: Handle special cases:
        // - olimar
        // - pikmin
        // - frog

        var anmFiles = directory.FilesWithExtension(".anm").ToArray();
        return directory
               .FilesWithExtension(".mod")
               .Select(modFile => {
                 var anmFile = anmFiles
                     .FirstOrDefault(
                         anmFile => anmFile.NameWithoutExtension ==
                                    modFile.NameWithoutExtension);
                 return new ModModelFileBundle {
                     GameName = "pikmin_1",
                     ModFile = modFile,
                     AnmFile = anmFile,
                 };
               });
      });
    }
  }
}