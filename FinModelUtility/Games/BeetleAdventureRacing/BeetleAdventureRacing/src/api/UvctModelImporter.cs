using bar.schema;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvctModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory)
    : IModelFileBundle<UvctModelFileBundle, UvctModelFileImporter>;

public sealed class UvctModelFileImporter
    : IModelImporter<UvctModelFileImporter, UvctModelFileBundle> {
  public IModel Import(UvctModelFileBundle fileBundle) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return new ModelImpl {
          FileBundle = fileBundle,
          Files = fileBundle.MainFile.AsFileSet(),
      };
    }

    var uvct = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
        .ReadNew<Uvct>();
    return UvmdModelFileImporter.FromMaterialMeshes(
        fileBundle,
        fileBundle.RootDirectory,
        false,
        uvct.MaterialMeshes.Select(m => m.Impl),
        true);
  }
}