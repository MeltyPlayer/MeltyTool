using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.util.io;

public class FileHierarchyAssetBundleSeparator : IAnnotatedFileBundleGatherer {
  private readonly IFileHierarchy fileHierarchy_;

  private readonly Action<IFileHierarchyDirectory, IFileBundleOrganizer>
      handler_;

  public FileHierarchyAssetBundleSeparator(
      IFileHierarchy fileHierarchy,
      Action<IFileHierarchyDirectory, IFileBundleOrganizer> handler) {
    this.fileHierarchy_ = fileHierarchy;
    this.handler_ = handler;
  }

  public void GatherFileBundles(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress) {
    foreach (var directory in this.fileHierarchy_) {
      this.handler_(directory, organizer);
    }

    mutablePercentageProgress.ReportProgressAndCompletion();
  }
}