using System.Collections.Generic;

using fin.model;
using fin.model.impl;

namespace fin.scene {
  public partial class SceneImpl : IScene {
    private readonly List<ISceneArea> areas_ = [];
    public IReadOnlyList<ISceneArea> Areas => this.areas_;

    public ISceneArea AddArea() {
      var area = new SceneAreaImpl();
      this.areas_.Add(area);
      return area;
    }

    public ILighting? Lighting { get; private set; }
    public ILighting CreateLighting() => this.Lighting = new LightingImpl();
  }
}