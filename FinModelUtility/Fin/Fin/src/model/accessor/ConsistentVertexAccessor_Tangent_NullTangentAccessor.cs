using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class NullTangentAccessor
        : BAccessor, IVertexTangentAccessor {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) { }

      public Vector4? LocalTangent => null;
    }
  }
}