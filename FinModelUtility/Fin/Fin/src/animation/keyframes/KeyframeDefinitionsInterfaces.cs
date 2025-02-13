using System.Collections.Generic;

using schema.readOnly;

namespace fin.animation.keyframes;

[GenerateReadOnly]
public partial interface IKeyframeDefinitions<T> {
  void SetKeyframe(int frame, T value, string frameType = "");
  void SetAllKeyframes(IEnumerable<T> value);

  new bool HasAtLeastOneKeyframe { get; }

  new IReadOnlyList<KeyframeDefinition<T>> Definitions { get; }

  [Const]
  new KeyframeDefinition<T> GetKeyframeAtIndex(int index);

  [Const]
  new KeyframeDefinition<T>? GetKeyframeAtFrame(int frame);

  [Const]
  new KeyframeDefinition<T>? GetKeyframeAtExactFrame(int frame);

  [Const]
  new bool FindIndexOfKeyframe(
      int frame,
      out int keyframeIndex,
      out KeyframeDefinition<T> keyframe,
      out bool isLastKeyframe);
}