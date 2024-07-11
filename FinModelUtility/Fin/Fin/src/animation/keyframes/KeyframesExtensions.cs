using System.Collections.Generic;
using System.Linq;

namespace fin.animation.keyframes;

public static class KeyframesExtensions {
  public static void SetKeyframe<T>(this IKeyframes<Keyframe<T>> keyframes,
                                    float frame,
                                    T value)
    => keyframes.Add(new Keyframe<T>(frame, value));

  public static void SetKeyframe<T>(
      this IKeyframes<KeyframeWithTangents<T>> keyframes,
      float frame,
      T value,
      float? tangent)
    => keyframes.Add(new KeyframeWithTangents<T>(frame, value, tangent));

  public static void SetKeyframe<T>(
      this IKeyframes<KeyframeWithTangents<T>> keyframes,
      float frame,
      T value,
      float? tangentIn,
      float? tangentOut)
    => keyframes.Add(
        new KeyframeWithTangents<T>(frame,
                                    value,
                                    value,
                                    tangentIn,
                                    tangentOut));

  public static void SetAllKeyframes<T>(this IKeyframes<Keyframe<T>> keyframes,
                                        IEnumerable<T> values) {
    foreach (var (frame, value) in values.Select((v, i) => (i, v))) {
      keyframes.Add(new Keyframe<T>(frame, value));
    }
  }
}