namespace modl.schema.anim;

public class AnimBoneFrames(int positionCapacity, int rotationCapacity) {
  public List<(float, float, float)> PositionFrames { get; } = new(positionCapacity);
  public List<(float, float, float, float)> RotationFrames { get; } = new(rotationCapacity);

  public AnimBoneFrames() : this(0, 0) { }
}