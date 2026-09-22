using fin.importers;

namespace fin.model.io.importers;

public interface IModelImporter<TSelf, in TModelFileBundle>
    : I3dImporter<IModel, TModelFileBundle>
    where TSelf : IModelImporter<TSelf, TModelFileBundle>, new()
    where TModelFileBundle : IModelFileBundle<TModelFileBundle, TSelf>;