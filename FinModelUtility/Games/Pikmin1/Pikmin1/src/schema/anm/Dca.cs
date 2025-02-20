using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.anm {
  [BinarySchema]
  public partial class Dca : IDcx {
    [WSizeOfMemberInBytes(nameof(AnimationData))]
    private uint animationLength_;

    [StringLengthSource(SchemaIntegerType.INT32)]
    public string Name { get; set; }
    
    public IDcxAnimationData AnimationData { get; } = new DcaAnimationData();
   
    public override string ToString() => this.Name;
  }


  [BinarySchema]
  public partial class DcaAnimationData : IDcxAnimationData {
    [WLengthOfSequence(nameof(JointDataList))]
    public uint jointCount_ { get; private set; }
    public uint FrameCount { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] ScaleValues { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] RotationValues { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] PositionValues { get; set; }

    [RSequenceLengthSource(nameof(jointCount_))]
    public DcaJointData[] JointDataList { get; set; }

    [Skip]
    IDcxJointData[] IDcxAnimationData.JointDataList {
      get => this.JointDataList;
      set => this.JointDataList = value as DcaJointData[];
    }
  }

  [BinarySchema]
  public partial class DcaJointData : IDcxJointData {
    public int JointIndex { get; set; }
    public int ParentIndex { get; set; }

    public IDcxAxes ScaleAxes { get; } = new DcaAxes();
    public IDcxAxes RotationAxes { get; } = new DcaAxes();
    public IDcxAxes PositionAxes { get; } = new DcaAxes();
  }

  [BinarySchema]
  public partial class DcaAxes : IDcxAxes {
    public IDcxAxis[] Axes { get; } = [
        new DcaAxis(),
        new DcaAxis(),
        new DcaAxis()
    ];
  }

  [BinarySchema]
  public partial class DcaAxis : IDcxAxis {
    public int FrameCount { get; set; }
    public int FrameOffset { get; set; }
  }
}