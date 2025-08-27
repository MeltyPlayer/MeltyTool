using System;

using f3dzex2.combiner;

using fin.model;
using fin.util.hash;

namespace f3dzex2.image;

public struct MaterialParams : IEquatable<MaterialParams> {
  public MaterialParams() { }

  public TextureParams? TextureParams0 { get; set; } = new();
  public TextureParams? TextureParams1 { get; set; } = new();

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams? CombinerCycleParams1 { get; set; }

  public CullingMode CullingMode { get; set; }

  public override int GetHashCode() => FluentHash.Start()
                                                 .With(this.TextureParams0 ??
                                                   default)
                                                 .With(this.TextureParams1 ??
                                                   default)
                                                 .With(
                                                     this.CombinerCycleParams0)
                                                 .With(
                                                     this
                                                         .CombinerCycleParams1 ??
                                                     default)
                                                 .With(this.CullingMode);

  public bool Equals(MaterialParams other)
    => this.TextureParams0.Equals(other.TextureParams0) &&
       this.TextureParams1.Equals(other.TextureParams1) &&
       this.CombinerCycleParams0.Equals(other.CombinerCycleParams0) &&
       this.CombinerCycleParams1.Equals(other.CombinerCycleParams1) &&
       this.CullingMode == other.CullingMode;
}