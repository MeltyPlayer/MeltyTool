using ttyd.schema.model.blocks;

namespace ttyd.api;

// TODO: Optimize this
public class TtydGroupTransformKeyframes(
    float[] initialTransforms,
    int animationLength) {
  private readonly
      List<(float keyframe, IReadOnlyList<GroupTransformDelta> deltas)>
      keyframeTimesAndDeltas_ = new();

  public void AddDeltasForKeyframe(
      float keyframe,
      ReadOnlySpan<GroupTransformDelta> deltas) {
      this.keyframeTimesAndDeltas_.Add((keyframe, deltas.ToArray()));
    }

  public IGroupTransformBakedFrames BakeTransformsAtFrames() {
      var allTransformFrames
          = new float[initialTransforms.Length * animationLength];

      for (var i = 0; i < animationLength; ++i) {
        this.AnimationUpdate_(i, allTransformFrames);
      }

      return new TtydGroupTransformBakedFrames(initialTransforms.Length,
                                               allTransformFrames);
    }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/naclomi/noclip.website/blob/8b0de601d6d8f596683f0bdee61a9681a42512f9/src/PaperMarioTTYD/AnimGroup.ts#L903
  /// </summary>
  // TODO: Optimize this
  private void AnimationUpdate_(
      int frame,
      Span<float> allTransformFrames) {
      var allTransformsAtFrame = allTransformFrames.Slice(
          initialTransforms.Length * frame,
          initialTransforms.Length);

      initialTransforms.CopyTo(allTransformsAtFrame);

      if (this.keyframeTimesAndDeltas_.Count == 1) {
        AnimationUpdateFrameImmediate_(this.keyframeTimesAndDeltas_[0].deltas,
                                       allTransformsAtFrame);
        return;
      }

      var i1 = this.FindKeyframeIndex_(frame);
      var i0 = Math.Max(0, i1 - 1);

      for (var i = 0; i < i1; ++i) {
        AnimationUpdateFrameImmediate_(this.keyframeTimesAndDeltas_[i].deltas,
                                       allTransformsAtFrame);
      }

      if (i1 > i0) {
        var (time0, _) = this.keyframeTimesAndDeltas_[i0];
        var (time1, deltas1) = this.keyframeTimesAndDeltas_[i1];

        var duration = time1 - time0;
        var t = (frame - time0) / duration;

        AnimationUpdateFrame_(deltas1, allTransformsAtFrame, t, duration);
      }
    }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/naclomi/noclip.website/blob/8b0de601d6d8f596683f0bdee61a9681a42512f9/src/PaperMarioTTYD/AnimGroup.ts#L783
  /// </summary>
  private int FindKeyframeIndex_(int frame) {
      for (var i = 0; i < this.keyframeTimesAndDeltas_.Count; ++i) {
        var (keyframeTime, _) = this.keyframeTimesAndDeltas_[i];
        if (frame < keyframeTime) {
          return i;
        }
      }

      return -1;
    }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/naclomi/noclip.website/blob/8b0de601d6d8f596683f0bdee61a9681a42512f9/src/PaperMarioTTYD/AnimGroup.ts#L840
  /// </summary>
  // TODO: Optimize this
  private static void AnimationUpdateFrameImmediate_(
      IReadOnlyList<GroupTransformDelta> deltas,
      Span<float> allTransformsAtFrame) {
      var transformIndexAccumulator = 0;

      foreach (var delta in deltas) {
        transformIndexAccumulator += delta.IndexDelta;
        var valueDelta = delta.ValueDelta / 16f;
        allTransformsAtFrame[transformIndexAccumulator] += valueDelta;
      }
    }

  private static void AnimationUpdateFrame_(
      IReadOnlyList<GroupTransformDelta> deltas,
      Span<float> allTransformsAtFrame,
      float t,
      float duration) {
      var transformIndexAccumulator = 0;

      var t2 = t * t;
      var t3 = t2 * t;

      foreach (var delta in deltas) {
        transformIndexAccumulator += delta.IndexDelta;

        var valueDelta = delta.ValueDelta / 16f;
        if (delta.OutTangentDegrees == 90) {
          allTransformsAtFrame[transformIndexAccumulator] += valueDelta;
          continue;
        }

        var tangentIn = TtydTangents.GetTangent(delta.InTangentDegrees);
        var tangentOut = TtydTangents.GetTangent(delta.OutTangentDegrees);

        allTransformsAtFrame[transformIndexAccumulator] += (float) (
            (1.0 * t3 + -1.0 * t2 + 0.0 * t) * (duration * tangentOut) +
            (-2.0 * t3 + 3.0 * t2 + 0.0 * t) * valueDelta +
            (1.0 * t3 + -2.0 * t2 + 1.0 * t) * (duration * tangentIn)
        );
      }
    }
}