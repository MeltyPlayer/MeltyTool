using schema.binary;
using schema.binary.attributes;

namespace sm64ds.schema.bca;

/// <summary>
///   Shamelessly stolen from:
///   https://kuribo64.net/get.php?id=KNIJfjt4ZG2tcgDM
///   https://github.com/Arisotura/SM64DSe/blob/master/SM64DSFormats/BCA.cs
/// </summary>
[BinarySchema]
public partial class BoneAnimationData : IBinaryDeserializable, IChildOf<Bca> {
  public Bca Parent { get; set; }

  public AnimationDescriptor ScaleX { get; } = new();
  public AnimationDescriptor ScaleY { get; } = new();
  public AnimationDescriptor ScaleZ { get; } = new();

  public AnimationDescriptor RotationX { get; } = new();
  public AnimationDescriptor RotationY { get; } = new();
  public AnimationDescriptor RotationZ { get; } = new();

  public AnimationDescriptor TranslationX { get; } = new();
  public AnimationDescriptor TranslationY { get; } = new();
  public AnimationDescriptor TranslationZ { get; } = new();

  [Skip]
  public (int, float)[] ScaleXValues { get; set; }

  [Skip]
  public (int, float)[] ScaleYValues { get; set; }

  [Skip]
  public (int, float)[] ScaleZValues { get; set; }

  [Skip]
  public (int, float)[] RotationXValues { get; set; }

  [Skip]
  public (int, float)[] RotationYValues { get; set; }

  [Skip]
  public (int, float)[] RotationZValues { get; set; }

  [Skip]
  public (int, float)[] TranslationXValues { get; set; }

  [Skip]
  public (int, float)[] TranslationYValues { get; set; }

  [Skip]
  public (int, float)[] TranslationZValues { get; set; }

  [ReadLogic]
  private void ReadValues_(IBinaryReader br) {
    this.ScaleXValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.ScaleValuesOffset,
        this.ScaleX);
    this.ScaleYValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.ScaleValuesOffset,
        this.ScaleY);
    this.ScaleZValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.ScaleValuesOffset,
        this.ScaleZ);

    this.RotationXValues = this.ReadValuesForDescriptor4_12Rotation_(
        br,
        this.Parent.RotationValuesOffset,
        this.RotationX);
    this.RotationYValues = this.ReadValuesForDescriptor4_12Rotation_(
        br,
        this.Parent.RotationValuesOffset,
        this.RotationY);
    this.RotationZValues = this.ReadValuesForDescriptor4_12Rotation_(
        br,
        this.Parent.RotationValuesOffset,
        this.RotationZ);

    this.TranslationXValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.TranslationValuesOffset,
        this.TranslationX);
    this.TranslationYValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.TranslationValuesOffset,
        this.TranslationY);
    this.TranslationZValues = this.ReadValuesForDescriptor20_12Float_(
        br,
        this.Parent.TranslationValuesOffset,
        this.TranslationZ);
  }

  private (int, float)[] ReadValuesForDescriptor20_12Float_(
      IBinaryReader br,
      uint offset,
      AnimationDescriptor descriptor) {
    this.GetLengthOfChannel_(descriptor, out var length, out var increment);
    return br.SubreadAt(offset + descriptor.FirstIndex * 4,
                        () => {
                          var values = new (int, float)[length];
                          for (int i = 0; i < length; i++) {
                            values[i] = (
                                i * increment, br.ReadInt32() / 4096.0f);
                          }

                          return values;
                        });
  }

  private (int, float)[] ReadValuesForDescriptor4_12Rotation_(
      IBinaryReader br,
      uint offset,
      AnimationDescriptor descriptor) {
    this.GetLengthOfChannel_(descriptor, out var length, out var increment);
    return br.SubreadAt(
        offset + descriptor.FirstIndex * 2,
        () => {
          var values = new (int, float)[length];
          for (int i = 0; i < length; i++) {
            values[i] = (i * increment, br.ReadInt16() * MathF.PI / 2048.0f);
          }

          return values;
        });
  }

  private void GetLengthOfChannel_(AnimationDescriptor descriptor,
                                   out int length,
                                   out int increment) {
    if (!descriptor.IncrementOffsetsEachFrame) {
      length = 1;
      increment = 0;
      return;
    }

    if (descriptor.InterpolateOddFrames) {
      length = this.Parent.NumFrames >> 1 + 1;
      increment = 2;
      return;
    }

    length = this.Parent.NumFrames;
    increment = 1;
  }
}