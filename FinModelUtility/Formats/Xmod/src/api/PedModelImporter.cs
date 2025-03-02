using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

using xmod.schema.ped;


namespace xmod.api;

public class PedModelImporter : IModelImporter<PedModelFileBundle> {
  public IModel Import(PedModelFileBundle modelFileBundle) {
    var pedFile = modelFileBundle.PedFile;
    var ped = pedFile.ReadNewFromText<Ped>();

    var files = pedFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    return finModel;
  }
}