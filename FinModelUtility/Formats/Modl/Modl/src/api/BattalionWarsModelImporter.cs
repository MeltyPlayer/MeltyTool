using fin.model;
using fin.model.io.importers;

namespace modl.api;

public class BattalionWarsModelImporter 
    : IModelImporter<IBattalionWarsModelFileBundle> {
  public IModel Import(IBattalionWarsModelFileBundle modelFileBundle)
    => modelFileBundle switch {
        ModlModelFileBundle modlFileBundle => new ModlModelImporter()
                                              .ImportAsync(modlFileBundle).Result,
        OutModelFileBundle outFileBundle => new OutModelImporter().Import(
            outFileBundle),
        _ => throw new ArgumentOutOfRangeException(
            nameof(modelFileBundle), modelFileBundle, null)
    };
}