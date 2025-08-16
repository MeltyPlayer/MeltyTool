using System.Drawing;

using f3dzex2.combiner;
using f3dzex2.displaylist.opcodes;
using f3dzex2.rsp;

using fin.math;
using fin.math.matrix.four;
using fin.model;

namespace f3dzex2.image;

public enum N64UvType : byte {
  STANDARD,
  SPHERICAL,
  LINEAR
}

public interface IRsp {
  GeometryMode GeometryMode { get; set; }

  ushort TexScaleXShort { get; set; }
  ushort TexScaleYShort { get; set; }

  float TexScaleXFloat { get; }
  float TexScaleYFloat { get; }

  IBoneMapper BoneMapper { get; }
  IReadOnlyBoneWeights? ActiveBoneWeights { get; set; }

  Color EnvironmentColor { get; set; }
  Color PrimColor { get; set; }

  N64UvType UvType {
    get => (N64UvType) ((uint) this.GeometryMode).ExtractFromRight(18, 2);
    set => this.GeometryMode = (GeometryMode) ((uint) this.GeometryMode).SetFromRight(18, 2, (uint) value);
  }
}

public class Rsp : IRsp {
  private float texScaleXFloat_ = 1;
  private float texScaleYFloat_ = 1;
  private ushort texScaleXShort_ = 0xFFFF;
  private ushort texScaleYShort_ = 0xFFFF;

  public GeometryMode GeometryMode { get; set; } = (GeometryMode) 0x22205;

  public ushort TexScaleXShort {
    get => this.texScaleXShort_;
    set {
        this.texScaleXShort_ = value;
        this.texScaleXFloat_ =
            (float) BitLogic.ConvertBinaryFractionToDouble(value);
      }
  }

  public ushort TexScaleYShort {
    get => this.texScaleYShort_;
    set {
        this.texScaleYShort_ = value;
        this.texScaleYFloat_ =
            (float) BitLogic.ConvertBinaryFractionToDouble(value);
      }
  }

  public float TexScaleXFloat => this.texScaleXFloat_;
  public float TexScaleYFloat => this.texScaleYFloat_;

  public IBoneMapper BoneMapper { get; } = new BoneMapper();
  public IReadOnlyBoneWeights? ActiveBoneWeights { get; set; }

  public Color EnvironmentColor { get; set; }
  public Color PrimColor { get; set; }

  public CombinerCycleParams CombinerCycleParams0 { get; set; }
  public CombinerCycleParams CombinerCycleParams1 { get; set; }
}