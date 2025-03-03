namespace nitro.schema.nsbmd.mesh;

public interface IOpcode;

public interface ILoadMatrixOpcode : IOpcode {
  byte StackPos { get; }
}

public interface IStoreMatrixOpcode : IOpcode {
  byte StackPos { get; }
}

public interface IMultMatrixOpcode : IOpcode {
  byte ObjectIndex { get; }
}

public interface ISkinTerm {
  byte StackPos { get; }
  byte InverseBindIndex { get; }
  float Weight { get; }
}

public interface ISkinOpcode : IOpcode {
  IReadOnlyList<ISkinTerm> SkinTerms { get; }
}

public interface IScaleUpOpcode : IOpcode;

public interface IScaleDownOpcode : IOpcode;

public interface IBindMaterialOpcode : IOpcode {
  byte MaterialIndex { get; }
}

public interface IDrawOpcode : IOpcode {
  byte PieceIndex { get; }
}