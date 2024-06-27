using fin.data.lazy;
using fin.io;
using fin.io.bundles;

namespace uni.util.io {
  public class FileBundleHierarchyOrganizer {
    public IFileBundleDirectory Organize(
        IEnumerable<IAnnotatedFileBundle> fileBundles) {
      var rootFileBundleDirectory = new FileBundleDirectory("(root)");

      var lazyFileHierarchyDirToBundleDir =
          new LazyDictionary<IFileHierarchyDirectory, IFileBundleDirectory>(
              (lazyDict, dir) => {
                var parent = dir.Parent;
                if (parent == null) {
                  return rootFileBundleDirectory.AddSubdir(dir.Hierarchy.Root);
                }

                return lazyDict[parent].AddSubdir(dir);
              });

      foreach (var fileBundle in fileBundles) {
        lazyFileHierarchyDirToBundleDir[fileBundle.File.Parent!]
            .AddFileBundle(fileBundle);
      }

      rootFileBundleDirectory.CleanUp();
      return rootFileBundleDirectory;
    }
  }
}