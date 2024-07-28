using System;
using System.Collections.Generic;
using System.Linq;

using fin.model;

namespace fin.scene.instance;

public partial class SceneInstanceImpl(IReadOnlyScene scene)
    : ISceneInstance {
  ~SceneInstanceImpl() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var area in this.Areas) {
      area.Dispose();
    }
  }

  public IReadOnlyScene Definition => scene;

  public IReadOnlyList<ISceneAreaInstance> Areas { get; }
    = scene.Areas
           .Select(a => new SceneAreaInstanceImpl(a))
           .ToArray();

  public void Tick() {
    foreach (var area in this.Areas) {
      area.Tick();
    }
  }

  // TODO: Clone lighting here instead?

  public IReadOnlyLighting? Lighting => scene.Lighting;


  private float viewerScale_ = 1;

  public float ViewerScale {
    get => this.viewerScale_;
    set {
      this.viewerScale_ = value;
      foreach (var area in this.Areas) {
        area.ViewerScale = this.viewerScale_;
      }
    }
  }
}