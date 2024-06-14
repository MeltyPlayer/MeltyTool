using fin.data.dictionaries;
using fin.model;

namespace fin.scene {
  public partial class SceneImpl {
    private class SceneModelImpl : ISceneModel {
      private readonly ListDictionary<IReadOnlyBone, IReadOnlySceneModel>
          children_ = new();

      public SceneModelImpl(IReadOnlyModel model) {
        this.Model = model;
      }

      public IReadOnlyListDictionary<IReadOnlyBone, IReadOnlySceneModel>
          Children => this.children_;

      public ISceneModel AddModelOntoBone(IReadOnlyModel model,
                                          IReadOnlyBone bone) {
        var child = new SceneModelImpl(model);
        this.children_.Add(bone, child);
        return child;
      }

      public IReadOnlyModel Model { get; }
    }
  }
}