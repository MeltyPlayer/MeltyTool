using schema.binary;
using schema.binary.attributes;

namespace ttyd.schema.model.blocks {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/PistonMiner/ttyd-tools/blob/master/ttyd-tools/docs/MarioSt_AnimGroupBase.bt#L377
  /// </summary>
  [BinarySchema]
  [LocalPositions]
  public partial class AnimationData : IBinaryConvertible {
    private uint DataSize { get; set; }

    private uint BaseInfoCount { get; set; }
    private uint KeyframeCount { get; set; }
    private uint VertexPositionDeltaCount { get; set; }
    private uint VertexNormalDeltaCount { get; set; }
    private uint TextureCoordinateTransformDeltaCount { get; set; }
    private uint VisibilityGroupDeltaCount { get; set; }
    private uint GroupTransformDataDeltaCount { get; set; }
    private uint wAnimDataType8Count { get; set; }

    private uint BaseInfoOffset { get; set; }
    private uint KeyframeOffset { get; set; }
    private uint VertexPositionDeltaOffset { get; set; }
    private uint VertexNormalDeltaOffset { get; set; }
    private uint TextureCoordinateTransformDeltaOffset { get; set; }
    private uint VisibilityGroupDeltaOffset { get; set; }
    private uint GroupTransformDataDeltaOffset { get; set; }
    private uint wAnimDataType8Offset { get; set; }

    [RSequenceLengthSource(nameof(BaseInfoCount))]
    [RAtPositionOrNull(nameof(BaseInfoOffset))]
    public AnimationModelFileAnimationBaseInfo[] BaseInfos { get; set; }

    [RSequenceLengthSource(nameof(KeyframeCount))]
    [RAtPositionOrNull(nameof(KeyframeOffset))]
    public AnimationModelFileAnimationKeyframe[] Keyframes { get; set; }

    [RSequenceLengthSource(nameof(VertexPositionDeltaCount))]
    [RAtPositionOrNull(nameof(VertexPositionDeltaOffset))]
    public AnimationModelFileAnimationVectorDelta[] VertexPositionDeltas {
      get;
      set;
    }

    [RSequenceLengthSource(nameof(VertexNormalDeltaCount))]
    [RAtPositionOrNull(nameof(VertexNormalDeltaOffset))]
    public AnimationModelFileAnimationVectorDelta[] VertexNormalDeltas {
      get;
      set;
    }

    [RSequenceLengthSource(nameof(TextureCoordinateTransformDeltaCount))]
    [RAtPositionOrNull(nameof(TextureCoordinateTransformDeltaOffset))]
    public AnimationModelFileAnimationTextureCoordinateTransformDelta[]
        TextureCoordinateTransformDeltas { get; set; }

    [RSequenceLengthSource(nameof(VisibilityGroupDeltaCount))]
    [RAtPositionOrNull(nameof(VisibilityGroupDeltaOffset))]
    public AnimationModelFileAnimationVisibilityGroupStatus[]
        VisibilityGroupDeltas { get; set; }

    [RSequenceLengthSource(nameof(GroupTransformDataDeltaCount))]
    [RAtPositionOrNull(nameof(GroupTransformDataDeltaOffset))]
    public AnimationModelFileAnimationGroupTransformDataDelta[]
        GroupTransformDataDeltas { get; set; }

  }

  [BinarySchema]
  public partial class
      AnimationModelFileAnimationBaseInfo : IBinaryConvertible {
    [IntegerFormat(SchemaIntegerType.UINT32)]
    public bool Loop { get; set; }

    public float Start { get; set; }
    public float End { get; set; }
  }

  [BinarySchema]
  public partial class AnimationModelFileAnimationKeyframe
      : IBinaryConvertible {
    public float Time { get; set; }

    public uint VertexPositionDeltaBaseIndex { get; set; }
    public uint VertexPositionDeltaCount { get; set; }

    public uint VertexNormalDeltaBaseIndex { get; set; }
    public uint VertexNormalDeltaCount { get; set; }

    public uint TextureCoordinateTransformDeltaBaseIndex { get; set; }
    public uint TextureCoordinateTransformDeltaCount { get; set; }

    public uint VisibilityGroupDeltaBaseIndex { get; set; }
    public uint VisibilityGroupDeltaCount { get; set; }

    public uint GroupTransformDataDeltaBaseIndex { get; set; }
    public uint GroupTransformDataDeltaCount { get; set; }
  }

  [BinarySchema]
  public partial class AnimationModelFileAnimationVectorDelta
      : IBinaryConvertible {
    public byte IndexDelta { get; set; }

    [SequenceLengthSource(3)]
    public sbyte[] CoordinateDeltas { get; set; }
  }

  [BinarySchema]
  public partial class
      AnimationModelFileAnimationTextureCoordinateTransformDelta
      : IBinaryConvertible {
    public byte IndexDelta { get; set; }
    public sbyte wFrameExtDelta { get; set; }
    private short padding_;
    public float TranslateXDelta { get; set; }
    public float TranslateYDelta { get; set; }
  }

  [BinarySchema]
  public partial class AnimationModelFileAnimationVisibilityGroupStatus
      : IBinaryConvertible {
    public byte VisibilityGroupId { get; set; }
    public sbyte Visible { get; set; }
  }


  [BinarySchema]
  public partial class AnimationModelFileAnimationGroupTransformDataDelta
      : IBinaryConvertible {
    public byte IndexDelta { get; set; }
    public sbyte ValueDelta { get; set; }
    public sbyte InTangentDegrees { get; set; }
    public sbyte OutTangentDegrees { get; set; }
  }
}