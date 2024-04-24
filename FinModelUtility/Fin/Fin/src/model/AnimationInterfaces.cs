using System.Collections.Generic;

using fin.animation;
using fin.data.indexable;
using fin.math.interpolation;

using schema.readOnly;

namespace fin.model {
  public interface IAnimationManager {
    IReadOnlyList<IModelAnimation> Animations { get; }
    IModelAnimation AddAnimation();
    void RemoveAnimation(IModelAnimation animation);

    IReadOnlyList<IMorphTarget> MorphTargets { get; }
    IMorphTarget AddMorphTarget();
  }

  public interface IMorphTarget {
    string Name { get; set; }

    IReadOnlyDictionary<IVertex, Position> Morphs { get; }
    IMorphTarget MoveTo(IVertex vertex, Position position);
  }

  [GenerateReadOnly]
  public partial interface IAnimation {
    string Name { get; set; }

    int FrameCount { get; set; }
    float FrameRate { get; set; }
    AnimationInterpolationMagFilter AnimationInterpolationMagFilter { get; set; }
  }

  [GenerateReadOnly]
  public partial interface IModelAnimation : IAnimation {
    IReadOnlyIndexableDictionary<IReadOnlyBone, IBoneTracks> BoneTracks { get; }
    IBoneTracks AddBoneTracks(IReadOnlyBone bone);

    IReadOnlyDictionary<IMesh, IMeshTracks> MeshTracks { get; }
    IMeshTracks AddMeshTracks(IMesh mesh);

    IReadOnlyDictionary<ITexture, ITextureTracks> TextureTracks { get; }
    ITextureTracks AddTextureTracks(ITexture texture);

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

    ICombinedPositionAxesTrack3d UseCombinedPositionAxesTrack(int initialCapacity = 0);
    ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(int initialCapacity = 0);
    ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(
        int initialXCapacity,
        int initialYCapacity,
        int initialZCapacity);

    IQuaternionRotationTrack3d UseQuaternionRotationTrack(int initialCapacity = 0);

    IQuaternionAxesRotationTrack3d UseQuaternionAxesRotationTrack();

    IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(int initialCapacity = 0);

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

  public interface IMeshTracks : IAnimationData {
    IInputOutputTrack<MeshDisplayState, StairStepInterpolator<MeshDisplayState>>
        DisplayStates { get; }
  }



  public interface ITextureTracks : IAnimationData {
  }


  // TODO: Add a track for animating params, e.g. textures, UVs, frames.
}