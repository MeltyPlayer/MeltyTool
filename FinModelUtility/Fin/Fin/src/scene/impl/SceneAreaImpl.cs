using System.Collections.Generic;
using System.Drawing;

using fin.image;

namespace fin.scene;

public partial class SceneImpl {
  private class SceneAreaImpl : ISceneArea {
    private readonly List<ISceneObject> objects_ = [];

    public IReadOnlyList<ISceneObject> Objects => this.objects_;

    public ISceneObject AddObject() {
      var obj = new SceneObjectImpl();
      this.objects_.Add(obj);
      return obj;
    }

    public Color? BackgroundColor { get; set; }
    public IReadOnlyImage? BackgroundImage { get; set; }
    public float BackgroundImageScale { get; set; } = 1;
    public ISceneObject? CustomSkyboxObject { get; set; }

    public ISceneObject CreateCustomSkyboxObject()
      => this.CustomSkyboxObject = new SceneObjectImpl();
  }
}