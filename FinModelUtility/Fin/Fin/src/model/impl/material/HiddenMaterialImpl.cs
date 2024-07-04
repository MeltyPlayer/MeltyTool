using System;
using System.Collections.Generic;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IHiddenMaterial AddHiddenMaterial() {
      var material = new HiddenMaterialImpl();
      this.materials_.Add(material);
      return material;
    }
  }

  private class HiddenMaterialImpl : BMaterialImpl, IHiddenMaterial {
    public override IEnumerable<ITexture> Textures { get; } =
      Array.Empty<ITexture>();
  }
}