using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;


namespace marioartist.schema;

public record TstltModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public partial class TstltModelLoader : IModelImporter<TstltModelFileBundle> {
  public IModel Import(TstltModelFileBundle fileBundle) {
    var tstlt = fileBundle.MainFile.ReadNew<Tstlt>();

    var model = new ModelImpl {
        Files = fileBundle.MainFile.AsFileSet(),
        FileBundle = fileBundle,
    };

    var materialManager = model.MaterialManager;
    var thumbnailTexture = materialManager.CreateTexture(tstlt.Thumbnail.ToImage());
    thumbnailTexture.Name = "thumbnail";

    var imageTexture = materialManager.CreateTexture(tstlt.FaceTextures.ToImage());
    imageTexture.Name = "face";

    return model;
  }
}
