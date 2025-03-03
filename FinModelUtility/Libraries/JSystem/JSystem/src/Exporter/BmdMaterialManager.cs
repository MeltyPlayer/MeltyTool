using System.Collections.Generic;
using System.Linq;

using fin.model;

using gx;
using gx.impl;

using jsystem.GCN;
using jsystem.schema.jutility.bti;


namespace jsystem.exporter;

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
    var lazyTextureMap = new GxLazyTextureDictionary(model);
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