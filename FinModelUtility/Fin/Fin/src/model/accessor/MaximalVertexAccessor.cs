using System.Numerics;
using System.Runtime.CompilerServices;

using fin.color;

namespace fin.model.accessor {
  public class MaximalVertexAccessor : IVertexAccessor {
    private IReadOnlyVertex currentVertex_;

    public static IVertexAccessor GetAccessorForModel(IReadOnlyModel model)
      => new MaximalVertexAccessor();

    private MaximalVertexAccessor() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Target(IReadOnlyVertex vertex) {
      this.currentVertex_ = vertex;
    }

    public int Index => this.currentVertex_.Index;

    public IReadOnlyBoneWeights? BoneWeights => this.currentVertex_.BoneWeights;
    public Vector3 LocalPosition => this.currentVertex_.LocalPosition;

    public Vector3? LocalNormal
      => (this.currentVertex_ as IReadOnlyNormalVertex)?.LocalNormal;

    public Vector4? LocalTangent
      => (this.currentVertex_ as IReadOnlyTangentVertex)?.LocalTangent;

    public int ColorCount
      => (this.currentVertex_ as IReadOnlyMultiColorVertex)?.ColorCount ??
         (this.currentVertex_ is IReadOnlySingleColorVertex ? 1 : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor()
      => (this.currentVertex_ as IReadOnlyMultiColorVertex)?.GetColor(0) ??
         (this.currentVertex_ as IReadOnlySingleColorVertex)?.GetColor();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IColor? GetColor(int colorIndex)
      => (this.currentVertex_ as IReadOnlyMultiColorVertex)?.GetColor(
             colorIndex) ??
         (this.currentVertex_ as IReadOnlySingleColorVertex)?.GetColor();

    public int UvCount
      => (this.currentVertex_ as IReadOnlyMultiUvVertex)?.UvCount ??
         (this.currentVertex_ is IReadOnlySingleUvVertex ? 1 : 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2? GetUv()
      => (this.currentVertex_ as IReadOnlyMultiUvVertex)?.GetUv(0) ??
         (this.currentVertex_ as IReadOnlySingleUvVertex)?.GetUv();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2? GetUv(int uvIndex)
      => (this.currentVertex_ as IReadOnlyMultiUvVertex)?.GetUv(uvIndex) ??
         (this.currentVertex_ as IReadOnlySingleUvVertex)?.GetUv();
  }
}