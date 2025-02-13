using System.Runtime.CompilerServices;

using fin.color;
using fin.util.asserts;

namespace fin.model.accessor;

public partial class ConsistentVertexAccessor {
  private sealed class MultiColorAccessor : BAccessor, IVertexColorAccessor {
    private IReadOnlyMultiColorVertex colorVertex_;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Target(IReadOnlyVertex vertex) {
      this.colorVertex_ = vertex.AssertAsA<IReadOnlyMultiColorVertex>();
    }

    public int ColorCount => this.colorVertex_.ColorCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor() => this.GetColor(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor(int colorIndex)
      => this.colorVertex_.GetColor(colorIndex);
  }
}