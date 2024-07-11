using System.Collections.Generic;

using fin.animation;
using fin.animation.keyframes;
using fin.animation.types.vector3;
using fin.model;

namespace mod.schema.anm {
  public static class DcxHelpers {
    public static IModelAnimation AddAnimation(
        IReadOnlyList<IBone> bones,
        IAnimationManager animationManager,
        IDcx dcx) {
      var isDck = dcx is Dck;
      var dcxAnimationData = dcx.AnimationData;

      var animation = animationManager.AddAnimation();
      animation.Name = dcx.Name;
      animation.FrameCount = (int) dcxAnimationData.FrameCount;
      animation.FrameRate = 30;

      foreach (var jointData in dcxAnimationData.JointDataList) {
        var jointIndex = jointData.JointIndex;

        var jointKeyframes = animation.AddBoneTracks(bones[jointIndex]);

        KeyframeDefinition<ValueAndTangents<float>>[][] frames;

        frames = DcxHelpers.ReadKeyframes_(
            isDck,
            dcxAnimationData,
            jointData.ScaleAxes,
            dcxAnimationData.ScaleValues);
        DcxHelpers.MergeKeyframesToScaleTrack(
            frames,
            jointKeyframes.UseSeparateScaleKeyframesWithTangents());

        frames = DcxHelpers.ReadKeyframes_(
            isDck,
            dcxAnimationData,
            jointData.RotationAxes,
            dcxAnimationData.RotationValues);
        DcxHelpers.MergeKeyframesToRotationTrack(
            frames,
            jointKeyframes.UseEulerRadiansRotationTrack());

        frames = DcxHelpers.ReadKeyframes_(
            isDck,
            dcxAnimationData,
            jointData.PositionAxes,
            dcxAnimationData.PositionValues);
        DcxHelpers.MergeKeyframesToPositionTrack(
            frames,
            jointKeyframes.UseSeparatePositionAxesTrack());
      }

      return animation;
    }

    private static KeyframeDefinition<ValueAndTangents<float>>[][]
        ReadKeyframes_(
            bool isDck,
            IDcxAnimationData animationData,
            IDcxAxes axes,
            float[] values) {
      var frames = new KeyframeDefinition<ValueAndTangents<float>>[3][];
      for (var i = 0; i < 3; ++i) {
        var axis = axes.Axes[i];

        var frameCount = axis.FrameCount;
        var frameOffset = axis.FrameOffset;

        var sparse = isDck && frameCount != 1;
        frames[i] = !sparse
            ? DcxHelpers.ReadDenseFrames(
                values,
                frameOffset,
                frameCount)
            : DcxHelpers.ReadSparseFrames(
                values,
                frameOffset,
                frameCount);
      }

      return frames;
    }

    public static KeyframeDefinition<ValueAndTangents<float>>[] ReadDenseFrames(
        float[] values,
        int offset,
        int count
    ) {
      var keyframes = new KeyframeDefinition<ValueAndTangents<float>>[count];
      for (var i = 0; i < count; ++i) {
        keyframes[i] =
            new KeyframeDefinition<ValueAndTangents<float>>(
                i,
                new ValueAndTangents<float>(values[offset + i]));
      }

      return keyframes;
    }

    public static KeyframeDefinition<ValueAndTangents<float>>[]
        ReadSparseFrames(
            float[] values,
            int offset,
            int count
        ) {
      var keyframes = new KeyframeDefinition<ValueAndTangents<float>>[count];
      for (var i = 0; i < count; ++i) {
        var index = (int) values[offset + 3 * i];
        var value = values[offset + 3 * i + 1];

        // TODO: This is a guess, is this actually right?
        // The tangents are HUGE, have to be scaled down by the FPS.
        var tangent = values[offset + 3 * i + 2] / 30f;

        keyframes[i] =
            new KeyframeDefinition<ValueAndTangents<float>>(
                index,
                new ValueAndTangents<float>(value, tangent));
      }

      return keyframes;
    }

    // TODO: Do this sparsely
    public static void MergeKeyframesToPositionTrack(
        KeyframeDefinition<ValueAndTangents<float>>[][] positionKeyframes,
        ISeparatePositionAxesTrack3d positionTrack) {
      for (var i = 0; i < 3; ++i) {
        foreach (var keyframe in positionKeyframes[i]) {
          positionTrack.Set(keyframe.Frame,
                            i,
                            keyframe.Value.IncomingValue,
                            keyframe.Value.OutgoingValue,
                            keyframe.Value.IncomingTangent,
                            keyframe.Value.OutgoingTangent);
        }
      }
    }

    public static void MergeKeyframesToRotationTrack(
        KeyframeDefinition<ValueAndTangents<float>>[][] rotationKeyframes,
        IEulerRadiansRotationTrack3d rotationTrack) {
      for (var i = 0; i < 3; ++i) {
        foreach (var keyframe in rotationKeyframes[i]) {
          rotationTrack.Set(keyframe.Frame,
                            i,
                            keyframe.Value.IncomingValue,
                            keyframe.Value.OutgoingValue,
                            keyframe.Value.IncomingTangent,
                            keyframe.Value.OutgoingTangent);
        }
      }
    }

    public static void MergeKeyframesToScaleTrack(
        KeyframeDefinition<ValueAndTangents<float>>[][] scaleKeyframes,
        ISeparateVector3Keyframes<KeyframeWithTangents<float>> scaleTrack) {
      for (var i = 0; i < 3; ++i) {
        foreach (var keyframe in scaleKeyframes[i]) {
          scaleTrack.Axes[i]
                    .Add(new KeyframeWithTangents<float>(keyframe.Frame,
                           keyframe.Value.IncomingValue,
                           keyframe.Value.OutgoingValue,
                           keyframe.Value.IncomingTangent,
                           keyframe.Value.OutgoingTangent));
        }
      }
    }
  }
}