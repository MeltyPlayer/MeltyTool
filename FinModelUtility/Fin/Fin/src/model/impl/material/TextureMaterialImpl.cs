using System.Collections.Generic;
using System.Collections.ObjectModel;

using fin.util.image;

namespace fin.model.impl;

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

      if (texture.TransparencyType == TransparencyType.TRANSPARENT) {
        this.TransparencyType = TransparencyType.TRANSPARENT;
      }
    }

    public IReadOnlyTexture Texture { get; }
    public override IEnumerable<IReadOnlyTexture> Textures { get; }
  }
}