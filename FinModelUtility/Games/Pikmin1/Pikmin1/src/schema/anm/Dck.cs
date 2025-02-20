using fin.schema;

using schema.binary;
using schema.binary.attributes;

namespace pikmin1.schema.anm {
  [BinarySchema]
  public partial class Dck : IDcx {
    [WSizeOfMemberInBytes(nameof(AnimationData))]
    private uint animationLength_;

    [StringLengthSource(SchemaIntegerType.INT32)]
    public string Name { get; set; }

    public IDcxAnimationData AnimationData { get; } = new DckAnimationData();

    public override string ToString() => this.Name;
  }


  [BinarySchema]
  public partial class DckAnimationData : IDcxAnimationData {
    [WLengthOfSequence(nameof(JointDataList))]
    private uint jointCount_;

    public uint FrameCount { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] ScaleValues { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] RotationValues { get; set; }

    [SequenceLengthSource(SchemaIntegerType.INT32)]
    public float[] PositionValues { get; set; }

    [RSequenceLengthSource(nameof(jointCount_))]
    public DckJointData[] JointDataList { get; set; }

    [Skip]
    IDcxJointData[] IDcxAnimationData.JointDataList {
      get => this.JointDataList;
      set => this.JointDataList = value as DckJointData[];
    }
  }

  [BinarySchema]
  public partial class DckJointData : IDcxJointData {
    public int JointIndex { get; set; }
    public int ParentIndex { get; set; }

    public IDcxAxes ScaleAxes { get; } = new DckAxes();
    public IDcxAxes RotationAxes { get; } = new DckAxes();
    public IDcxAxes PositionAxes { get; } = new DckAxes();
  }

  [BinarySchema]
  public partial class DckAxes : IDcxAxes {
    public IDcxAxis[] Axes { get; } = [
        new DckAxis(), new DckAxis(), new DckAxis()
    ];
  }

  [BinarySchema]
  public partial class DckAxis : IDcxAxis {
    public int FrameCount { get; set; }
    public int FrameOffset { get; set; }

    [Unknown]
    public int Unknown { get; set; } // Usually 0, but sometimes 1 (e.g. intro/countdwn)
  }
}