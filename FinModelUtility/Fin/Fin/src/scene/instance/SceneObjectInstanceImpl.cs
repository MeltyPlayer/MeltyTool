using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.model;

namespace fin.scene.instance;

public partial class SceneInstanceImpl {
  private class SceneObjectInstanceImpl(IReadOnlySceneObject sceneObject)
      : ISceneObjectInstance {
    ~SceneObjectInstanceImpl() => ReleaseUnmanagedResources_();

    public void Dispose() {
      ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var model in this.Models) {
        model.Dispose();
      }
    }

    public IReadOnlySceneObject Definition => sceneObject;

    public Vector3 Position { get; private set; } = sceneObject.Position;
    public IRotation Rotation { get; } = sceneObject.Rotation;
    public Vector3 Scale { get; private set; } = sceneObject.Scale;

    public ISceneObjectInstance SetPosition(float x, float y, float z) {
      this.Position = new Vector3(x, y, z);
      return this;
    }

    public ISceneObjectInstance SetRotationRadians(float xRadians,
                                                   float yRadians,
                                                   float zRadians) {
      this.Rotation.SetRadians(
          xRadians,
          yRadians,
          zRadians
      );
      return this;
    }

    public ISceneObjectInstance SetRotationDegrees(float xDegrees,
                                                   float yDegrees,
                                                   float zDegrees) {
      this.Rotation.SetDegrees(
          xDegrees,
          yDegrees,
          zDegrees
      );
      return this;
    }

    public ISceneObjectInstance SetScale(float x, float y, float z) {
      this.Scale = new Vector3(x, y, z);
      return this;
    }

    public IReadOnlyList<ISceneModelInstance> Models { get; }
      = sceneObject.Models
                   .Select(m => new SceneModelInstanceImpl(m))
                   .ToArray();

    public void Tick() => sceneObject.TickHandler?.Invoke(this);


    private float viewerScale_ = 1;

    public float ViewerScale {
      get => this.viewerScale_;
      set {
        this.viewerScale_ = value;
        foreach (var model in this.Models) {
          model.ViewerScale = this.viewerScale_;
        }
      }
    }
  }
}