using System.Runtime.CompilerServices;

namespace fin.model.accessor {
  public partial class ConsistentVertexAccessor {
    private sealed class MultiUvAccessor : BAccessor, IVertexUvAccessor {
      private IReadOnlyMultiUvVertex uvVertex_;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void Target(IReadOnlyVertex vertex) {
        this.uvVertex_ = vertex as IReadOnlyMultiUvVertex;
      }

      public int UvCount => this.uvVertex_.UvCount;

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public TexCoord? GetUv() => this.GetUv(0);

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public TexCoord? GetUv(int uvIndex) => this.uvVertex_.GetUv(uvIndex);
    }
  }
}