using System.Numerics;
using System.Runtime.CompilerServices;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class TangentAccessor : BAccessor, IVertexTangentAccessor {
      private IReadOnlyTangentVertex tangentVertex_;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) {
        this.tangentVertex_ = vertex as IReadOnlyTangentVertex;
      }

      public Vector4? LocalTangent => this.tangentVertex_.LocalTangent;
    }
  }
}