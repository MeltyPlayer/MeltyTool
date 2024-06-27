namespace modl.schema.anim;

public class AnimBoneFrames {
  public List<(float, float, float)> PositionFrames { get; }
  public List<(float, float, float, float)> RotationFrames { get; }

  public AnimBoneFrames() : this(0, 0) { }

  public AnimBoneFrames(int positionCapacity, int rotationCapacity) {
    this.PositionFrames = new(positionCapacity);
    this.RotationFrames = new(rotationCapacity);
  }
}