using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    private partial class MaterialManagerImpl {
      public IColorMaterial AddColorMaterial(Color color) {
        var material = new ColorMaterialImpl(color);
        this.materials_.Add(material);
        return material;
      }
    }

    private class ColorMaterialImpl : BMaterialImpl, IColorMaterial {
      public ColorMaterialImpl(Color color) {
        this.Color = color;
      }

      public Color Color { get; set; }

      public override IEnumerable<ITexture> Textures
        => Enumerable.Empty<ITexture>();
    }
  }
}