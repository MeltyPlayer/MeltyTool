using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.vatr;

[BinarySchema]
[LocalPositions]
public partial class Vatr : IBinaryConvertible {
  private readonly string magic_ = "vatr";

  public uint chunkSize;
  // i.e., vertex count of model
  public uint maxIndex;

  // Basically just used to get each attribute into it's own byte[] (We won't
  // be doing that here)
  public readonly AttributeSlice position = new();
  public readonly AttributeSlice normal = new();

  [Skip]
  private bool hasTangent_ 
    => CmbHeader.Version > Version.OCARINA_OF_TIME_3D;

  [RIfBoolean(nameof(hasTangent_))]
  public AttributeSlice? tangent;
    
  public readonly AttributeSlice color = new();
  public readonly AttributeSlice uv0 = new();
  public readonly AttributeSlice uv1 = new();
  public readonly AttributeSlice uv2 = new();
  public readonly AttributeSlice bIndices = new();
  public readonly AttributeSlice bWeights = new();
}