using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace fin.scene.instance {
  public partial class SceneInstanceImpl {
    private class SceneAreaInstanceImpl(IReadOnlySceneArea sceneArea)
        : ISceneAreaInstance {
      ~SceneAreaInstanceImpl() => ReleaseUnmanagedResources_();

      public void Dispose() {
        ReleaseUnmanagedResources_();
        GC.SuppressFinalize(this);
      }

      private void ReleaseUnmanagedResources_() {
        this.CustomSkyboxObject?.Dispose();
        foreach (var obj in this.Objects) {
          obj.Dispose();
        }
      }

      public IReadOnlyList<ISceneObjectInstance> Objects { get; } = sceneArea
          .Objects
          .Select(o => new SceneObjectInstanceImpl(o))
          .ToArray();

      public void Tick() {
        foreach (var obj in this.Objects) {
          obj.Tick();
        }
      }

      private float viewerScale_ = 1;

      public float ViewerScale {
        get => this.viewerScale_;
        set {
          this.viewerScale_ = value;
          foreach (var obj in this.Objects) {
            obj.ViewerScale = this.viewerScale_;
          }
        }
      }

      public Color? BackgroundColor => sceneArea.BackgroundColor;

      public ISceneObjectInstance? CustomSkyboxObject { get; }
        = sceneArea.CustomSkyboxObject != null
            ? new SceneObjectInstanceImpl(sceneArea.CustomSkyboxObject)
            : null;
    }
  }
}