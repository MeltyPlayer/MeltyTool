using System;

using f3dzex2.combiner;

using fin.image;
using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public sealed class MaterialParams : IEquatable<MaterialParams> {
  public TextureParams? TextureParams0 { get; set; } = new();
  public TextureParams? TextureParams1 { get; set; } = new();
  
  public IReadOnlyTexture? HardcodedTexture0 { get; set; }
  public IReadOnlyTexture? HardcodedTexture1 { get; set; }

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams? CombinerCycleParams1 { get; set; }

  public CullingMode CullingMode { get; set; }

  private int? hashCode_;

  public override int GetHashCode() {
    if (this.hashCode_ == null) {
      var fluentHash = FluentHash.Start();

      if (this.HardcodedTexture0 != null) {
        fluentHash.With(this.HardcodedTexture0);
      } else {
        fluentHash.With(this.TextureParams0);
      }

      if (this.HardcodedTexture1 != null) {
        fluentHash.With(this.HardcodedTexture1);
      } else {
        fluentHash.With(this.TextureParams1);
      }

      this.hashCode_ = fluentHash.With(this.CombinerCycleParams0)
                .With(this.CombinerCycleParams1)
                .With(this.CullingMode);
    }

    return this.hashCode_.Value;
  }

  public bool Equals(MaterialParams other) {
    var areEqual = true;

    if (this.HardcodedTexture0 != null) {
      areEqual &= IEquatable<IReadOnlyImage>.Equals(
          this.HardcodedTexture0,
          other.HardcodedTexture0);
    } else {
      areEqual &= IEquatable<TextureParams>.Equals(
          this.TextureParams0,
          other.TextureParams0);
    }

    if (this.HardcodedTexture1 != null) {
      areEqual &= IEquatable<IReadOnlyImage>.Equals(
          this.HardcodedTexture1,
          other.HardcodedTexture1);
    } else {
      areEqual &= IEquatable<TextureParams>.Equals(
          this.TextureParams1,
          other.TextureParams1);
    }
    
    return areEqual &&
           IEquatable<CombinerCycleParams>.Equals(
               this.CombinerCycleParams0,
               other.CombinerCycleParams0) &&
           IEquatable<CombinerCycleParams>.Equals(
               this.CombinerCycleParams1,
               other.CombinerCycleParams1) &&
           this.CullingMode == other.CullingMode;
  }
}