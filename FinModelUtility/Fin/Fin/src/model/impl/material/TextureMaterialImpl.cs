using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace fin.model.impl {
  public partial class ModelImpl<TVertex> {
    private partial class MaterialManagerImpl {
      public ITextureMaterial AddTextureMaterial(IReadOnlyTexture texture) {
        var material = new TextureMaterialImpl(texture);
        this.materials_.Add(material);
        return material;
      }
    }

    private class TextureMaterialImpl : BMaterialImpl, ITextureMaterial {
      public TextureMaterialImpl(IReadOnlyTexture texture) {
        this.Texture = texture;
        this.Textures
            = new ReadOnlyCollection<IReadOnlyTexture>(new[] { texture });
      }

      public IReadOnlyTexture Texture { get; }
      public override IEnumerable<IReadOnlyTexture> Textures { get; }
    }
  }
}