using fin.schema.matrix;
using fin.schema.vector;
using fin.util.enums;

using schema.binary;
using schema.binary.attributes;

namespace nitro.schema.nsbmd;

[Flags]
public enum ObjectFlags : ushort {
  NO_TRANSLATION = 1 << 0,
  NO_ROTATION = 1 << 1,
  NO_SCALE = 1 << 2,
  USE_PIVOT_MATRIX = 1 << 3,
}

/// <summary>
///   Shamelessly copied from:
///   https://github.com/scurest/apicula/blob/07c4d8facdcb015d118bf26a29d37c8b41021bfd/src/nitro/model.rs#L329
/// </summary>
[BinarySchema]
public partial class Object : IBinaryDeserializable {
  public ObjectFlags Flags { get; set; }

  [NumberFormat(SchemaNumberType.UN16)]
  public float M0 { get; set; }


  [RIfBoolean(nameof(HasTranslation))]
  public Vector3f? Translation { get; set; }

  [ReadLogic]
  public void ReadRotation_(IBinaryReader br) {
      if (this.Flags.CheckFlag(ObjectFlags.USE_PIVOT_MATRIX)) {
        // TODO: Implement
      } else if (!this.Flags.CheckFlag(ObjectFlags.NO_ROTATION)) {
        this.Rotation ??= new Matrix3x3f();

        Span<float> m1To9 = stackalloc float[8];
        br.ReadUn16s(m1To9);

        this.Rotation[0, 0] = this.M0;
        for (var i = 1; i < 9; i++) {
          this.Rotation[i % 3, i / 3] = m1To9[i - 1];
        }
      } else {
        this.Rotation = null;
      }
    }

  [RIfBoolean(nameof(HasScale))]
  public Vector3f? Scale { get; set; }


  [Skip]
  public Matrix3x3f? Rotation { get; set; }


  [Skip]
  public bool HasTranslation
    => !this.Flags.CheckFlag(ObjectFlags.NO_TRANSLATION);

  [Skip]
  public bool HasScale
    => !this.Flags.CheckFlag(ObjectFlags.NO_SCALE);
}