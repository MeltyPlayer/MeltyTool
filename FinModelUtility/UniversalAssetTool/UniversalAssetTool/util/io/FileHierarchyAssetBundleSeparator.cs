using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.util.io {
  public class FileHierarchyAssetBundleSeparator
      : IAnnotatedFileBundleGatherer {
    private readonly IFileHierarchy fileHierarchy_;

    private readonly Func<IFileHierarchyDirectory,
            IEnumerable<IAnnotatedFileBundle>>
        handler_;

    public FileHierarchyAssetBundleSeparator(
        IFileHierarchy fileHierarchy,
        Func<IFileHierarchyDirectory,
            IEnumerable<IAnnotatedFileBundle>> handler) {
      this.fileHierarchy_ = fileHierarchy;
      this.handler_ = handler;
    }

    public IEnumerable<IAnnotatedFileBundle> GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      var bundles = this.fileHierarchy_.SelectMany(directory
                                                => this.handler_(directory));
      mutablePercentageProgress.ReportProgressAndCompletion();
      return bundles;
    }
  }

  public class FileHierarchyAssetBundleSeparator<TFileBundle>
      : IAnnotatedFileBundleGatherer<TFileBundle>
      where TFileBundle : IFileBundle {
    private readonly IFileHierarchy fileHierarchy_;

    private readonly Func<IFileHierarchyDirectory,
            IEnumerable<IAnnotatedFileBundle<TFileBundle>>>
        handler_;

    public FileHierarchyAssetBundleSeparator(
        IFileHierarchy fileHierarchy,
        Func<IFileHierarchyDirectory,
            IEnumerable<IAnnotatedFileBundle<TFileBundle>>> handler) {
      this.fileHierarchy_ = fileHierarchy;
      this.handler_ = handler;
    }

    public IEnumerable<IAnnotatedFileBundle<TFileBundle>> GatherFileBundles(
        IMutablePercentageProgress mutablePercentageProgress) {
      var bundles = this.fileHierarchy_.SelectMany(directory
                                                => this.handler_(directory));
      mutablePercentageProgress.ReportProgressAndCompletion();
      return bundles;
    }
  }
}