using fin.animation;
using fin.math.interpolation;
using fin.util.linq;

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

    private readonly IReadOnlyList<Keyframes<ValueAndTangents<float>>>
        transformKeyframes_;

    private readonly FloatInterpolator interpolator_ = new();

    public TtydGroupTransformKeyframes(
        IReadOnlyList<float> initialTransforms) {
      this.transforms_ = initialTransforms.ToArray();
      this.transformKeyframes_
          = initialTransforms
            .Select(v => {
                      var keyframes = new Keyframes<ValueAndTangents<float>>();
                      keyframes.SetKeyframe(0,
                                            new ValueAndTangents<float>(
                                                v,
                                                v,
                                                0,
                                                0));
                      return keyframes;
                    })
            .ToArray();
    }

    public void SetTransformAtFrame(int transformIndex,
                                    float frame,
                                    float deltaValue,
                                    float inTangent,
                                    float outTangent) {
      var value = this.transforms_[transformIndex] += deltaValue;

      var keyframes = this.transformKeyframes_[transformIndex];

      var intFrame = (int) frame;
      var keyframe = keyframes.GetKeyframeAtExactFrame(intFrame);
      if (keyframe != null) {
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

        keyframes.FindIndexOfKeyframe(
            frame,
            out var keyframeIndex,
            out var keyframe,
            out var isLastKeyframe);

        if (isLastKeyframe) {
          buffer[i] = keyframe.Value.OutgoingValue;
          continue;
        }

        var nextKeyframe = keyframes.GetKeyframeAtIndex(keyframeIndex + 1);
        buffer[i]
            = this.interpolator_
                  .Interpolate(keyframe.Frame,
                               keyframe.Value.OutgoingValue,
                               keyframe.Value.OutgoingTangent.Value,
                               nextKeyframe.Frame,
                               nextKeyframe.Value.IncomingValue,
                               nextKeyframe.Value.IncomingTangent.Value,
                               frame);
      }
    }
  }
}