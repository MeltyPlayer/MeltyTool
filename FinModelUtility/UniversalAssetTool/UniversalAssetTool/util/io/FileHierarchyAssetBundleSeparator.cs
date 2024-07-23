using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.util.io;

public class FileHierarchyAssetBundleSeparator(
    IFileHierarchy fileHierarchy,
    Action<IFileHierarchyDirectory, IFileBundleOrganizer> handler)
    : IAnnotatedFileBundleGatherer {
  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    foreach (var directory in fileHierarchy) {
      handler(directory, organizer);
    }

    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}