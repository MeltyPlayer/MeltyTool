using System.Collections.Generic;
using System.Numerics;

using fin.animation;
using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.animation.types.vector3;
using fin.data.indexable;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IAnimationManager {
  new IReadOnlyList<IModelAnimation> Animations { get; }
  IModelAnimation AddAnimation();
  void RemoveAnimation(IModelAnimation animation);

  new IReadOnlyList<IMorphTarget> MorphTargets { get; }
  IMorphTarget AddMorphTarget();
}

public interface IMorphTarget {
  string Name { get; set; }

  IReadOnlyDictionary<IReadOnlyVertex, Vector3> Morphs { get; }
  IMorphTarget MoveTo(IReadOnlyVertex vertex, Vector3 position);
}

[GenerateReadOnly]
public partial interface IAnimation {
  new string Name { get; set; }

  new int FrameCount { get; set; }
  new float FrameRate { get; set; }
  new bool UseLoopingInterpolation { get; set; }

  new AnimationInterpolationMagFilter AnimationInterpolationMagFilter {
    get;
    set;
  }
}

[GenerateReadOnly]
public partial interface IModelAnimation : IAnimation {
  new IReadOnlyIndexableDictionary<IReadOnlyBone, IBoneTracks> BoneTracks {
    get;
  }

  IBoneTracks GetOrCreateBoneTracks(IReadOnlyBone bone);

  new IReadOnlyIndexableDictionary<IReadOnlyMesh, IMeshTracks> MeshTracks {
    get;
  }

  IMeshTracks AddMeshTracks(IReadOnlyMesh mesh);

  new IReadOnlyIndexableDictionary<IReadOnlyTexture, ITextureTracks>
      TextureTracks { get; }

  ITextureTracks AddTextureTracks(IReadOnlyTexture texture);

  // TODO: Allow setting looping behavior (once, back and forth, etc.)
}

[GenerateReadOnly]
public partial interface IAnimationData {
  new IAnimation Animation { get; }
}

[GenerateReadOnly]
public partial interface IBoneTracks : IAnimationData {
  new IReadOnlyBone Bone { get; }

  new IVector3Interpolatable? Translations { get; }
  new IQuaternionInterpolatable? Rotations { get; }
  new IVector3Interpolatable? Scales { get; }

  // Translation
  ISeparateVector3Keyframes<Keyframe<float>> UseSeparateTranslationKeyframes(
      int initialCapacity = 0)
    => this.UseSeparateTranslationKeyframes(initialCapacity,
                                            initialCapacity,
                                            initialCapacity);

  ISeparateVector3Keyframes<Keyframe<float>> UseSeparateTranslationKeyframes(
      int initialXCapacity,
      int initialYCapacity,
      int initialZCapacity);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateTranslationKeyframesWithTangents(int initialCapacity = 0)
    => this.UseSeparateTranslationKeyframesWithTangents(initialCapacity,
      initialCapacity,
      initialCapacity);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateTranslationKeyframesWithTangents(int initialXCapacity,
                                                  int initialYCapacity,
                                                  int initialZCapacity);

  ICombinedVector3Keyframes<Keyframe<Vector3>> UseCombinedTranslationKeyframes(
      int initialCapacity = 0);

  ICombinedVector3Keyframes<KeyframeWithTangents<Vector3>>
      UseCombinedTranslationKeyframesWithTangents(int initialCapacity = 0);

  // Rotation
  ISeparateQuaternionKeyframes<Keyframe<float>>
      UseSeparateQuaternionKeyframes(int initialCapacity = 0)
    => this.UseSeparateQuaternionKeyframes(initialCapacity,
                                           initialCapacity,
                                           initialCapacity,
                                           initialCapacity);

  ISeparateQuaternionKeyframes<Keyframe<float>>
      UseSeparateQuaternionKeyframes(int initialXCapacity,
                                     int initialYCapacity,
                                     int initialZCapacity,
                                     int initialWCapacity);

  ISeparateQuaternionKeyframes<KeyframeWithTangents<float>>
      UseSeparateQuaternionKeyframesWithTangents(int initialCapacity = 0)
    => this.UseSeparateQuaternionKeyframesWithTangents(initialCapacity,
      initialCapacity,
      initialCapacity,
      initialCapacity);

  ISeparateQuaternionKeyframes<KeyframeWithTangents<float>>
      UseSeparateQuaternionKeyframesWithTangents(int initialXCapacity,
                                                 int initialYCapacity,
                                                 int initialZCapacity,
                                                 int initialWCapacity);

  ICombinedQuaternionKeyframes<Keyframe<Quaternion>>
      UseCombinedQuaternionKeyframes(int initialCapacity = 0);

  ISeparateEulerRadiansKeyframes<Keyframe<float>>
      UseSeparateEulerRadiansKeyframes(int initialCapacity = 0)
    => this.UseSeparateEulerRadiansKeyframes(initialCapacity,
                                             initialCapacity,
                                             initialCapacity);

  ISeparateEulerRadiansKeyframes<Keyframe<float>>
      UseSeparateEulerRadiansKeyframes(int initialXCapacity,
                                       int initialYCapacity,
                                       int initialZCapacity);

  ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
      UseSeparateEulerRadiansKeyframesWithTangents(int initialCapacity = 0)
    => this.UseSeparateEulerRadiansKeyframesWithTangents(initialCapacity,
      initialCapacity,
      initialCapacity);

  ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
      UseSeparateEulerRadiansKeyframesWithTangents(int initialXCapacity,
                                                   int initialYCapacity,
                                                   int initialZCapacity);

  // Scale
  ISeparateVector3Keyframes<Keyframe<float>> UseSeparateScaleKeyframes(
      int initialCapacity = 0)
    => this.UseSeparateScaleKeyframes(initialCapacity,
                                      initialCapacity,
                                      initialCapacity);

  ISeparateVector3Keyframes<Keyframe<float>> UseSeparateScaleKeyframes(
      int initialXCapacity,
      int initialYCapacity,
      int initialZCapacity);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateScaleKeyframesWithTangents(int initialCapacity = 0)
    => this.UseSeparateScaleKeyframesWithTangents(initialCapacity,
                                                  initialCapacity,
                                                  initialCapacity);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateScaleKeyframesWithTangents(int initialXCapacity,
                                            int initialYCapacity,
                                            int initialZCapacity);

  ICombinedVector3Keyframes<Keyframe<Vector3>> UseCombinedScaleKeyframes(
      int initialCapacity = 0);

  ICombinedVector3Keyframes<KeyframeWithTangents<Vector3>>
      UseCombinedScaleKeyframesWithTangents(int initialCapacity = 0);
}

public enum MeshDisplayState {
  UNDEFINED,
  HIDDEN,
  VISIBLE,
}

[GenerateReadOnly]
public partial interface IMeshTracks {
  new IReadOnlyMesh Mesh { get; }
  new IStairStepKeyframes<MeshDisplayState> DisplayStates { get; }
}

[GenerateReadOnly]
public partial interface ITextureTracks {
  new IReadOnlyTexture Texture { get; }

  new ISeparateVector3Keyframes<KeyframeWithTangents<float>>? Translations {
    get;
  }

  new ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>? Rotations {
    get;
  }

  new ISeparateVector3Keyframes<KeyframeWithTangents<float>>? Scales { get; }

  // Translation
  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateTranslationKeyframesWithTangents(int initialCapacity = 0,
                                                  int? animationLength = null)
    => this.UseSeparateTranslationKeyframesWithTangents(initialCapacity,
      initialCapacity,
      initialCapacity,
      animationLength);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateTranslationKeyframesWithTangents(int initialXCapacity,
                                                  int initialYCapacity,
                                                  int initialZCapacity,
                                                  int? animationLength = null);

  // Rotation
  ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
      UseSeparateRotationKeyframesWithTangents(int initialCapacity = 0,
                                               int? animationLength = null)
    => this.UseSeparateRotationKeyframesWithTangents(initialCapacity,
      initialCapacity,
      initialCapacity,
      animationLength);

  ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
      UseSeparateRotationKeyframesWithTangents(int initialXCapacity,
                                               int initialYCapacity,
                                               int initialZCapacity,
                                               int? animationLength = null);

  // Scale
  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateScaleKeyframesWithTangents(int initialCapacity = 0,
                                            int? animationLength = null)
    => this.UseSeparateScaleKeyframesWithTangents(initialCapacity,
                                                  initialCapacity,
                                                  initialCapacity,
                                                  animationLength);

  ISeparateVector3Keyframes<KeyframeWithTangents<float>>
      UseSeparateScaleKeyframesWithTangents(int initialXCapacity,
                                            int initialYCapacity,
                                            int initialZCapacity,
                                            int? animationLength = null);
}


// TODO: Add a track for animating params, e.g. textures, UVs, frames.