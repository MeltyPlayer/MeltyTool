using System.Runtime.CompilerServices;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class NormalAccessor : BAccessor, IVertexNormalAccessor {
      private IReadOnlyNormalVertex normalVertex_;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) {
        this.normalVertex_ = vertex as IReadOnlyNormalVertex;
      }

      public Normal? LocalNormal => this.normalVertex_.LocalNormal;
    }
  }
}