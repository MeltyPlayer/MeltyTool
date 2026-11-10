using System;
using System.Collections.Generic;

using fin.data.lazy;

namespace fin.io.bundles;

public interface IFileBundleOrganizer {
  void Add(IFileBundle fileBundle);
}

public sealed class FileBundleTreeOrganizer : IFileBundleOrganizer {
  private readonly FileBundleDirectory root_ = new("(root)");

  private readonly
      LazyDictionary<IReadOnlyTreeDirectory, IFileBundleDirectory>
      lazyDirToBundleDir_;

  public FileBundleTreeOrganizer() {
    this.lazyDirToBundleDir_ = new((lazyDict, dir) => {
      if (!dir.TryGetParent(out var parent)) {
        return this.root_.AddSubdir(dir);
      }

      return lazyDict[parent].AddSubdir(dir);
    });
  }

  public void Add(IFileBundle fileBundle)
    => this.GetBundleDirectory_(fileBundle).AddFileBundle(fileBundle);

  private IFileBundleDirectory GetBundleDirectory_(IFileBundle fileBundle) {
    IReadOnlyTreeDirectory dir = fileBundle.MainFile.AssertGetParent();
    string? virtualPath = fileBundle.VirtualDirectoryPath;
    if (virtualPath == null) {
      return this.lazyDirToBundleDir_[dir];
    }

    while (dir.TryGetParent(out IReadOnlyTreeDirectory? parent)) {
      dir = parent;
    }

    // Real directories along the path are reused, so bundles are shown
    // alongside any files that are actually there. Once a folder doesn't
    // exist on disk, the rest of the path is virtual.
    IFileBundleDirectory? virtualDir = null;
    foreach (string name in virtualPath.Split('/',
                                              StringSplitOptions
                                                  .RemoveEmptyEntries)) {
      if (virtualDir == null &&
          dir.TryToGetExistingSubdir(name, out IReadOnlyTreeDirectory? subdir)) {
        dir = subdir;
        continue;
      }

      virtualDir = (virtualDir ?? this.lazyDirToBundleDir_[dir])
          .GetOrAddSubdir(name);
    }

    return virtualDir ?? this.lazyDirToBundleDir_[dir];
  }

  public IFileBundleDirectory CleanUpAndGetRoot() {
    this.root_.CleanUp();
    return this.root_;
  }
}

public sealed class FileBundleListOrganizer : IFileBundleOrganizer {
  public List<IFileBundle> List { get; } = [];

  public void Add(IFileBundle fileBundle)
    => this.List.Add(fileBundle);
}