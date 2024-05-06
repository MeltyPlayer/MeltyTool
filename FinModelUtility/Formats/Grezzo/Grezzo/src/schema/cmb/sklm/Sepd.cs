using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb.sklm {
  [BinarySchema]
  public partial class Sepd : IBinaryConvertible {
    private readonly string magic_ = "sepd";

    public uint chunkSize;

    [WLengthOfSequence(nameof(primitiveSetOffsets_))]
    [WLengthOfSequence(nameof(primitiveSets))]
    private ushort primSetCount_;

    /**
      Bit Flags: (HasTangents was added in versions > OoT:3D (aka 6))
         HasPosition : 00000001
         HasNormals  : 00000010
         HasTangents : 00000100 (MM3D/LM3D/EO only)
         HasColors   : 00000100
         HasUV0      : 00001000
         HasUV1      : 00010000
         HasUV2      : 00100000
         HasIndices  : 01000000
         HasWeights  : 10000000
     */
    public ushort vertFlags;

    public float[] meshCenter { get; } = new float[3];
    public float[] positionOffset { get; } = new float[3];

    [Skip]
    private bool hasMinAndMax_ => CmbHeader.Version.SupportsMinAndMaxInSepd();

    // Min coordinate of the shape
    [RIfBoolean(nameof(hasMinAndMax_))]
    [SequenceLengthSource(3)]
    public float[]? min { get; private set; }

    // Max coordinate of the shape
    [RIfBoolean(nameof(hasMinAndMax_))]
    [SequenceLengthSource(3)]
    public float[]? max { get; private set; }

    public readonly VertexAttribute position = new();
    public readonly VertexAttribute normal = new();

    [Skip]
    private bool hasTangents_ => CmbHeader.Version.SupportsInSepd();

    [RIfBoolean(nameof(hasTangents_))]
    public VertexAttribute? tangents;

    public readonly VertexAttribute color = new();
    public readonly VertexAttribute uv0 = new();
    public readonly VertexAttribute uv1 = new();
    public readonly VertexAttribute uv2 = new();
    public readonly VertexAttribute bIndices = new();
    public readonly VertexAttribute bWeights = new();

    // How many weights each vertex has for this shape
    public ushort boneDimensions;

    /**
      Note: Constant values are set in "VertexAttribute" (Use constants instead of an array to save space, assuming all values are the same)
        #Bit Flags:
        # PositionUseConstant : 00000001
        # NormalsUseConstant  : 00000010
        # TangentsUseConstant : 00000100 (MM3D/LM3D/EO only)
        # ColorsUseConstant   : 00000100
        # UV0UseConstant      : 00001000
        # UV1UseConstant      : 00010000
        # UV2UseConstant      : 00100000
        # IndicesUseConstant  : 01000000
        # WeightsUseConstant  : 10000000
    */
    public ushort constantFlags;

    [RSequenceLengthSource(nameof(primSetCount_))]
    private short[] primitiveSetOffsets_;

    [Align(4)]
    [RSequenceLengthSource(nameof(primSetCount_))]
    public PrimitiveSet[] primitiveSets { get; set; }
  }
}