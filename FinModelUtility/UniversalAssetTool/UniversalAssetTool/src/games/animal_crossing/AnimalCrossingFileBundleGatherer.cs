using ac.api;

using fin.config;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using uni.platforms.gcn;
using uni.platforms.gcn.tools;

namespace uni.games.animal_crossing;

public sealed class AnimalCrossingFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "animal_crossing";

  public override bool IsListed => false;

  public override GcnFileHierarchyExtractor.Options Options
    => GcnFileHierarchyExtractor
       .Options
       .Standard()
       .UseRarcDumpForExtensions(".arc");

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    var forestaDir = root.Impl.GetOrCreateSubdir("foresta");
    if (forestaDir.IsEmpty &&
        root.TryToGetExistingFile("foresta.rarc", out var forestaRarc) &&
        root.TryToGetExistingFile("foresta.map", out var forestaMap)) {
      var relDump = new RelDump();
      relDump.Run(forestaRarc,
                  forestaMap,
                  FinConfig.CleanUpArchives);
      fileHierarchy.RefreshRootAndUpdateCache();
    }

    var dataObjectDirectory
        = root.AssertGetExistingSubdir("foresta/.data/dataobject.obj");

    var modelFile = dataObjectDirectory.AssertGetExistingFile(
            "int_yos_wheel_obj_model.bin");
    var vertexFile = dataObjectDirectory.AssertGetExistingFile(
            "int_yos_wheel_v.bin");

    organizer.Add(new AnimalCrossingModelFileBundle {
        ModelFile = modelFile.Impl,
        VertexFile = vertexFile.Impl,
    });
  }
}