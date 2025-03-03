using System;

using fin.schema.color;
using fin.schema.vector;

namespace f3dzex2.gbi;

public enum AlphaCompareMode {
  // TODO: What are these values?
}

public enum CombineMode1 {
  // TODO: What are these values?
}

public enum CombineMode2 {
  // TODO: What are these values?
}

public enum CycleType {
  // TODO: What are these values?
}

public enum RenderMode1 {
  // TODO: What are these values?
}

public enum RenderMode2 {
  // TODO: What are these values?
}

/// <summary>
///   Interface for the static version of the N64 display processor macros.
/// </summary>
internal interface IGdp {
  void SetAlphaCompare(AlphaCompareMode mode);
  void SetBlendColor(byte r, byte g, byte b, byte a);
  void SetCombineMode(CombineMode1 mode1, CombineMode2 mode2);
  void SetCycleType(CycleType type);
  void SetEnvColor(byte r, byte g, byte b, byte a);
  void SetFogColor(byte r, byte g, byte b, byte a);
  void SetFogPosition(int min, int max);
  void SetPrimColor(byte m, byte l, byte r, byte g, byte b, byte a);
  void SetRenderMode(RenderMode1 mode1, RenderMode2 mode2);
}

public interface IN64Vertex {
  Vector3s Xyz { get; }
  Vector2s TextureCoordinates { get; }
  Rgba32 ColorOrNormalAndAlpha { get; }
}

public enum GeometryMode {
  // TODO: What are these values?
}

public enum TriangleFlag {
  ONE,
  TWO,
  THREE
}

/// <summary>
///   Interface for the static version of the N64 signal processor macros.
/// </summary>
internal interface IGsp {
  void _1Triangle(int v0, int v1, int v2, TriangleFlag flag);

  void _2Triangles(int v00,
                   int v01,
                   int v02,
                   TriangleFlag flag0,
                   int v10,
                   int v11,
                   int v12,
                   TriangleFlag flag1);

  void ClearGeometryMode(GeometryMode mode);
  void SetGeometryMode(GeometryMode mode);

  void Vertex(ReadOnlySpan<IN64Vertex> vertices, uint startingOffset);
}