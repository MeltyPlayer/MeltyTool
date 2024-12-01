using fin.compression;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

using schema.binary;

namespace sm64ds.api;

public class Sm64dsModelImporter : IModelImporter<Sm64dsModelFileBundle> {
  public IModel Import(Sm64dsModelFileBundle fileBundle) {
    var bmdFile = fileBundle.BmdFile;

    var files = bmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var ms = new MemoryStream(
        new Lz77Decompressor().Decompress(bmdFile.OpenReadAsBinary()));
    var br = new SchemaBinaryReader(ms);
    //var bmd = br.ReadNew<Bmd>();

    return model;
  }
}