using System.Collections.Generic;
using System.Linq;

using fin.util.strings;

namespace fin.io.bundles;

public interface IFileBundleDirectory {
  IReadOnlyTreeDirectory? Directory { get; }
  string Name { get; }

  IReadOnlyList<IFileBundleDirectory> Subdirs { get; }
  IReadOnlyList<IFileBundle> FileBundles { get; }

  IFileBundleDirectory AddSubdir(IReadOnlyTreeDirectory directory);
  IFileBundleDirectory AddSubdir(string name);
  IFileBundleDirectory GetOrAddSubdir(string name);
  void AddFileBundle(IFileBundle fileBundle);

  void CleanUp();
}

public sealed class FileBundleDirectory : IFileBundleDirectory {
  private readonly List<IFileBundleDirectory> subdirs_ = [];
  private readonly List<IFileBundle> fileBundles_ = [];

  public FileBundleDirectory(IReadOnlyTreeDirectory? directory) {
    this.Directory = directory;
    this.Name = directory.Name.ToString();
  }

  public FileBundleDirectory(string name) {
    this.Name = name;
  }

  public IReadOnlyTreeDirectory? Directory { get; }
  public string Name { get; }

  public IReadOnlyList<IFileBundleDirectory> Subdirs => this.subdirs_;
  public IReadOnlyList<IFileBundle> FileBundles => this.fileBundles_;

  public IFileBundleDirectory AddSubdir(IReadOnlyTreeDirectory directory) {
    var subdir = new FileBundleDirectory(directory);
    this.subdirs_.Add(subdir);
    return subdir;
  }

  public IFileBundleDirectory AddSubdir(string name) {
    var subdir = new FileBundleDirectory(name);
    this.subdirs_.Add(subdir);
    return subdir;
  }

  /// <summary>
  ///   Gets the virtual (not backed by a directory) subdir with the given
  ///   name, adding it if it doesn't exist yet.
  /// </summary>
  public IFileBundleDirectory GetOrAddSubdir(string name)
    => this.subdirs_.FirstOrDefault(
           subdir => subdir.Directory == null && subdir.Name == name) ??
       this.AddSubdir(name);

  public void AddFileBundle(IFileBundle fileBundle)
    => this.fileBundles_.Add(fileBundle);

  public void CleanUp() {
    var subdirsToRemove = new List<IFileBundleDirectory>();
    foreach (var subdir in this.subdirs_) {
      subdir.CleanUp();
      if (!subdir.Subdirs.Any() && !subdir.FileBundles.Any()) {
        subdirsToRemove.Add(subdir);
      }
    }

    foreach (var subdir in subdirsToRemove) {
      this.subdirs_.Remove(subdir);
    }

    this.subdirs_.Sort((lhs, rhs) => StringUtil.NaturalSortInstance.Compare(lhs.Name, rhs.Name));
    this.fileBundles_.Sort();
  }
}