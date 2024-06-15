using System.Collections.Generic;

using fin.model;
using fin.model.impl;

namespace fin.scene {
  public partial class SceneImpl {
    private class SceneObjectImpl : ISceneObject {
      private readonly List<ISceneModel> models_ = [];
      public Position Position { get; private set; }
      public IRotation Rotation { get; } = new RotationImpl();
      public Scale Scale { get; private set; } = new Scale(1, 1, 1);

      public ISceneObject SetPosition(float x, float y, float z) {
        this.Position = new Position(x, y, z);
        return this;
      }

      public ISceneObject SetRotationRadians(float xRadians,
                                             float yRadians,
                                             float zRadians) {
        this.Rotation.SetRadians(
            xRadians,
            yRadians,
            zRadians
        );
        return this;
      }

      public ISceneObject SetRotationDegrees(float xDegrees,
                                             float yDegrees,
                                             float zDegrees) {
        this.Rotation.SetDegrees(
            xDegrees,
            yDegrees,
            zDegrees
        );
        return this;
      }

      public ISceneObject SetScale(float x, float y, float z) {
        this.Scale = new Scale(x, y, z);
        return this;
      }

      public IReadOnlyList<ISceneModel> Models => this.models_;

      public ISceneModel AddSceneModel(IModel model) {
        var sceneModel = new SceneModelImpl(model);
        this.models_.Add(sceneModel);
        return sceneModel;
      }

      public ISceneObject.OnTick? TickHandler { get; private set; }

      public ISceneObject SetOnTickHandler(ISceneObject.OnTick handler) {
        this.TickHandler = handler;
        return this;
      }
    }
  }
}