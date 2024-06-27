using System;
using System.Collections.Generic;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public INullMaterial AddNullMaterial() {
      var material = new NullMaterialImpl();
      this.materials_.Add(material);
      return material;
    }
  }

  private class NullMaterialImpl : BMaterialImpl, INullMaterial {
    public override IEnumerable<ITexture> Textures { get; } =
      Array.Empty<ITexture>();
  }
}