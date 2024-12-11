using System.Drawing;
using System.Numerics;

using fin.color;
using fin.math.xyz;

namespace fin.model.impl;

public class OneColor2UvVertexImpl
    : IVertex,
      ISingleColorVertex,
      IMultiUvVertex {
  private IColor? color_;
  private Vector2[] uv_ = new Vector2[2];

  public OneColor2UvVertexImpl(int index, Vector3 position) {
    this.Index = index;
    this.SetLocalPosition(position);
  }

  public OneColor2UvVertexImpl(int index,
                               float x,
                               float y,
                               float z) {
    this.Index = index;
    this.SetLocalPosition(x, y, z);
  }


  public int Index { get; }

  public IBoneWeights? BoneWeights { get; private set; }

  public void SetBoneWeights(IBoneWeights boneWeights) {
    this.BoneWeights = boneWeights;
  }


  public Vector3 LocalPosition { get; private set; }

  public void SetLocalPosition(Vector3 localPosition)
    => this.LocalPosition = localPosition;

  public void SetLocalPosition(IReadOnlyXyz localPosition)
    => this.SetLocalPosition(new Vector3(localPosition.X,
                                         localPosition.Y,
                                         localPosition.Z));


  public void SetLocalPosition(float x, float y, float z)
    => this.SetLocalPosition(new Vector3(x, y, z));


  public void SetColor(Color? color)
    => this.SetColor(color != null
                         ? FinColor.FromSystemColor(color.Value)
                         : null);

  public void SetColor(IColor? color) => this.color_ = color;

  public void SetColor(Vector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.Value.X,
                             color.Value.Y,
                             color.Value.Z,
                             color.Value.W)
                         : null);

  public void SetColor(IReadOnlyVector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.X,
                             color.Y,
                             color.Z,
                             color.W)
                         : null);

  public void SetColorBytes(byte r, byte g, byte b, byte a)
    => this.SetColor(FinColor.FromRgbaBytes(r, g, b, a));

  public IColor? GetColor() => this.color_;

  public int UvCount => 2;
  public Vector2? GetUv(int uvIndex) => this.uv_[uvIndex];

  public void SetUv(int uvIndex, Vector2? uv) => this.uv_[uvIndex] = uv.Value;

  public void SetUv(int uvIndex, float u, float v)
    => this.uv_[uvIndex] = new Vector2(u, v);
}


public class NormalUvVertexImpl
    : IVertex,
      INormalVertex,
      ISingleUvVertex {
  private Vector2? uv_;

  public NormalUvVertexImpl(int index, Vector3 position) {
    this.Index = index;
    this.SetLocalPosition(position);
  }

  public NormalUvVertexImpl(int index,
                            float x,
                            float y,
                            float z) {
    this.Index = index;
    this.SetLocalPosition(x, y, z);
  }

  public int Index { get; }

  public IBoneWeights? BoneWeights { get; private set; }

  public void SetBoneWeights(IBoneWeights boneWeights) {
    this.BoneWeights = boneWeights;
  }


  public Vector3 LocalPosition { get; private set; }

  public void SetLocalPosition(Vector3 localPosition) {
    this.LocalPosition = localPosition;
  }

  public void SetLocalPosition(IReadOnlyXyz localPosition)
    => this.SetLocalPosition(new Vector3(localPosition.X,
                                         localPosition.Y,
                                         localPosition.Z));


  public void SetLocalPosition(float x, float y, float z)
    => this.SetLocalPosition(new Vector3(x, y, z));


  public Vector3? LocalNormal { get; private set; }

  public void SetLocalNormal(Vector3? localNormal)
    => this.LocalNormal = localNormal;

  public void SetLocalNormal(IReadOnlyXyz? localNormal)
    => this.SetLocalNormal(localNormal != null
                               ? new Vector3(localNormal.X,
                                             localNormal.Y,
                                             localNormal.Z)
                               : null);

  public void SetLocalNormal(float x, float y, float z)
    => this.SetLocalNormal(new Vector3(x, y, z));


  public void SetUv(Vector2? uv) => this.uv_ = uv;

  public void SetUv(IReadOnlyVector2? uv)
    => this.SetUv(uv != null
                      ? new Vector2(uv.X, uv.Y)
                      : null);

  public void SetUv(float u, float v) => this.uv_ = new Vector2(u, v);

  public Vector2? GetUv() => this.uv_;
}


public class Normal1Color1UvVertexImpl
    : IVertex,
      INormalVertex,
      ISingleColorVertex,
      ISingleUvVertex {
  private IColor? color_;
  private Vector2? uv_;

  public Normal1Color1UvVertexImpl(int index, Vector3 position) {
    this.Index = index;
    this.SetLocalPosition(position);
  }

  public Normal1Color1UvVertexImpl(int index,
                                   float x,
                                   float y,
                                   float z) {
    this.Index = index;
    this.SetLocalPosition(x, y, z);
  }

  public int Index { get; }

  public IBoneWeights? BoneWeights { get; private set; }

  public void SetBoneWeights(IBoneWeights boneWeights)
    => this.BoneWeights = boneWeights;


  public Vector3 LocalPosition { get; private set; }

  public void SetLocalPosition(Vector3 localPosition)
    => this.LocalPosition = localPosition;

  public void SetLocalPosition(IReadOnlyXyz localPosition)
    => this.SetLocalPosition(new Vector3(localPosition.X,
                                         localPosition.Y,
                                         localPosition.Z));


  public void SetLocalPosition(float x, float y, float z)
    => this.SetLocalPosition(new Vector3(x, y, z));


  public Vector3? LocalNormal { get; private set; }

  public void SetLocalNormal(Vector3? localNormal)
    => this.LocalNormal = localNormal;

  public void SetLocalNormal(IReadOnlyXyz? localNormal)
    => this.SetLocalNormal(localNormal != null
                               ? new Vector3(localNormal.X,
                                             localNormal.Y,
                                             localNormal.Z)
                               : null);

  public void SetLocalNormal(float x, float y, float z)
    => this.SetLocalNormal(new Vector3(x, y, z));


  public void SetColor(Color? color)
    => this.SetColor(color != null
                         ? FinColor.FromSystemColor(color.Value)
                         : null);

  public void SetColor(IColor? color) => this.color_ = color;

  public void SetColor(Vector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.Value.X,
                             color.Value.Y,
                             color.Value.Z,
                             color.Value.W)
                         : null);

  public void SetColor(IReadOnlyVector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.X,
                             color.Y,
                             color.Z,
                             color.W)
                         : null);

  public void SetColorBytes(byte r, byte g, byte b, byte a)
    => this.SetColor(FinColor.FromRgbaBytes(r, g, b, a));

  public IColor? GetColor() => this.color_;


  public void SetUv(Vector2? uv) => this.uv_ = uv;

  public void SetUv(IReadOnlyVector2? uv)
    => this.SetUv(uv != null ? new Vector2(uv.X, uv.Y) : null);

  public void SetUv(float u, float v) => this.uv_ = new Vector2(u, v);

  public Vector2? GetUv() => this.uv_;
}

public class NormalTangent1Color1UvVertexImpl
    : IVertex,
      INormalTangentVertex,
      ISingleColorVertex,
      ISingleUvVertex {
  private IColor? color_;
  private Vector2? uv_;

  public NormalTangent1Color1UvVertexImpl(int index, Vector3 position) {
    this.Index = index;
    this.SetLocalPosition(position);
  }

  public NormalTangent1Color1UvVertexImpl(int index,
                                          float x,
                                          float y,
                                          float z) {
    this.Index = index;
    this.SetLocalPosition(x, y, z);
  }

  public int Index { get; }

  public IBoneWeights? BoneWeights { get; private set; }

  public void SetBoneWeights(IBoneWeights boneWeights) {
    this.BoneWeights = boneWeights;
  }


  public Vector3 LocalPosition { get; private set; }

  public void SetLocalPosition(Vector3 localPosition)
    => this.LocalPosition = localPosition;

  public void SetLocalPosition(IReadOnlyXyz localPosition)
    => this.SetLocalPosition(new Vector3(localPosition.X,
                                         localPosition.Y,
                                         localPosition.Z));


  public void SetLocalPosition(float x, float y, float z)
    => this.SetLocalPosition(new Vector3(x, y, z));


  public Vector3? LocalNormal { get; private set; }

  public void SetLocalNormal(Vector3? localNormal)
    => this.LocalNormal = localNormal;

  public void SetLocalNormal(IReadOnlyXyz? localNormal)
    => this.SetLocalNormal(localNormal != null
                               ? new Vector3(localNormal.X,
                                             localNormal.Y,
                                             localNormal.Z)
                               : null);

  public void SetLocalNormal(float x, float y, float z)
    => this.SetLocalNormal(new Vector3(x, y, z));


  public Vector4? LocalTangent { get; private set; }

  public void SetLocalTangent(Vector4? localTangent)
    => this.LocalTangent = localTangent;

  public void SetLocalTangent(IReadOnlyVector4? localTangent)
    => this.SetLocalTangent(localTangent != null
                                ? new Vector4(localTangent.X,
                                              localTangent.Y,
                                              localTangent.Z,
                                              localTangent.W)
                                : null);

  public void SetLocalTangent(float x, float y, float z, float w)
    => this.SetLocalTangent(new Vector4(x, y, z, w));

  public void SetColor(Color? color)
    => this.SetColor(color != null
                         ? FinColor.FromSystemColor(color.Value)
                         : null);

  public void SetColor(IColor? color) => this.color_ = color;

  public void SetColor(Vector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.Value.X,
                             color.Value.Y,
                             color.Value.Z,
                             color.Value.W)
                         : null);

  public void SetColor(IReadOnlyVector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.X,
                             color.Y,
                             color.Z,
                             color.W)
                         : null);

  public void SetColorBytes(byte r, byte g, byte b, byte a)
    => this.SetColor(FinColor.FromRgbaBytes(r, g, b, a));

  public IColor? GetColor() => this.color_;


  public void SetUv(Vector2? uv) => this.uv_ = uv;

  public void SetUv(IReadOnlyVector2? uv)
    => this.SetUv(uv != null ? new Vector2(uv.X, uv.Y) : null);

  public void SetUv(float u, float v) => this.uv_ = new Vector2(u, v);
  public Vector2? GetUv() => this.uv_;
}

public class NormalTangentMultiColorMultiUvVertexImpl
    : IVertex,
      INormalTangentVertex,
      ISingleColorVertex,
      IMultiColorVertex,
      ISingleUvVertex,
      IMultiUvVertex {
  private IVertexAttributeArray<IColor>? colors_;
  private IVertexAttributeArray<Vector2>? uvs_;

  public NormalTangentMultiColorMultiUvVertexImpl(
      int index,
      Vector3 position) {
    this.Index = index;
    this.SetLocalPosition(position);
  }

  public NormalTangentMultiColorMultiUvVertexImpl(
      int index,
      float x,
      float y,
      float z) {
    this.Index = index;
    this.SetLocalPosition(x, y, z);
  }

  public int Index { get; }

  public IBoneWeights? BoneWeights { get; private set; }

  public void SetBoneWeights(IBoneWeights boneWeights) {
    this.BoneWeights = boneWeights;
  }


  public Vector3 LocalPosition { get; private set; }

  public void SetLocalPosition(Vector3 localPosition)
    => this.LocalPosition = localPosition;

  public void SetLocalPosition(IReadOnlyXyz localPosition)
    => this.SetLocalPosition(new Vector3(localPosition.X,
                                         localPosition.Y,
                                         localPosition.Z));


  public void SetLocalPosition(float x, float y, float z)
    => this.SetLocalPosition(new Vector3(x, y, z));


  public Vector3? LocalNormal { get; private set; }

  public void SetLocalNormal(Vector3? localNormal)
    => this.LocalNormal = localNormal;

  public void SetLocalNormal(IReadOnlyXyz? localNormal)
    => this.SetLocalNormal(localNormal != null
                               ? new Vector3(localNormal.X,
                                             localNormal.Y,
                                             localNormal.Z)
                               : null);

  public void SetLocalNormal(float x, float y, float z)
    => this.SetLocalNormal(new Vector3(x, y, z));


  public Vector4? LocalTangent { get; private set; }

  public void SetLocalTangent(Vector4? localTangent)
    => this.LocalTangent = localTangent;

  public void SetLocalTangent(IReadOnlyVector4? localTangent)
    => this.SetLocalTangent(localTangent != null
                                ? new Vector4(localTangent.X,
                                              localTangent.Y,
                                              localTangent.Z,
                                              localTangent.W)
                                : null);

  public void SetLocalTangent(float x, float y, float z, float w)
    => this.SetLocalTangent(new Vector4(x, y, z, w));

  public void SetColor(Color? color) {
    if (color != null) {
      this.colors_ ??= new SingleVertexAttribute<IColor>();
      this.colors_[0] = FinColor.FromSystemColor(color.Value);
    } else {
      this.colors_?.Set(0, null);
      if (this.colors_?.Count == 0) {
        this.colors_ = null;
      }
    }
  }

  public void SetColor(IColor? color) {
    if (color != null) {
      this.colors_ ??= new SingleVertexAttribute<IColor>();
      this.colors_[0] = color;
    } else {
      this.colors_?.Set(0, null);
      if (this.colors_?.Count == 0) {
        this.colors_ = null;
      }
    }
  }

  public void SetColor(Vector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.Value.X,
                             color.Value.Y,
                             color.Value.Z,
                             color.Value.W)
                         : null);

  public void SetColor(IReadOnlyVector4? color)
    => this.SetColor(color != null
                         ? FinColor.FromRgbaFloats(
                             color.X,
                             color.Y,
                             color.Z,
                             color.W)
                         : null);

  public void SetColor(int colorIndex, IColor? color) {
    if (color != null) {
      this.colors_ ??= new SparseVertexAttributeArray<IColor>();
      this.colors_[colorIndex] = color;
    } else {
      this.colors_?.Set(colorIndex, null);
      if (this.colors_?.Count == 0) {
        this.colors_ = null;
      }
    }
  }

  public void SetColorBytes(byte r, byte g, byte b, byte a)
    => this.SetColor(FinColor.FromRgbaBytes(r, g, b, a));

  public void SetColorBytes(
      int colorIndex,
      byte r,
      byte g,
      byte b,
      byte a)
    => this.SetColor(colorIndex, FinColor.FromRgbaBytes(r, g, b, a));

  public int ColorCount => this.colors_?.Count ?? 0;
  public IColor? GetColor() => this.GetColor(0);

  public IColor? GetColor(int colorIndex) => this.colors_?.Get(colorIndex);


  public int UvCount => this.uvs_?.Count ?? 0;

  public void SetUv(Vector2? uv) {
    if (uv == null) {
      this.uvs_ = null;
    } else {
      this.uvs_ ??= new SingleVertexAttribute<Vector2>();
      this.uvs_[0] = uv.Value;
    }
  }

  public void SetUv(IReadOnlyVector2? uv)
    => this.SetUv(uv != null ? new Vector2(uv.X, uv.Y) : null);

  public void SetUv(float u, float v) {
    this.uvs_ ??= new SingleVertexAttribute<Vector2>();
    this.uvs_[0] = new Vector2(u, v);
  }

  public void SetUv(int uvIndex, Vector2? uv) {
    if (uv != null) {
      this.uvs_ ??= new SparseVertexAttributeArray<Vector2>();
      this.uvs_[uvIndex] = uv.Value;
    } else {
      this.uvs_?.Set(uvIndex, default!);
      if (this.uvs_?.Count == 0) {
        this.uvs_ = null;
      }
    }
  }

  public void SetUv(int uvIndex, float u, float v)
    => this.SetUv(uvIndex, new Vector2(u, v));

  public Vector2? GetUv() => this.GetUv(0);

  public Vector2? GetUv(int uvIndex) => this.uvs_?.Get(uvIndex);


  public override string ToString() => $"{this.Index}: {this.LocalPosition}";
}