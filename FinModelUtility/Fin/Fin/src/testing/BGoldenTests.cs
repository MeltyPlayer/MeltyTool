using fin.io;
using fin.io.bundles;

namespace fin.testing {
  public abstract class BGoldenTests<TFileBundle>
      where TFileBundle : IFileBundle {
    public abstract TFileBundle GetFileBundleFromDirectory(
        IFileHierarchyDirectory directory);
  }
}