using System.Drawing;
using System.Numerics;

using fin.color;
using fin.data.indexable;
using fin.math.xyz;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IVertex : IIndexable {
  IBoneWeights? BoneWeights { get; }
  Vector3 LocalPosition { get; }

  void SetBoneWeights(IBoneWeights boneWeights);

  void SetLocalPosition(Vector3 localPosition);
  void SetLocalPosition(IReadOnlyXyz localPosition);
  void SetLocalPosition(float x, float y, float z);
}

[GenerateReadOnly]
public partial interface INormalVertex : IVertex {
  Vector3? LocalNormal { get; }
  void SetLocalNormal(Vector3? localNormal);
  void SetLocalNormal(IReadOnlyXyz? localNormal);
  void SetLocalNormal(float x, float y, float z);
}

[GenerateReadOnly]
public partial interface ITangentVertex : IVertex {
  Vector4? LocalTangent { get; }
  void SetLocalTangent(Vector4? localTangent);
  void SetLocalTangent(IReadOnlyVector4? localTangent);
  void SetLocalTangent(float x, float y, float z, float w);
}

[GenerateReadOnly]
public partial interface INormalTangentVertex : INormalVertex, ITangentVertex;

[GenerateReadOnly]
public partial interface ISingleColorVertex : IVertex {
  [Const]
  IColor? GetColor();

  void SetColor(Color? color);
  void SetColor(IColor? color);
  void SetColor(Vector4? color);
  void SetColor(IReadOnlyVector4? color);
  void SetColorBytes(byte r, byte g, byte b, byte a);
}

[GenerateReadOnly]
public partial interface IMultiColorVertex : IVertex {
  int ColorCount { get; }

  [Const]
  IColor? GetColor(int colorIndex);

  void SetColor(int colorIndex, IColor? color);

  void SetColorBytes(int colorIndex,
                     byte r,
                     byte g,
                     byte b,
                     byte a);
}

[GenerateReadOnly]
public partial interface ISingleUvVertex : IVertex {
  [Const]
  Vector2? GetUv();

  void SetUv(Vector2? uv);
  void SetUv(IReadOnlyVector2? uv);
  void SetUv(float u, float v);
}

[GenerateReadOnly]
public partial interface IMultiUvVertex : IVertex {
  int UvCount { get; }

  [Const]
  Vector2? GetUv(int uvIndex);

  void SetUv(int uvIndex, Vector2? uv);
  void SetUv(int uvIndex, float u, float v);
}