using System.Collections;

using fin.data.lazy;
using fin.model;

namespace gx.impl;

public interface IGxTextureBundle {
  GxTexMap TexMap { get; }
  IGxTexture GxTexture { get; }
  ITexCoordGen? TexCoordGen { get; }
  ITextureMatrixInfo? TextureMatrixInfo { get; }
}

public record GxTextureBundle(
    GxTexMap TexMap,
    IGxTexture GxTexture,
    ITexCoordGen? TexCoordGen,
    ITextureMatrixInfo? TextureMatrixInfo) : IGxTextureBundle;

public record GxTextureBundle<TDiscriminator>(
    GxTexMap TexMap,
    IGxTexture GxTexture,
    ITexCoordGen? TexCoordGen,
    ITextureMatrixInfo? TextureMatrixInfo,
    TDiscriminator? Discriminator) : IGxTextureBundle;

public class GxLazyTextureDictionary(IModel model)
    : GxLazyTextureDictionary<int, int>(model);

public class GxLazyTextureDictionary<TState, TDiscriminator>
    : ILazyDictionary<IGxTextureBundle, ITexture> {
  private readonly LazyDictionary<GxTextureBundle<TDiscriminator>, ITexture>
      impl_;

  private Func<IGxTextureBundle, TState, TDiscriminator>? getDiscriminator_;

  public GxLazyTextureDictionary(
      IModel model,
      Func<IGxTextureBundle, TState, TDiscriminator>? getDiscriminator = null,
      Action<IGxTextureBundle, TState, ITexture>? handleNewTexture = null) {
    this.getDiscriminator_ = getDiscriminator;
    this.impl_ = new(
        (dict, texInfo) => {
          var (_, gxTexture, texCoordGen, texMatrix, _) = texInfo;

          // TODO: Share texture definitions between materials?
          var texture =
              model.MaterialManager.CreateTexture(gxTexture.MipmapImages);

          texture.Name = gxTexture.Name ?? $"texture{dict.Count}";
          texture.WrapModeU = gxTexture.WrapModeS.ToFinWrapMode();
          texture.WrapModeV = gxTexture.WrapModeT.ToFinWrapMode();

          texture.MagFilter =
              gxTexture.MagTextureFilter.ToFinMagFilter();
          texture.MinFilter =
              gxTexture.MinTextureFilter.ToFinMinFilter();
          texture.ColorType = gxTexture.ColorType;

          texture.MinLod = gxTexture.MinLod;
          texture.MaxLod = gxTexture.MaxLod;
          texture.LodBias = gxTexture.LodBias;

          var texGenSrc = texCoordGen.TexGenSrc;
          switch (texGenSrc) {
            case >= GxTexGenSrc.Tex0 and <= GxTexGenSrc.Tex7: {
              var texCoordIndex = texGenSrc - GxTexGenSrc.Tex0;
              texture.UvIndex = texCoordIndex;
              break;
            }
            case GxTexGenSrc.Normal: {
              texture.UvType = UvType.SPHERICAL;
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

            var texCenter = texMatrix.Center;
            var texTranslation = texMatrix.Translation;
            var texScale = texMatrix.Scale;
            var texRotationRadians =
                texMatrix.Rotation / 32768f * MathF.PI;

            texture
                .SetCenter2d(texCenter.X, texCenter.Y)
                .SetTranslation2d(texTranslation.X, texTranslation.Y)
                .SetScale2d(texScale.X, texScale.Y)
                .SetRotationRadians2d(texRotationRadians);
          }

          handleNewTexture?.Invoke(texInfo, this.State!, texture);

          return texture;
        });
  }

  public TState State { get; set; }

  public int Count => this.impl_.Count;
  public void Clear() => this.impl_.Clear();

  public IEnumerable<IGxTextureBundle> Keys => this.impl_.Keys;
  public IEnumerable<ITexture> Values => this.impl_.Values;

  public bool ContainsKey(IGxTextureBundle key)
    => this.impl_.ContainsKey(this.GetKey_(key));

  public ITexture this[IGxTextureBundle key] {
    get => this.impl_[this.GetKey_(key)];
    set => this.impl_[this.GetKey_(key)] = value;
  }

  public bool Remove(IGxTextureBundle key)
    => this.impl_.Remove(this.GetKey_(key));

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(IGxTextureBundle Key, ITexture Value)> GetEnumerator()
    => this.impl_.Select(tuple => ((IGxTextureBundle) tuple.Key, tuple.Value))
           .GetEnumerator();

  private GxTextureBundle<TDiscriminator> GetKey_(
      IGxTextureBundle gxTextureBundle)
    => new(gxTextureBundle.TexMap,
           gxTextureBundle.GxTexture,
           gxTextureBundle.TexCoordGen,
           gxTextureBundle.TextureMatrixInfo,
           this.getDiscriminator_ != null
               ? this.getDiscriminator_(gxTextureBundle, this.State)
               : default);
}