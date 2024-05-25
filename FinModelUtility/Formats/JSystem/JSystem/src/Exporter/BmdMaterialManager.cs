using System;
using System.Collections.Generic;
using System.Linq;

using fin.data.lazy;
using fin.model;
using fin.util.image;

using gx;

using jsystem.GCN;
using jsystem.schema.jutility.bti;

namespace jsystem.exporter {
  public class BmdMaterialManager {
    private readonly BMD bmd_;
    private readonly IList<IGxTexture> textures_;
    private readonly IList<GxFixedFunctionMaterial> materials_;

    public BmdMaterialManager(
        IModel model,
        BMD bmd,
        IList<(string, Bti)>? pathsAndBtis = null) {
      this.bmd_ = bmd;

      var tex1 = bmd.TEX1.Data;
      this.textures_ = tex1.TextureHeaders.Select((textureHeader, i) => {
                             var textureName = tex1.StringTable[i];

                             return (IGxTexture) new BmdGxTexture(
                                 textureName,
                                 textureHeader,
                                 pathsAndBtis);
                           })
                           .ToList();

      this.materials_ = this.GetMaterials_(model, bmd);
    }

    public GxFixedFunctionMaterial Get(int entryIndex)
      => this.materials_[this.bmd_.MAT3.MaterialEntryIndieces[entryIndex]];

    private IList<GxFixedFunctionMaterial>
        GetMaterials_(IModel model, BMD bmd) {
      var lazyTextureMap =
          new LazyDictionary<(IGxTexture, ITexCoordGen?, ITextureMatrixInfo?),
              ITexture>(
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

      return bmd.MAT3.MaterialEntries.Select(
                    (_, i) => new GxFixedFunctionMaterial(
                        model,
                        model.MaterialManager,
                        bmd.MAT3.PopulatedMaterials[i],
                        this.textures_,
                        lazyTextureMap))
                .ToList();
    }
  }
}