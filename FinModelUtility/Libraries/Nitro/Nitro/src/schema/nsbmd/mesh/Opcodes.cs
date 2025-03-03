namespace nitro.schema.nsbmd.mesh;

public record LoadMatrixOpcode(byte StackPos) : ILoadMatrixOpcode;

public record StoreMatrixOpcode(byte StackPos) : IStoreMatrixOpcode;

public record MultObjectOpcode(byte ObjectIndex) : IMultMatrixOpcode;

public record SkinTerm(
    byte StackPos,
    byte InverseBindIndex,
    float Weight) : ISkinTerm;

public record SkinOpcode(IReadOnlyList<ISkinTerm> SkinTerms) : ISkinOpcode;

public record ScaleUpOpcode : IScaleUpOpcode;

public record ScaleDownOpcode : IScaleDownOpcode;

public record BindMaterialOpcode(byte MaterialIndex) : IBindMaterialOpcode;

public record DrawOpcode(byte PieceIndex) : IDrawOpcode;