namespace jsystem.schema.j3dgraph.bcx;

public interface IBcx {
  IAnx1 Anx1 { get; }
}

public interface IAnx1 {
  int FrameCount { get; }
  IAnimatedJoint[] Joints { get; }
}

public interface IAnimatedJoint {
  IJointAnim Values { get; }
}

public interface IJointAnim {
  IJointAnimKey[][] Scales { get; }
  IJointAnimKey[][] Rotations { get; }
  IJointAnimKey[][] Translations { get; }
}

public interface IJointAnimKey {
  int Frame { get; }
  float Value { get; }
}