using fin.io;
using fin.io.bundles;
using fin.log;
using fin.util.progress;

using hpcos.api;
using hpcos.schema.geo;

using schema.binary;

namespace uni.games.hp_chamber_of_secrets;

/// <summary>
///   Harry Potter and the Chamber of Secrets GCN file bundle gatherer
/// </summary>
// https://wiki.dolphin-emu.org/index.php?title=GHSE69
[GameIDs("GHSY69", "GHSJ13", "GHSE69", "GHSP69", "GHSX69")]
public sealed class HPChamberOfSecretsFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  private static readonly ILogger LOGGER_
      = Logging.Create<HPChamberOfSecretsFileBundleGatherer>();

  public override string Name => "hp_chamber_of_secrets";
  public override string Title => "Harry Potter and the Chamber of Secrets";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    GeoUnplacedMeshCollector unplacedMeshes = new();
    foreach (IFileHierarchyDirectory directory in fileHierarchy) {
      foreach (IFileHierarchyFile geoFile in
               directory.FilesWithExtension(".geo")) {
        using IBinaryReader br
            = geoFile.OpenReadAsBinary(Endianness.BigEndian);
        if (GeoHeader.ReadVersion(br) != GeoHeader.SUPPORTED_VERSION) {
          continue;
        }

        GeoModelFileBundle bundle = GeoModelFileBundle.Create(geoFile);
        organizer.Add(bundle);

        // A GEO that can't be read is still listed, so it fails on its own
        // when opened rather than hiding every other bundle.
        try {
          unplacedMeshes.Add(bundle, br);
        } catch (Exception e) {
          LOGGER_.LogError(
              $"Failed to collect unplaced meshes from {geoFile.DisplayFullPath}: {e.Message}");
        }
      }
    }

    foreach (GeoModelFileBundle bundle in unplacedMeshes.GetBundles()) {
      organizer.Add(bundle);
    }
  }
}
