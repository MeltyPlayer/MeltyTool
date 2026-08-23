using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.io;
using fin.io.bundles;

using SubstreamSharp;

namespace fin.archives;

public interface ISimpleArchiveFileBundle<out TThis> : IArchiveFileBundle2
    where TThis : ISimpleArchiveFileBundle<TThis> {
  static abstract TThis FromFile(IReadOnlyTreeFile file);
}

public interface ISimpleArchiveDirectory : IArchiveDirectory2 {
  ISimpleArchiveDirectory AddSubdir(string name);
  void AddFile(string path, long position, long length);

  void AddFile(string path,
               long position,
               long length,
               Func<Stream, Stream> processor);

  void AddFile(string path, string text);
}

public abstract class BSimpleArchiveImporter<TBundle>
    : IArchiveImporter2<TBundle>
    where TBundle : IArchiveFileBundle2 {
  protected abstract void BuildHierarchyAndGetFileStream(
      TBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream);

  public IArchive2 Import(TBundle fileBundle) {
    var fileSet = fileBundle.Files.ToHashSet();

    var archive = new SimpleArchive {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var root = new SimpleArchiveDirectory(archive, "");
    archive.Root = root;
    this.BuildHierarchyAndGetFileStream(fileBundle,
                                        fileSet,
                                        root,
                                        out var baseStream,
                                        out var readStream);
    archive.BaseStream = baseStream;
    archive.ReadStream = readStream;

    return archive;
  }

  private sealed class SimpleArchive : IArchive2 {
    public required IFileBundle FileBundle { get; init; }
    public required IReadOnlySet<IReadOnlyGenericFile> Files { get; init; }

    public IArchiveDirectory2 Root { get; set; }
    public Stream BaseStream { get; set; }
    public Stream ReadStream { get; set; }

    public void Dispose() => this.BaseStream.Dispose();
  }

  private sealed class SimpleArchiveDirectory(
      SimpleArchive archive,
      string name)
      : ISimpleArchiveDirectory {
    private readonly Dictionary<string, SimpleArchiveDirectory>
        subdirsImpl_ = new();

    private readonly Dictionary<string, IArchiveFile2> filesImpl_ = new();

    public string Name => name;
    public IEnumerable<IArchiveDirectory2> Subdirs => this.subdirsImpl_.Values;
    public IEnumerable<IArchiveFile2> Files => this.filesImpl_.Values;

    public ISimpleArchiveDirectory AddSubdir(string path)
      => this.GetOrAddSubdirsFromParts_(SplitPath_(path));

    public void AddFile(string path, long position, long length) {
      var (parentDir, fileName) = this.GetParentDirAndFileName_(path);
      parentDir.filesImpl_[fileName]
          = new SimpleArchiveFile(archive, fileName, position, length);
    }

    public void AddFile(string path,
                        long position,
                        long length,
                        Func<Stream, Stream> processor) {
      var (parentDir, fileName) = this.GetParentDirAndFileName_(path);
      parentDir.filesImpl_[fileName] = new ProcessedArchiveFile(
          archive,
          fileName,
          position,
          length,
          processor);
    }

    public void AddFile(string path, string text) {
      var (parentDir, fileName) = this.GetParentDirAndFileName_(path);
      parentDir.filesImpl_[fileName] = new TextArchiveFile(fileName, text);
    }


    private (SimpleArchiveDirectory, string) GetParentDirAndFileName_(
        string path) {
      var parts = SplitPath_(path);

      var directoryParts = parts[..^1];
      var fileName = parts[^1];

      var parentDir = this.GetOrAddSubdirsFromParts_(directoryParts);
      return (parentDir, fileName);
    }

    private static ReadOnlySpan<string> SplitPath_(string name)
      => name.Split('/', '\\');

    private SimpleArchiveDirectory GetOrAddSubdirsFromParts_(
        ReadOnlySpan<string> parts) {
      SimpleArchiveDirectory current = this;
      foreach (var part in parts) {
        current = this.GetOrAddSubdirFromPart_(part);
      }

      return current;
    }

    private SimpleArchiveDirectory GetOrAddSubdirFromPart_(string part) {
      if (part == "") {
        return this;
      }

      if (!this.subdirsImpl_.TryGetValue(part, out var subdir)) {
        subdir = new SimpleArchiveDirectory(archive, part);
        this.subdirsImpl_[part] = subdir;
      }

      return subdir;
    }
  }

  private sealed class SimpleArchiveFile(
      SimpleArchive archive,
      string name,
      long position,
      long length) : IArchiveFile2 {
    public string Name => name;

    public Stream OpenRead() => archive.ReadStream.Substream(position, length);
  }

  private sealed class ProcessedArchiveFile(
      SimpleArchive archive,
      string name,
      long position,
      long length,
      Func<Stream, Stream> processor) : IArchiveFile2 {
    public string Name => name;

    public Stream OpenRead()
      => processor(archive.ReadStream.Substream(position, length));
  }

  private sealed class TextArchiveFile(
      string name,
      string text) : IArchiveFile2 {
    public string Name => name;

    public Stream OpenRead() {
      var stream = new MemoryStream();
      var writer = new StreamWriter(stream);
      writer.Write(text);
      writer.Flush();
      stream.Position = 0;
      return stream;
    }
  }
}