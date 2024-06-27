namespace gx;

public enum GxTexGenType : byte {
  Matrix3x4 = 0,
  Matrix2x4 = 1,
  Bump0 = 2,
  Bump1 = 3,
  Bump2 = 4,
  Bump3 = 5,
  Bump4 = 6,
  Bump5 = 7,
  Bump6 = 8,
  Bump7 = 9,
  SRTG = 10
}

public enum GxTexGenSrc : byte {
  Position = 0,
  Normal = 1,
  Binormal = 2,
  Tangent = 3,
  Tex0 = 4,
  Tex1 = 5,
  Tex2 = 6,
  Tex3 = 7,
  Tex4 = 8,
  Tex5 = 9,
  Tex6 = 10,
  Tex7 = 11,
  TexCoord0 = 12,
  TexCoord1 = 13,
  TexCoord2 = 14,
  TexCoord3 = 15,
  TexCoord4 = 16,
  TexCoord5 = 17,
  TexCoord6 = 18,
  Color0 = 19,
  Color1 = 20,
}

public enum GxTexMatrix : byte {
  TexMtx0 = 30,
  TexMtx1 = 33,
  TexMtx2 = 36,
  TexMtx3 = 39,
  TexMtx4 = 42,
  TexMtx5 = 45,
  TexMtx6 = 48,
  TexMtx7 = 51,
  TexMtx8 = 54,
  TexMtx9 = 57,
  Identity = 60,
}

public interface ITexCoordGen {
  GxTexGenType TexGenType { get; }
  GxTexGenSrc TexGenSrc { get; }
  GxTexMatrix TexMatrix { get; }
}

public class TexCoordGenImpl : ITexCoordGen {
  public GxTexGenType TexGenType { get; set; }
  public GxTexGenSrc TexGenSrc { get; set; }
  public GxTexMatrix TexMatrix { get; set; }
}