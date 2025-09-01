using System;

using f3dzex2.combiner;

using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public class MaterialParams : IEquatable<MaterialParams> {
  public TextureParams? TextureParams0 { get; set; } = new();
  public TextureParams? TextureParams1 { get; set; } = new();

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams? CombinerCycleParams1 { get; set; }

  public CullingMode CullingMode { get; set; }

  private int? hashCode_;

  public override int GetHashCode()
    => this.hashCode_ ??=
        FluentHash.Start()
                  .With(this.TextureParams0)
                  .With(this.TextureParams1)
                  .With(this.CombinerCycleParams0)
                  .With(this.CombinerCycleParams1)
                  .With(this.CullingMode);

  public bool Equals(MaterialParams other)
    => IEquatable<TextureParams>.Equals(this.TextureParams0,
                                        other.TextureParams0) &&
       IEquatable<TextureParams>.Equals(this.TextureParams1,
                                        other.TextureParams1) &&
       this.CombinerCycleParams0.Equals(other.CombinerCycleParams0) &&
       this.CombinerCycleParams1.Equals(other.CombinerCycleParams1) &&
       this.CullingMode == other.CullingMode;
}