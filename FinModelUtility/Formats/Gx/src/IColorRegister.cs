using System.Drawing;

namespace gx;

public interface IColorRegister {
  int Index { get; }
  Color Color { get; }
}

public readonly struct GxColorRegister : IColorRegister {
  public required int Index { get; init; }
  public required Color Color { get; init; }
}