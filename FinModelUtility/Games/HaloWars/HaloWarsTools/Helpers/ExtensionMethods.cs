using System.Numerics;

namespace HaloWarsTools;

public static class ExtensionMethods {
  public static Vector3 ReverseComponents(this Vector3 vector) {
      return new Vector3(vector.Z, vector.Y, vector.X);
    }
}