using fin.compression;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

namespace nitro.api;

public class NsbmdModelImporter : IModelImporter<NsbmdModelFileBundle> {
  public IModel Import(NsbmdModelFileBundle fileBundle) {
    var nsbmdFile = fileBundle.NsbmdFile;

    var files = nsbmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var nsbmdData
        = new Lz77Decompressor().Decompress(nsbmdFile.OpenReadAsBinary());

    return model;
  }
}