using System.Collections.Generic;

using fin.util.image;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class MaterialManagerImpl {
    public IStandardMaterial AddStandardMaterial() {
      var material = new StandardMaterialImpl();
      this.materials_.Add(material);
      return material;
    }
  }

  private class StandardMaterialImpl : BMaterialImpl, IStandardMaterial {
    private IReadOnlyTexture? diffuseTexture_;

    public override IEnumerable<IReadOnlyTexture> Textures {
      get {
        if (this.DiffuseTexture != null) {
          yield return this.DiffuseTexture;
        }

        if (this.MaskTexture != null) {
          yield return this.MaskTexture;
        }

        if (this.AmbientOcclusionTexture != null) {
          yield return this.AmbientOcclusionTexture;
        }

        if (this.NormalTexture != null) {
          yield return this.NormalTexture;
        }

        if (this.EmissiveTexture != null) {
          yield return this.EmissiveTexture;
        }

        if (this.SpecularTexture != null) {
          yield return this.SpecularTexture;
        }
      }
    }

    public IReadOnlyTexture? DiffuseTexture {
      get => this.diffuseTexture_;
      set {
        this.diffuseTexture_ = value;
        this.TransparencyType
            = value?.TransparencyType == TransparencyType.TRANSPARENT
                ? TransparencyType.TRANSPARENT
                : TransparencyType.MASK;
      }
    }

    public IReadOnlyTexture? MaskTexture { get; set; }
    public IReadOnlyTexture? AmbientOcclusionTexture { get; set; }
    public IReadOnlyTexture? NormalTexture { get; set; }
    public IReadOnlyTexture? EmissiveTexture { get; set; }
    public IReadOnlyTexture? SpecularTexture { get; set; }
  }
}