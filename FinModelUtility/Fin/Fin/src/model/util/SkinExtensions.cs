using System.Numerics;

using fin.math.matrix.four;
using fin.math.rotations;
using fin.math.xyz;

namespace fin.model.util {
  public static class SkinExtensions {
    public static TVertex AddVertex<TVertex>(
        this ISkin<TVertex> skin,
        float x,
        float y,
        float z)
        where TVertex : IReadOnlyVertex
      => skin.AddVertex(new Position(x, y, z));

    public static TVertex AddVertex<TVertex>(
        this ISkin<TVertex> skin,
        Vector3 position)
        where TVertex : IReadOnlyVertex
      => skin.AddVertex(position.X, position.Y, position.Z);

    public static TVertex AddVertex<TVertex>(
        this ISkin<TVertex> skin,
        IReadOnlyXyz position)
        where TVertex : IReadOnlyVertex
      => skin.AddVertex(position.X, position.Y, position.Z);
  }
}