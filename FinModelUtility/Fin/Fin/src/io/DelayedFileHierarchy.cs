using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

using fin.util.asserts;
using fin.util.enumerables;

namespace fin.io;

public static partial class FileHierarchy {
  private class DelayedFileHierarchy : IFileHierarchy {
    public DelayedFileHierarchy(ISystemDirectory directory) : this(
        directory.Name,
        directory) { }

    public DelayedFileHierarchy(string name, ISystemDirectory directory) {
      this.Name = name;
      this.Root = new FileHierarchyDirectory(this, directory);
    }

    public string Name { get; }
    public IFileHierarchyDirectory Root { get; }


    private abstract class BFileHierarchyIoObject : IFileHierarchyIoObject {
      protected BFileHierarchyIoObject(IFileHierarchy hierarchy) {
        this.Hierarchy = hierarchy;
        this.LocalPath = string.Empty;
      }

      protected BFileHierarchyIoObject(IFileHierarchy hierarchy,
                                       ISystemIoObject instance) {
        this.Hierarchy = hierarchy;
        this.LocalPath
            = instance.FullPath.Substring(hierarchy.Root.FullPath.Length);
      }

      protected abstract ISystemIoObject Instance { get; }

      public string LocalPath { get; }
      public IFileHierarchy Hierarchy { get; }

      public IFileHierarchyDirectory? Parent
        => this.Instance.TryGetParent(out var parent)
            ? new FileHierarchyDirectory(this.Hierarchy, parent)
            : null;

      public bool Equals(IReadOnlyTreeIoObject? other)
        => this.Instance.Equals(other);

      public IReadOnlyTreeDirectory AssertGetParent()
        => Asserts.True(this.TryGetParent(out var parent))
            ? parent
            : default!;

      public bool TryGetParent(out IReadOnlyTreeDirectory parent)
        => this.Instance.TryGetParent(out parent);

      public IEnumerable<IReadOnlyTreeDirectory> GetAncestry()
        => this.Instance.GetAncestry();

      public bool Exists => this.Instance.Exists;
      public string FullPath => this.Instance.FullPath;

      public string Name => this.Parent == null
          ? this.Hierarchy.Name
          : this.Instance.Name;

      public override string ToString() => this.LocalPath;
    }


    private class FileHierarchyDirectory(
        IFileHierarchy hierarchy,
        ISystemDirectory impl)
        : BFileHierarchyIoObject(hierarchy),
          IFileHierarchyDirectory {
      protected override ISystemIoObject Instance => this.Impl;
      public ISystemDirectory Impl { get; } = impl;

      public bool IsEmpty => this.Impl.IsEmpty;

      public IEnumerable<IFileHierarchyDirectory> GetExistingSubdirs()
        => this.Impl.GetExistingSubdirs()
               .Select(
                   dir => new FileHierarchyDirectory(this.Hierarchy, dir));

      public IEnumerable<IFileHierarchyFile> GetExistingFiles()
        => this.Impl.GetExistingFiles()
               .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public void Refresh(bool _) { }

      public IFileHierarchyFile AssertGetExistingFile(string relativePath) {
        Asserts.True(
            this.TryToGetExistingFile(relativePath, out var outFile));
        return outFile;
      }

      public bool TryToGetExistingFile(
          string localPath,
          out IFileHierarchyFile outFile) {
        if (this.Impl.TryToGetExistingFile(localPath, out var file)) {
          outFile = new FileHierarchyFile(this.Hierarchy, file);
          return true;
        }

        outFile = default;
        return false;
      }

      public IFileHierarchyDirectory AssertGetExistingSubdir(
          string relativePath) {
        Asserts.True(
            this.TryToGetExistingSubdir(relativePath, out var outDir));
        return outDir;
      }

      public bool TryToGetExistingSubdir(
          string localPath,
          out IFileHierarchyDirectory outDirectory) {
        if (this.Impl.TryToGetExistingSubdir(localPath, out var subdir)) {
          outDirectory = new FileHierarchyDirectory(this.Hierarchy, subdir);
          return true;
        }

        outDirectory = default;
        return false;
      }

      public bool TryToGetExistingFileWithFileType(
          string pathWithoutExtension,
          out IFileHierarchyFile outFile,
          params string[] fileTypes) {
        if (this.Impl.TryToGetExistingFileWithFileType(
                pathWithoutExtension,
                out var file,
                fileTypes)) {
          outFile = new FileHierarchyFile(this.Hierarchy, file);
          return true;
        }

        outFile = default;
        return false;
      }

      public IEnumerable<IFileHierarchyFile> GetFilesWithNameRecursive(
          string name)
        => this.Impl.GetFilesWithNameRecursive(name)
               .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> GetFilesWithFileType(
          string fileType,
          bool includeSubdirs = false)
        => includeSubdirs
            ? FilesWithExtensionRecursive(fileType)
            : FilesWithExtension(fileType);

      public IEnumerable<IFileHierarchyFile> FilesWithExtension(
          string extension)
        => this.Impl.GetFilesWithFileType(extension)
               .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> FilesWithExtensions(
          IEnumerable<string> extensions)
        => extensions
           .SelectMany(
               fileType => this.Impl.GetFilesWithFileType(fileType))
           .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> FilesWithExtensions(
          string first,
          params string[] rest)
        => first.Yield()
                .Concat(rest)
                .SelectMany(
                    fileType => this.Impl.GetFilesWithFileType(fileType))
                .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> FilesWithExtensionRecursive(
          string extension)
        => this.Impl.GetFilesWithFileType(extension, true)
               .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> FilesWithExtensionsRecursive(
          IEnumerable<string> extensions)
        => extensions
           .SelectMany(fileType
                           => this.Impl.GetFilesWithFileType(fileType, true))
           .Select(file => new FileHierarchyFile(this.Hierarchy, file));

      public IEnumerable<IFileHierarchyFile> FilesWithExtensionsRecursive(
          string first,
          params string[] rest)
        => first.Yield()
                .Concat(rest)
                .SelectMany(
                    fileType => this.Impl.GetFilesWithFileType(
                        fileType,
                        true))
                .Select(file => new FileHierarchyFile(this.Hierarchy, file));
    }

    private class FileHierarchyFile(
        IFileHierarchy hierarchy,
        ISystemFile file)
        : BFileHierarchyIoObject(hierarchy, file),
          IFileHierarchyFile {
      protected override ISystemIoObject Instance => this.Impl;
      public ISystemFile Impl { get; } = file;

      // File fields
      public string FileType => this.Impl.FileType;

      public string FullNameWithoutExtension
        => this.Impl.FullNameWithoutExtension;

      public string NameWithoutExtension => this.Impl.NameWithoutExtension;

      public string DisplayFullPath
        => $"//{this.Hierarchy.Name}{this.LocalPath.Replace('\\', '/')}";

      public FileSystemStream OpenRead() => this.Impl.OpenRead();
    }

    public IEnumerator<IFileHierarchyDirectory> GetEnumerator() {
      var directoryQueue = new Queue<IFileHierarchyDirectory>();
      directoryQueue.Enqueue(this.Root);
      while (directoryQueue.Count > 0) {
        var directory = directoryQueue.Dequeue();

        yield return directory;

        foreach (var subdir in directory.GetExistingSubdirs()) {
          directoryQueue.Enqueue(subdir);
        }
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }
}