using schema.binary;
using schema.binary.attributes;

using sonicadventure.util;

namespace sonicadventure.schema.animation;

[BinarySchema]
public partial class Animation(uint keyedPointer, uint key)
    : IKeyedInstance<Animation> {
  public static Animation New(uint keyedPointer, uint key)
    => new(keyedPointer, key);

  public uint ObjectKeyedPointer { get; set; }

  private uint dataOffset_;

  [Skip]
  public AnimationData Data { get; private set; }

  [ReadLogic]
  private void ReadData_(IBinaryReader br) {
    this.Data = br.ReadAtPointer<AnimationData>(this.dataOffset_, key);
  }
}

[BinarySchema]
public partial class AnimationData(uint keyedPointer, uint key)
    : IKeyedInstance<AnimationData> {
  public static AnimationData New(uint keyedPointer, uint key)
    => new(keyedPointer, key);

  private uint frameDataOffset_ { get; set; }
  public uint FrameCount { get; set; }
  public ushort Flags { get; set; }
  public ushort PointerFrameCountPairCount { get; set; }
}