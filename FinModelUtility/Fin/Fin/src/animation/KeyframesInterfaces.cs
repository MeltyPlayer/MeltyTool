using System.Collections.Generic;

using schema.readOnly;

namespace fin.animation {
  [GenerateReadOnly]
  public partial interface IKeyframes<T> {
    void SetKeyframe(int frame, T value, string frameType = "");
    void SetAllKeyframes(IEnumerable<T> value);

    bool HasAtLeastOneKeyframe { get; }

    IReadOnlyList<Keyframe<T>> Definitions { get; }

    [Const]
    Keyframe<T> GetKeyframeAtIndex(int index);

    [Const]
    Keyframe<T>? GetKeyframeAtFrame(int frame);

    [Const]
    Keyframe<T>? GetKeyframeAtExactFrame(int frame);

    [Const]
    bool FindIndexOfKeyframe(
        int frame,
        out int keyframeIndex,
        out Keyframe<T> keyframe,
        out bool isLastKeyframe);
  }
}