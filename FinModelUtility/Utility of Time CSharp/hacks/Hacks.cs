using System.Drawing;

using f3dzex2.image;
using f3dzex2.io;

namespace UoT.hacks;

public static class Hacks {
  public static void ApplyHacks(IN64Hardware<N64Memory> n64Hardware,
                                string fileName) {
      var environmentColor = EnvironmentColorHacks.GetColorForObject(fileName);
      n64Hardware.Rsp.EnvironmentColor = environmentColor ?? Color.Magenta;
    }
}