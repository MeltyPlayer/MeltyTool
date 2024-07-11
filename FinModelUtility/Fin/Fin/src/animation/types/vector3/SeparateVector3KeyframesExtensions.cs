using System.Numerics;

using fin.animation.keyframes;

namespace fin.animation.types.vector3;

public static class SeparateVector3KeyframesExtensions {
  public static void Add(
      this ISeparateVector3Keyframes<Keyframe<float>> keyframes,
      Keyframe<Vector3> keyframe) {
    keyframes.Axes[0]
             .Add(new Keyframe<float>(keyframe.Frame,
                                      keyframe.ValueIn.X,
                                      keyframe.ValueOut.X));
    keyframes.Axes[1]
             .Add(new Keyframe<float>(keyframe.Frame,
                                      keyframe.ValueIn.Y,
                                      keyframe.ValueOut.Y));
    keyframes.Axes[2]
             .Add(new Keyframe<float>(keyframe.Frame,
                                      keyframe.ValueIn.Z,
                                      keyframe.ValueOut.Z));
  }

  public static void Add(
      this ISeparateVector3Keyframes<KeyframeWithTangents<float>> keyframes,
      KeyframeWithTangents<Vector3> keyframe) {
    keyframes.Axes[0]
             .Add(new KeyframeWithTangents<float>(keyframe.Frame,
                                                  keyframe.ValueIn.X,
                                                  keyframe.ValueOut.X,
                                                  keyframe.TangentIn,
                                                  keyframe.TangentOut));
    keyframes.Axes[1]
             .Add(new KeyframeWithTangents<float>(keyframe.Frame,
                                                  keyframe.ValueIn.Y,
                                                  keyframe.ValueOut.Y,
                                                  keyframe.TangentIn,
                                                  keyframe.TangentOut));
    keyframes.Axes[2]
             .Add(new KeyframeWithTangents<float>(keyframe.Frame,
                                                  keyframe.ValueIn.Z,
                                                  keyframe.ValueOut.Z,
                                                  keyframe.TangentIn,
                                                  keyframe.TangentOut));
  }
}