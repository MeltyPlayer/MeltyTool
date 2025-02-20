using System.Numerics;


namespace HaloWarsTools {
  public class HWObjectInstance {
    public string Name;
    public HWObjectDefinition Definition;
    public Matrix4x4 Matrix;

    public Vector3 Position => Matrix.Translation;

    public override string ToString() {
      return $"{Name} {Position} ({Definition.Name})";
    }
  }
}