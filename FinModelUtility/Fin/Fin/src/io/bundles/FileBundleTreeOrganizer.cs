using System.Collections.Generic;

using fin.data.lazy;

namespace fin.io.bundles;

public interface IFileBundleOrganizer {
  void Add(IAnnotatedFileBundle annotatedFileBundle);
}

public class FileBundleTreeOrganizer : IFileBundleOrganizer {
  private readonly FileBundleDirectory root_ = new("(root)");

  private readonly
      LazyDictionary<IFileHierarchyDirectory, IFileBundleDirectory>
      lazyFileHierarchyDirToBundleDir_;

  public FileBundleTreeOrganizer() {
    this.lazyFileHierarchyDirToBundleDir_ = new((lazyDict, dir) => {
      var parent = dir.Parent;
      if (parent == null) {
        return this.root_.AddSubdir(dir.Hierarchy.Root);
      }

      return lazyDict[parent].AddSubdir(dir);
    });
  }

  public void Add(IAnnotatedFileBundle annotatedFileBundle) {
    this.lazyFileHierarchyDirToBundleDir_[annotatedFileBundle.File.Parent!]
        .AddFileBundle(annotatedFileBundle);
  }

  public IFileBundleDirectory CleanUpAndGetRoot() {
    this.root_.CleanUp();
    return this.root_;
  }
}

public class FileBundleListOrganizer : IFileBundleOrganizer {
  public List<IAnnotatedFileBundle> List { get; } = new();

  public void Add(IAnnotatedFileBundle annotatedFileBundle)
    => this.List.Add(annotatedFileBundle);
}