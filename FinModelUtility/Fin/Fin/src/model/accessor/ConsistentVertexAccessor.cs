using System;
using System.Numerics;
using System.Runtime.CompilerServices;

using fin.color;

namespace fin.model.accessor;

/// <summary>
///   Assumes all vertices are the same, consistent type.
/// </summary>
public partial class ConsistentVertexAccessor : IVertexAccessor {
  private IReadOnlyVertex currentVertex_;
  private readonly IVertexNormalAccessor normalAccessor_;
  private readonly IVertexTangentAccessor tangentAccessor_;
  private readonly IVertexColorAccessor colorAccessor_;
  private readonly IVertexUvAccessor uvAccessor_;

  public static IVertexAccessor GetAccessorForModel(IReadOnlyModel model)
    => new ConsistentVertexAccessor(model);

  private ConsistentVertexAccessor(IReadOnlyModel model) {
    var skin = model.Skin;
    var firstVertex = skin.Vertices.Count > 0 ? skin.Vertices[0] : null;

    this.normalAccessor_ = firstVertex is IReadOnlyNormalVertex
        ? new NormalAccessor()
        : new NullNormalAccessor();
    this.tangentAccessor_ = firstVertex is IReadOnlyTangentVertex
        ? new TangentAccessor()
        : new NullTangentAccessor();
    this.colorAccessor_ = firstVertex is IReadOnlyMultiColorVertex
        ? new MultiColorAccessor()
        : firstVertex is IReadOnlySingleColorVertex
            ? new SingleColorAccessor()
            : new NullColorAccessor();
    this.uvAccessor_ = firstVertex is IReadOnlyMultiUvVertex
        ? new MultiUvAccessor()
        : firstVertex is IReadOnlySingleUvVertex
            ? new SingleUvAccessor()
            : new NullUvAccessor();
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Target(IReadOnlyVertex vertex) {
    this.currentVertex_ = vertex;
    this.normalAccessor_.Target(vertex);
    this.tangentAccessor_.Target(vertex);
    this.colorAccessor_.Target(vertex);
    this.uvAccessor_.Target(vertex);
  }

  public int Index => this.currentVertex_.Index;

  public IReadOnlyBoneWeights? BoneWeights => this.currentVertex_.BoneWeights;
  public Vector3 LocalPosition => this.currentVertex_.LocalPosition;

  public Vector3? LocalNormal => this.normalAccessor_.LocalNormal;
  public Vector4? LocalTangent => this.tangentAccessor_.LocalTangent;

  public int ColorCount => this.colorAccessor_.ColorCount;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IColor? GetColor() => this.colorAccessor_.GetColor();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public IColor? GetColor(int colorIndex)
    => this.colorAccessor_.GetColor(colorIndex);

  public int UvCount => this.uvAccessor_.UvCount;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector2? GetUv() => this.uvAccessor_.GetUv();

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector2? GetUv(int uvIndex) => this.uvAccessor_.GetUv(uvIndex);


  private abstract class BAccessor : IReadOnlyVertex {
    public int Index => throw new NotImplementedException();

    public IReadOnlyBoneWeights? BoneWeights
      => throw new NotImplementedException();

    public Vector3 LocalPosition => throw new NotImplementedException();
  }
}