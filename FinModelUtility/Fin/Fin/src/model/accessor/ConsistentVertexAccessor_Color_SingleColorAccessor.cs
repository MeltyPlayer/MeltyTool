using System.Runtime.CompilerServices;

using fin.color;

namespace fin.model.accessor;

public partial class ConsistentVertexAccessor {
  private sealed class SingleColorAccessor : BAccessor, IVertexColorAccessor {
    private IReadOnlySingleColorVertex colorVertex_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Target(IReadOnlyVertex vertex) {
      this.colorVertex_ = vertex as IReadOnlySingleColorVertex;
    }

    public int ColorCount => this.GetColor() != null ? 1 : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor() => this.colorVertex_.GetColor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor(int colorIndex) => this.colorVertex_.GetColor();
  }
}