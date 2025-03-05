using System.Numerics;

using fin.math.matrix.four;
using fin.schema.vector;

namespace granny3d {
  public interface IGrannyFileInfo {
    string FromFileName { get; }

    IList<IGrannySkeleton> SkeletonHeaderList { get; }
    IList<IGrannyMesh> VertexDataList { get; }
    IList<IGrannyModel> ModelHeaderList { get; }
    IList<IGrannyTrackGroup> TrackGroupHeaderList { get; }
    IList<IGrannyAnimation> AnimationHeaderList { get; }
  }


  public interface IGrannySkeleton {
    string Name { get; }
    IList<IGrannyBone> Bones { get; }
    int LodType { get; }
  }

  public interface IGrannyBone {
    string Name { get; }
    int ParentIndex { get; }
    IGrannyTransform LocalTransform { get; }
    IFinMatrix4x4 InverseWorld4x4 { get; }
    float LodError { get; }
  }


  public interface IGrannyMesh;

  public interface IGrannyModel;

  public interface IGrannyTrackGroup;

  public interface IGrannyAnimation {
    string Name { get; }

    float Duration { get; }
    float TimeStep { get; }
    float Oversampling { get; }

    IList<IGrannyTrackGroup> TrackGroups { get; }
  }


  public interface IGrannyTransform {
    GrannyTransformFlags Flags { get; }
    Vector3f Position { get; }
    Quaternion Orientation { get; }
    Vector3f[] ScaleShear { get; }
  }

  [Flags]
  public enum GrannyTransformFlags : uint {
    HasPosition = 0x1,
    HasOrientation = 0x2,
    HasScaleShear = 0x4,
  }
}