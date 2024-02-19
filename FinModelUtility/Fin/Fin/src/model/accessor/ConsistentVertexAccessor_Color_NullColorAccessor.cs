using System.Runtime.CompilerServices;

using fin.color;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class NullColorAccessor
        : BAccessor, IVertexColorAccessor {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) { }

      public int ColorCount => 0;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public IColor? GetColor() => null;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public IColor? GetColor(int colorIndex) => null;
    }
  }
}