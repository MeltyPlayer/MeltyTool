using fin.importers;

namespace fin.model.io.importers;

public interface IModelImporter<in TModelFileBundle>
    : I3dImporter<IModel, TModelFileBundle>
    where TModelFileBundle : IModelFileBundle;