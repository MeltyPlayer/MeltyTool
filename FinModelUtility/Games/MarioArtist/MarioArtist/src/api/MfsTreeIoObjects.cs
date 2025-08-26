using System.Runtime.CompilerServices;

using fin.data.dictionaries;
using fin.data.queues;
using fin.io;
using fin.util.asserts;
using fin.util.linq;

using marioartist.schema.leo;
using marioartist.schema.mfs;

namespace marioartist.api;

public abstract class MfsTreeIoObject(IReadOnlyTreeDirectory? parent)
    : IReadOnlyTreeIoObject {
  public abstract string FullPath { get; }
  public ReadOnlySpan<char> Name => FinIoStatic.GetName(this.FullPath);

  public abstract IEnumerable<MfsTreeIoObject> Children { get; }

  public IReadOnlyTreeDirectory AssertGetParent() {
    Asserts.True(parent != null);
    return parent!;
  }

  public bool TryGetParent(out IReadOnlyTreeDirectory outParent) {
    outParent = parent!;
    return parent != null;
  }

  public IEnumerable<IReadOnlyTreeDirectory> GetAncestry() {
    IReadOnlyTreeIoObject current = this;
    while (current.TryGetParent(out var parent)) {
      yield return parent;
      current = parent;
    }
  }
}

public class MfsTreeDirectory(
    IReadOnlyTreeDirectory? parent,
    MfsDirectory impl,
    LinkedList<MfsTreeDirectory> subdirs,
    LinkedList<MfsTreeFile> files)
    : MfsTreeIoObject(parent), IReadOnlyTreeDirectory {
  public static MfsTreeDirectory CreateTreeFromMfsDisk(MfsDisk mfsDisk) {
    if (mfsDisk.Volume == null) {
      throw mfsDisk.Error switch {
          MfsDiskError.INVALID
              => new InvalidDataException("Disk data is invalid"),
          MfsDiskError.NOT_MFS
              => new NotSupportedException("Disk is not an MFS filesystem"),
          _ => new Exception("Unknown error")
      };
    }

    var disk = mfsDisk.Disk;
    var volume = mfsDisk.Volume;

    var mfsDirectoryById = new Dictionary<ushort, MfsDirectory>();
    var mfsEntryByParentId = new ListDictionary<ushort, IMfsEntry>();
    foreach (var mfsEntry in mfsDisk.Volume.MfsEntries) {
      mfsEntryByParentId.Add(mfsEntry.ParentDirectoryIndex, mfsEntry);

      if (mfsEntry is MfsDirectory mfsDirectory) {
        mfsDirectoryById[mfsDirectory.DirectoryId] = mfsDirectory;
      }
    }

    var rootDirectory = mfsDirectoryById[0];
    var rootSubdirs = new LinkedList<MfsTreeDirectory>();
    var rootFiles = new LinkedList<MfsTreeFile>();
    var rootTreeDirectory
        = new MfsTreeDirectory(null, rootDirectory, rootSubdirs, rootFiles);

    var directoryQueue
        = new FinQueue<(MfsDirectory, MfsTreeDirectory,
            LinkedList<MfsTreeDirectory>, LinkedList<MfsTreeFile>)>(
            (rootDirectory, rootTreeDirectory, rootSubdirs, rootFiles));
    while (directoryQueue.TryDequeue(out var tuple)) {
      var (directory, treeDirectory, subdirs, files) = tuple;

      if (!mfsEntryByParentId.TryGetList(directory.DirectoryId,
                                         out var childEntries)) {
        continue;
      }

      foreach (var childEntry in childEntries) {
        switch (childEntry) {
          case MfsDirectory childDirectory: {
            var childSubdirs = new LinkedList<MfsTreeDirectory>();
            var childFiles = new LinkedList<MfsTreeFile>();

            var childTreeDirectory = new MfsTreeDirectory(treeDirectory,
              childDirectory,
              childSubdirs,
              childFiles);
            subdirs.AddLast(childTreeDirectory);

            directoryQueue.Enqueue((childDirectory, childTreeDirectory,
                                    childSubdirs, childFiles));
            break;
          }
          case MfsFile childFile: {
            files.AddLast(
                new MfsTreeFile(treeDirectory, disk, volume, childFile));
            break;
          }
        }
      }
    }

    return rootTreeDirectory;
  }

  public override string FullPath { get; }
    = Path.Join(parent?.FullPath ?? "", impl.Name);

  public override IEnumerable<MfsTreeIoObject> Children
    => subdirs.CastTo<MfsTreeDirectory, MfsTreeIoObject>().Concat(files);

  public bool IsEmpty => subdirs.Count == 0 && files.Count == 0;


  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerable<IReadOnlyTreeDirectory> GetExistingSubdirs() => subdirs;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool TryToGetExistingSubdir(ReadOnlySpan<char> relativePath,
                                     out IReadOnlyTreeDirectory subdir)
    => throw new NotImplementedException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IReadOnlyTreeDirectory AssertGetExistingSubdir(
      ReadOnlySpan<char> relativePath)
    => throw new NotImplementedException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerable<IReadOnlyTreeFile> GetExistingFiles() => files;

  public bool TryToGetExistingFile(ReadOnlySpan<char> path,
                                   out IReadOnlyTreeFile outFile)
    => throw new NotImplementedException();

  public bool TryToGetExistingFileWithFileType(
      string pathWithoutExtension,
      out IReadOnlyTreeFile outFile,
      params string[] extensions)
    => throw new NotImplementedException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IReadOnlyTreeFile AssertGetExistingFile(ReadOnlySpan<char> path)
    => throw new NotImplementedException();

  public IEnumerable<IReadOnlyTreeFile> GetFilesWithNameRecursive(
      string name)
    => throw new NotImplementedException();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IEnumerable<IReadOnlyTreeFile> GetFilesWithFileType(
      string extension,
      bool includeSubdirs = false)
    => throw new NotImplementedException();
}

public class MfsTreeFile(
    IReadOnlyTreeDirectory parent,
    LeoDisk disk,
    MfsRamVolume volume,
    MfsFile impl)
    : MfsTreeIoObject(parent), IReadOnlyTreeFile {
  public override IEnumerable<MfsTreeIoObject> Children => [];

  public override string FullPath { get; }
    = Path.Join(parent.FullPath, $"{impl.Name}.{impl.Ext}");

  public string DisplayFullPath => this.FullPath;

  public string FileType => FinFileStatic.GetExtension(this.FullPath);

  public string FullNameWithoutExtension
    => FinFileStatic.GetNameWithoutExtension(this.FullPath).ToString();

  public ReadOnlySpan<char> NameWithoutExtension
    => FinIoStatic.GetName(this.FullPath);

  public Stream OpenRead() {
    byte[] filedata = new byte[impl.Size];

    //Add FAT Entries and Copy to data to blocks
    ushort nextblock = impl.FatEntry;
    uint offset = 0;
    uint size = impl.Size;

    //Recursively copy blocks
    do {
      byte[] blockdata = disk
                         .ReadLBA(Leo.RamStartLBA[volume.DiskType] + nextblock)
                         .AssertNonnull();
      int blocksize = blockdata.Length;
      Array.Copy(blockdata, 0, filedata, offset, Math.Min(blocksize, size));
      offset += (uint) Math.Min(blocksize, size);
      size -= (uint) Math.Min(blocksize, size);

      nextblock = volume.FatEntries[nextblock];
    } while (nextblock != (ushort) Mfs.FAT.LastFileBlock);

    return new MemoryStream(filedata);
  }
}