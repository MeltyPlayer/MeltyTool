using System.Collections;

using fin.data.lazy;
using fin.model;
using fin.util.image;

namespace gx.impl;

using GxTextureBundle = (IGxTexture, ITexCoordGen?, ITextureMatrixInfo?);

public class GxLazyTextureDictionary(IModel model)
    : ILazyDictionary<GxTextureBundle, ITexture> {
  private readonly LazyDictionary<GxTextureBundle, ITexture> impl_ = new(
      texInfo => {
        var (bmdTexture, texCoordGen, texMatrix) = texInfo;

        // TODO: Share texture definitions between materials?
        var texture =
            model.MaterialManager.CreateTexture(bmdTexture.Image);
        var type = TransparencyTypeUtil.GetTransparencyType(bmdTexture.Image);

        texture.Name = bmdTexture.Name;
        texture.WrapModeU = bmdTexture.WrapModeS.ToFinWrapMode();
        texture.WrapModeV = bmdTexture.WrapModeT.ToFinWrapMode();
        texture.MagFilter =
            bmdTexture.MagTextureFilter.ToFinMagFilter();
        texture.MinFilter =
            bmdTexture.MinTextureFilter.ToFinMinFilter();
        texture.ColorType = bmdTexture.ColorType;

        var texGenSrc = texCoordGen.TexGenSrc;
        switch (texGenSrc) {
          case >= GxTexGenSrc.Tex0 and <= GxTexGenSrc.Tex7: {
            var texCoordIndex = texGenSrc - GxTexGenSrc.Tex0;
            texture.UvIndex = texCoordIndex;
            break;
          }
          case GxTexGenSrc.Normal: {
            texture.UvType = UvType.LINEAR;
            break;
          }
          default: {
            //Asserts.Fail($"Unsupported texGenSrc type: {texGenSrc}");
            texture.UvIndex = 0;
            break;
          }
        }

        var texMatrixType = texCoordGen.TexMatrix;
        if (texMatrixType != GxTexMatrix.Identity) {
          // TODO: handle special matrix types

          var texTranslation = texMatrix.Translation;
          var texScale = texMatrix.Scale;
          var texRotationRadians =
              texMatrix.Rotation / 32768f * MathF.PI;

          texture.SetOffset2d(texTranslation.X, texTranslation.Y)
                 .SetScale2d(texScale.X, texScale.Y)
                 .SetRotationRadians2d(texRotationRadians);
        }

        return texture;
      });

  public int Count => this.impl_.Count;
  public void Clear() => this.impl_.Clear();

  public IEnumerable<GxTextureBundle> Keys => this.impl_.Keys;
  public IEnumerable<ITexture> Values => this.impl_.Values;

  public bool ContainsKey(GxTextureBundle key) => this.impl_.ContainsKey(key);

  public ITexture this[GxTextureBundle key] {
    get => this.impl_[key];
    set => this.impl_[key] = value;
  }

  public bool Remove(GxTextureBundle key) => this.impl_.Remove(key);

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxTextureBundle Key, ITexture Value)> GetEnumerator()
    => this.impl_.GetEnumerator();
}