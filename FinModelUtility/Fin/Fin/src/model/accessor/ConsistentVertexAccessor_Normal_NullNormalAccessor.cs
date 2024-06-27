using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.model.accessor;

public partial class ConsistentVertexAccessor {
  private sealed class NullNormalAccessor : BAccessor, IVertexNormalAccessor {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Target(IReadOnlyVertex vertex) { }

    public Vector3? LocalNormal => null;
  }
}