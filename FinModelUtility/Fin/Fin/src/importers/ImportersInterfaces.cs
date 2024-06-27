using System.Collections.Generic;

using fin.io;
using fin.io.bundles;

namespace fin.importers;

public interface IResource {
  IFileBundle FileBundle { get; }
  IReadOnlySet<IReadOnlyGenericFile> Files { get; }
}

public interface IImporter<out TResource, in TFileBundle>
    where TResource : IResource
    where TFileBundle : IFileBundle {
  TResource Import(TFileBundle fileBundle);
}


public interface I3dFileBundle : IFileBundle {
  /// <summary>
  ///   Whether to use a low-level exporter when exporting. This supports
  ///   less features at the moment, but is required for exporting huge
  ///   models without running into out of memory exceptions.
  /// </summary>
  bool UseLowLevelExporter => false;

  bool ForceGarbageCollection => false;
}

public interface I3dImporter<out TResource, in TFileBundle>
    : IImporter<TResource, TFileBundle>
    where TResource : IResource
    where TFileBundle : I3dFileBundle;