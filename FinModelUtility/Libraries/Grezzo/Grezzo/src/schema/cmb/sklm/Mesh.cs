using System;

using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm;

[BinarySchema]
public sealed partial class Mesh : IBinaryConvertible, IChildOf<Mshs> {
  public Mshs Parent { get; set; }

  public ushort shapeIndex;
  public byte materialIndex;
  public byte id;

  [Skip]
  private int unknownLength_ => this.Parent.Parent.Version switch {
      Version.OCARINA_OF_TIME_3D => 0,
      Version.MAJORAS_MASK_3D    => 0x8,
      Version.EVER_OASIS         => 0xC,
      Version.LUIGIS_MANSION_3D  => 0x54,
      _                          => throw new ArgumentOutOfRangeException()
  };

  [Unknown]
  [RSequenceLengthSource(nameof(unknownLength_))]
  private byte[] unknown_;
}