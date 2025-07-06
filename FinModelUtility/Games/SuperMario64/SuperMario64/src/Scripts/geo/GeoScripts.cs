using fin.math.matrix.four;

namespace sm64.Scripts {
  public class GeoScriptNode {
    public GeoScriptNode(GeoScriptNode? parent,
                         IReadOnlyFinMatrix4x4 inMatrix) {
      this.parent = parent;
      this.matrix = inMatrix;
    }

    public int ID = 0;
    public GeoScriptNode? parent = null;
    public IReadOnlyFinMatrix4x4 matrix { get; }
    public bool callSwitch = false, isSwitch = false;
    public uint switchFunc = 0, switchCount = 0, switchPos = 0;
  }
}