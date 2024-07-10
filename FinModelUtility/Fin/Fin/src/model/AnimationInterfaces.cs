using System.Collections.Generic;
using System.Numerics;

using fin.animation;
using fin.animation.keyframes;
using fin.animation.tracks;
using fin.data.indexable;
using fin.math.interpolation;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IAnimationManager {
  IReadOnlyList<IModelAnimation> Animations { get; }
  IModelAnimation AddAnimation();
  void RemoveAnimation(IModelAnimation animation);

  IReadOnlyList<IMorphTarget> MorphTargets { get; }
  IMorphTarget AddMorphTarget();
}

public interface IMorphTarget {
  string Name { get; set; }

  IReadOnlyDictionary<IReadOnlyVertex, Vector3> Morphs { get; }
  IMorphTarget MoveTo(IReadOnlyVertex vertex, Vector3 position);
}

[GenerateReadOnly]
public partial interface IAnimation {
  string Name { get; set; }

  int FrameCount { get; set; }
  float FrameRate { get; set; }
  bool UseLoopingInterpolation { get; set; }
  AnimationInterpolationMagFilter AnimationInterpolationMagFilter { get; set; }
}

[GenerateReadOnly]
public partial interface IModelAnimation : IAnimation {
  IReadOnlyIndexableDictionary<IReadOnlyBone, IBoneTracks> BoneTracks { get; }
  IBoneTracks AddBoneTracks(IReadOnlyBone bone);

  IReadOnlyIndexableDictionary<IReadOnlyMesh, IMeshTracks> MeshTracks { get; }
  IMeshTracks AddMeshTracks(IReadOnlyMesh mesh);

  IReadOnlyIndexableDictionary<IReadOnlyTexture, ITextureTracks> TextureTracks { get; }
  ITextureTracks AddTextureTracks(IReadOnlyTexture texture);

  // TODO: Allow setting looping behavior (once, back and forth, etc.)
}

[GenerateReadOnly]
public partial interface IAnimationData {
  IAnimation Animation { get; }
}

[GenerateReadOnly]
public partial interface IBoneTracks : IAnimationData {
  IReadOnlyBone Bone { get; }

  IPositionTrack3d? Positions { get; }
  IRotationTrack3d? Rotations { get; }
  IScale3dTrack? Scales { get; }

  ICombinedPositionAxesTrack3d UseCombinedPositionAxesTrack(
      int initialCapacity = 0);

  ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(
      int initialCapacity = 0);

  ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(
      int initialXCapacity,
      int initialYCapacity,
      int initialZCapacity);

  IQuaternionRotationTrack3d
      UseQuaternionRotationTrack(int initialCapacity = 0);

  IQuaternionAxesRotationTrack3d UseQuaternionAxesRotationTrack();

  IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
      int initialCapacity = 0);

  IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
      int initialXCapacity,
      int initialYCapacity,
      int initialZCapacity);

  IScale3dTrack UseScaleTrack(int initialCapacity = 0);

  IScale3dTrack UseScaleTrack(
      int initialXCapacity,
      int initialYCapacity,
      int initialZCapacity);
}

public enum MeshDisplayState {
  UNDEFINED,
  HIDDEN,
  VISIBLE,
}

[GenerateReadOnly]
public partial interface IMeshTracks {
  IReadOnlyMesh Mesh { get; }
  IStairStepKeyframes<MeshDisplayState> DisplayStates { get; }
}

[GenerateReadOnly]
public partial interface ITextureTracks {
  IReadOnlyTexture Texture { get; }
}


// TODO: Add a track for animating params, e.g. textures, UVs, frames.