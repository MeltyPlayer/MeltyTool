using System.Threading.Tasks;

using fin.importers;

namespace fin.model.io.importers;

public interface IModelImporter<in TModelFileBundle>
    : I3dImporter<IModel, TModelFileBundle>
    where TModelFileBundle : IModelFileBundle;

public interface IAsyncModelImporter<in TModelFileBundle>
    : IModelImporter<TModelFileBundle>
    where TModelFileBundle : IModelFileBundle {
  IModel IImporter<IModel, TModelFileBundle>.Import(
      TModelFileBundle modelFileBundle)
    => this.ImportAsync(modelFileBundle).Result;

  Task<IModel> ImportAsync(TModelFileBundle modelFileBundle);
}