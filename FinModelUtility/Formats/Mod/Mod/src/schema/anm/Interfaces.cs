using schema.binary;

namespace mod.schema.anm {
  public interface IDcx : IBinaryConvertible {
    string Name { get; set; }
    IDcxAnimationData AnimationData { get; }
  }

  public interface IDcxAnimationData : IBinaryConvertible {
    uint FrameCount { get; set; }

    float[] ScaleValues { get; set; }
    float[] RotationValues { get; set; }
    float[] PositionValues { get; set; }

    IDcxJointData[] JointDataList { get; set; }
  }

  public interface IDcxJointData : IBinaryConvertible {
    int JointIndex { get; set; }
    int ParentIndex { get; set; }

    IDcxAxes ScaleAxes { get; }
    IDcxAxes RotationAxes { get; }
    IDcxAxes PositionAxes { get; }
  }

  public interface IDcxAxes : IBinaryConvertible {
    IDcxAxis[] Axes { get; }
  }

  public interface IDcxAxis : IBinaryConvertible {
    int FrameCount { get; set; }
    int FrameOffset { get; set; }
  }
}