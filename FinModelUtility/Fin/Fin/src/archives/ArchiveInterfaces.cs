using System;

using fin.importers;
using fin.io;
using fin.io.bundles;

namespace fin.archives;

public interface IArchiveBundle : IFileBundle {
  FileBundleType IFileBundle.Type => FileBundleType.ARCHIVE;
}

public interface IArchive : IResource, IDisposable {
  IReadOnlyTreeDirectory Root { get; }
}

public interface IArchiveImporter<in TBundle>
    : IImporter<IArchive, TBundle>
    where TBundle : IArchiveBundle;