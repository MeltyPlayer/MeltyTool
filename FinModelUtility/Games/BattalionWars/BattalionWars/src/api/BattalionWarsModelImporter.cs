using fin.model;
using fin.model.io.importers;

namespace modl.api;

public sealed class BattalionWarsModelImporter 
    : IModelImporter<IBattalionWarsModelFileBundle> {
  public IModel Import(IBattalionWarsModelFileBundle modelFileBundle)
    => modelFileBundle switch {
        ModlModelFileBundle modlFileBundle => new ModlModelImporter().Import(modlFileBundle),
        OutModelFileBundle outFileBundle => new OutModelImporter().Import(
            outFileBundle),
        _ => throw new ArgumentOutOfRangeException(
            nameof(modelFileBundle), modelFileBundle, null)
    };
}