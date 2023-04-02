using fin.math;
using fin.schema.vector;
using schema.binary;
using schema.binary.attributes.ignore;


namespace cmb.schema.cmb {
  [BinarySchema]
  public partial class Bone : IBinaryConvertible {
    // Because only 12 bits are used, 4095 is the max bone count. (In
    // versions > OoT3D anyway)
    private ushort flags;

    [Ignore]
    public ushort id => (ushort) (this.flags & 0xFFF); // Get boneID

    // M-1:
    // Other 4 bits are probably more flags, but they're not used in any of
    // the three games
    // Though I probably missed a few compressed files. It's most likely
    // these flags below:
    // IsSegmentScaleCompensate, IsCompressible, IsNeededRendering, HasSkinningMatrix
    [Ignore]
    public bool hasSkinningMatrix => this.flags.GetBit(4);

    public short parentId;

    public Vector3f scale { get; } = new();
    public Vector3f rotation { get; } = new();
    public Vector3f translation { get; } = new();

    [Ignore]
    private bool HasUnk => CmbHeader.Version > CmbVersion.OCARINA_OF_TIME_3D;

    [IfBoolean(nameof(HasUnk))]
    public uint? unk0;
  }
}