using fin.math.matrix.four;

namespace sm64.Scripts {
  public class GeoScriptNode {
    public GeoScriptNode(GeoScriptNode? parent) {
      this.parent = parent;
    }

    public int ID = 0;
    public GeoScriptNode? parent = null;
    public IFinMatrix4x4 matrix { get; } = new FinMatrix4x4().SetIdentity();
    public bool callSwitch = false, isSwitch = false;
    public uint switchFunc = 0, switchCount = 0, switchPos = 0;

    public IFinMatrix4x4 GetTotalMatrix() {
      var matrices = new LinkedList<IFinMatrix4x4>();

      var current = this;
      while (current != null) {
        matrices.AddFirst(current.matrix);
        current = current.parent;
      }

      var matrix = new FinMatrix4x4().SetIdentity();
      foreach (var mat in matrices) {
        matrix.MultiplyInPlace(mat);
      }
      return matrix;
    }
  }
}