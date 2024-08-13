using fin.schema.vector;

using schema.binary;
using schema.binary.attributes;


namespace jsystem.schema.j3dgraph.bmd.jnt1;

public enum JointType : ushort {
  /// <summary>
  ///   This seems to be associated with manually animated joints, that is
  ///   joints that receive custom matrices at runtime from the code.
  ///
  ///   This is most often used on the model root, and surfaces that always
  ///   face the camera.
  /// </summary>
  MANUAL,

  /// <summary>
  ///   This seems to be associated with automatically animated joints, that
  ///   is joints that automatically receive matrices based on BCA/BCK
  ///   animations.
  /// </summary>
  AUTOMATED,

  /// <summary>
  ///   This seems to be associated with joints like hands and feet, where
  ///   things may be attached. It seems most likely that this marks an
  ///   attachment point for submodels, such as held items or particles.
  /// </summary>
  ATTACHMENT_POINT,
}

[BinarySchema]
public partial class Jnt1Entry : IBinaryConvertible {
  public JointType JointType { get; set; }

  [IntegerFormat(SchemaIntegerType.BYTE)]
  public bool IgnoreParentScale { get; set; }


  private readonly byte padding1_ = byte.MaxValue;
  public Vector3f Scale { get; } = new();
  public Vector3s Rotation { get; } = new();

  private readonly ushort padding2_ = ushort.MaxValue;
  public Vector3f Translation { get; } = new();

  public float BoundingSphereDiameter { get; set; }
  public Vector3f BoundingBoxMin { get; } = new();
  public Vector3f BoundingBoxMax { get; } = new();
}