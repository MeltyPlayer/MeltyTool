using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

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
          = new ReadOnlyCollection<IReadOnlyTexture>([texture]);

      this.UpdateTransparencyType_();
    }

    public IReadOnlyTexture Texture { get; }
    public override IEnumerable<IReadOnlyTexture> Textures { get; }

    public Color? DiffuseColor {
      get;
      set {
        field = value;
        this.UpdateTransparencyType_();
      }
    }

    private void UpdateTransparencyType_() {
      if (this.Texture.TransparencyType == TransparencyType.TRANSPARENT ||
          (this.DiffuseColor?.A ?? 255) < 255) {
        this.TransparencyType = TransparencyType.TRANSPARENT;
      } else {
        this.TransparencyType = this.Texture.TransparencyType;
      }
    }
  }
}