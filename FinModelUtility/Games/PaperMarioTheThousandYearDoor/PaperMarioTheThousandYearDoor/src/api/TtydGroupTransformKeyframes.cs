using fin.animation;
using fin.math.floats;

using ttyd.schema.model.blocks;

namespace ttyd.api {
  // TODO: Optimize this
  public interface IGroupTransformKeyframes {
    void GetTransformsAtFrame(Group group,
                              int frame,
                              Span<float> buffer)
      => this.GetTransformsAtFrame(group, frame, 0, buffer.Length, buffer);

    void GetTransformsAtFrame(Group group,
                              int frame,
                              int start,
                              int count,
                              Span<float> buffer);
  }

  public class TtydGroupTransformKeyframes : IGroupTransformKeyframes {
    private readonly float[] transforms_;
    private readonly int animationLength_;
    private readonly bool isLooping_;

    private readonly IReadOnlyList<Keyframes<ValueAndTangents<float>>>
        transformKeyframes_;

    public TtydGroupTransformKeyframes(
        IReadOnlyList<float> initialTransforms,
        int animationLength,
        bool isLooping) {
      this.transforms_ = initialTransforms.ToArray();
      this.animationLength_ = animationLength;
      this.isLooping_ = isLooping;

      this.transformKeyframes_
          = initialTransforms
            .Select(v => {
                      var keyframes = new Keyframes<ValueAndTangents<float>>();
                      if (!isLooping) {
                        keyframes.SetKeyframe(0,
                                              new ValueAndTangents<float>(
                                                  v,
                                                  v,
                                                  0,
                                                  0));
                      }

                      return keyframes;
                    })
            .ToArray();
    }

    public void SetTransformAtFrame(int transformIndex,
                                    float frame,
                                    float deltaValue,
                                    float inTangent,
                                    float outTangent) {
      if (deltaValue == 0 && inTangent == 0 && outTangent == 0) {
        return;
      }

      var value = this.transforms_[transformIndex] += deltaValue;

      var keyframes = this.transformKeyframes_[transformIndex];

      var intFrame = (int) frame;
      var keyframe = keyframes.GetKeyframeAtExactFrame(intFrame);
      if (keyframe != null && frame > 0) {
        var valueAndTangents = keyframe.Value.Value;
        keyframes.SetKeyframe(
            intFrame,
            valueAndTangents with {
                OutgoingValue = value,
                OutgoingTangent = outTangent
            });
      } else {
        keyframes.SetKeyframe(
            intFrame,
            new ValueAndTangents<float>(value, inTangent, outTangent));
      }
    }

    public void GetTransformsAtFrame(
        Group group,
        int frame,
        int start,
        int length,
        Span<float> buffer) {
      for (var i = 0; i < length; ++i) {
        var transformIndex = group.TransformBaseIndex + start + i;
        var keyframes = this.transformKeyframes_[transformIndex];

        if (!keyframes.HasAtLeastOneKeyframe) {
          buffer[i] = this.transforms_[transformIndex];
          continue;
        }

        var firstKeyframe = keyframes.Definitions.First();
        var beforeFirstKeyframe = frame < firstKeyframe.Frame;
        if (keyframes.Definitions.Count == 1 ||
            (!this.isLooping_ && beforeFirstKeyframe)) {
          buffer[i] = beforeFirstKeyframe
              ? firstKeyframe.Value.IncomingValue
              : firstKeyframe.Value.OutgoingValue;
          continue;
        }

        keyframes.FindIndexOfKeyframe(
            frame,
            out var keyframeIndex,
            out var fromKeyframe,
            out var isLastKeyframe);

        if (!this.isLooping_ && isLastKeyframe) {
          buffer[i] = fromKeyframe.Value.OutgoingValue;
          continue;
        }

        Keyframe<ValueAndTangents<float>> nextKeyframe;
        if (beforeFirstKeyframe) {
          nextKeyframe = firstKeyframe;
          fromKeyframe = keyframes.Definitions.Last();
        } else if (!isLastKeyframe) {
          nextKeyframe = keyframes.GetKeyframeAtIndex(keyframeIndex + 1);
        } else {
          nextKeyframe = firstKeyframe;
        }

        var fromTime = fromKeyframe.Frame;
        var toTime = nextKeyframe.Frame;
        if (toTime <= fromTime) {
          if (frame > fromTime) {
            toTime += this.animationLength_;
          } else {
            fromTime -= this.animationLength_;
          }
        }

        buffer[i] = Interpolate_(fromTime,
                                 fromKeyframe.Value.OutgoingValue,
                                 fromKeyframe.Value.OutgoingTangent.Value,
                                 toTime,
                                 nextKeyframe.Value.IncomingValue,
                                 nextKeyframe.Value.IncomingTangent.Value,
                                 frame);
      }
    }

    private static float Interpolate_(float fromTime,
                                      float fromValue,
                                      float fromTangent,
                                      float toTime,
                                      float toValue,
                                      float toTangent,
                                      float frame) {
      var delta = toValue - fromValue;

      var duration = toTime - fromTime;
      var t = (frame - fromTime) / duration;

      if (t.IsRoughly1() || float.IsInfinity(fromTangent)) {
        return fromValue + delta;
      }

      var t2 = t * t;
      var t3 = t2 * t;
      // Tangents are already converted when parsing the frames, and the 1/16.0 is also applied there.
      return fromValue +
             (float) (
                 (1.0 * t3 + -1.0 * t2 + 0.0 * t) * (duration * fromTangent) +
                 (-2.0 * t3 + 3.0 * t2 + 0.0 * t) * delta +
                 (1.0 * t3 + -2.0 * t2 + 1.0 * t) * (duration * toTangent)
             );
    }
  }
}