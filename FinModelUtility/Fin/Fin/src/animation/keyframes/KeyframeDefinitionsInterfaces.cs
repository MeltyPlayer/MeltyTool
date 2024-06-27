using System.Collections.Generic;

using schema.readOnly;

namespace fin.animation.keyframes;

[GenerateReadOnly]
public partial interface IKeyframeDefinitions<T> {
  void SetKeyframe(int frame, T value, string frameType = "");
  void SetAllKeyframes(IEnumerable<T> value);

  bool HasAtLeastOneKeyframe { get; }

  IReadOnlyList<KeyframeDefinition<T>> Definitions { get; }

  [Const]
  KeyframeDefinition<T> GetKeyframeAtIndex(int index);

  [Const]
  KeyframeDefinition<T>? GetKeyframeAtFrame(int frame);

  [Const]
  KeyframeDefinition<T>? GetKeyframeAtExactFrame(int frame);

  [Const]
  bool FindIndexOfKeyframe(
      int frame,
      out int keyframeIndex,
      out KeyframeDefinition<T> keyframe,
      out bool isLastKeyframe);
}