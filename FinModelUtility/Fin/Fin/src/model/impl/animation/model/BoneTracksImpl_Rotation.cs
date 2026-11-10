using System.Numerics;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.animation.types.radians;
using fin.animation.types.single;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class BoneTracksImpl {
    public IQuaternionInterpolatable? Rotations { get; private set; }

    public ISeparateQuaternionKeyframes<Keyframe<float>>
        UseSeparateQuaternionKeyframes(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity,
            int initialWCapacity) {
      var keyframes = new SeparateQuaternionKeyframes<Keyframe<float>>(
          sharedConfig,
          FloatKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.X),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.Y),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.Z),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialWCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.W),
          });

      this.Rotations = keyframes;

      return keyframes;
    }

    public ISeparateQuaternionKeyframes<KeyframeWithTangents<float>>
        UseSeparateQuaternionKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity,
            int initialWCapacity) {
      var keyframes
          = new SeparateQuaternionKeyframes<KeyframeWithTangents<float>>(
          sharedConfig,
          FloatKeyframeWithTangentsInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.X),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.Y),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.Z),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialWCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalRotation.W),
          });

      this.Rotations = keyframes;

      return keyframes;
    }

    public ICombinedQuaternionKeyframes<Keyframe<Quaternion>>
        UseCombinedQuaternionKeyframes(int initialCapacity = 0) {
      var keyframes = new CombinedQuaternionKeyframes<Keyframe<Quaternion>>(
          sharedConfig,
          QuaternionKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<Quaternion> {
              InitialCapacity = initialCapacity,
              DefaultValue = Optional.Of(bone.Transform.LocalRotation),
          });

      this.Rotations = keyframes;

      return keyframes;
    }

    public ISeparateEulerRadiansKeyframes<Keyframe<float>>
        UseSeparateEulerRadiansKeyframes(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes = new SeparateEulerRadiansKeyframes<Keyframe<float>>(
          sharedConfig,
          RadiansKeyframeInterpolator.Instance,
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialXCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalEulerRadians.X),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialYCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalEulerRadians.Y),
          },
          new IndividualInterpolationConfig<float> {
              InitialCapacity = initialZCapacity,
              DefaultValue
                  = Optional.Of(bone.Transform.LocalEulerRadians.Z),
          });

      this.Rotations = keyframes;

      return keyframes;
    }

    public ISeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>
        UseSeparateEulerRadiansKeyframesWithTangents(
            int initialXCapacity,
            int initialYCapacity,
            int initialZCapacity) {
      var keyframes
          = new SeparateEulerRadiansKeyframes<KeyframeWithTangents<float>>(
              sharedConfig,
              RadiansKeyframeWithTangentsInterpolator.Instance,
              new IndividualInterpolationConfig<float> {
                  InitialCapacity = initialXCapacity,
                  DefaultValue = Optional.Of(
                      bone.Transform.LocalEulerRadians.X),
              },
              new IndividualInterpolationConfig<float> {
                  InitialCapacity = initialYCapacity,
                  DefaultValue = Optional.Of(
                      bone.Transform.LocalEulerRadians.Y),
              },
              new IndividualInterpolationConfig<float> {
                  InitialCapacity = initialZCapacity,
                  DefaultValue = Optional.Of(
                      bone.Transform.LocalEulerRadians.Z),
              });

      this.Rotations = keyframes;

      return keyframes;
    }
  }
}