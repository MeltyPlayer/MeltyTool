using System.Runtime.CompilerServices;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class SingleUvAccessor : BAccessor, IVertexUvAccessor {
      private IReadOnlySingleUvVertex uvVertex_;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) {
        this.uvVertex_ = vertex as IReadOnlySingleUvVertex;
      }

      public int UvCount => this.GetUv() != null ? 1 : 0;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public TexCoord? GetUv() => this.uvVertex_.GetUv();

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public TexCoord? GetUv(int uvIndex) => this.uvVertex_.GetUv();
    }
  }
}