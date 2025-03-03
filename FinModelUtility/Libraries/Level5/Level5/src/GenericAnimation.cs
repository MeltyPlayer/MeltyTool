namespace level5;

public class GenericAnimation {
  public string Name { get; set; }
  public uint NameHash { get; set; }

  public List<GenericAnimationTransform> TransformNodes = [];

  public int FrameCount { get; set; } = 0;

  public override string ToString() {
      return this.Name;
    }
}