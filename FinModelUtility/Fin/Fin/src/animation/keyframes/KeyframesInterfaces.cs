using System.Collections.Generic;

using schema.readOnly;

namespace fin.animation.keyframes;

public interface IKeyframe {
  float Frame { get; }
}

public interface IKeyframe<out T> : IKeyframe {
  T Value { get; }
}

public interface IKeyframeWithTangents : IKeyframe {
  float TangentIn { get; }
  float TangentOut { get; }
}

public interface IKeyframeWithTangents<out T>
    : IKeyframe<T>, IKeyframeWithTangents;

[GenerateReadOnly]
public partial interface IKeyframes<TKeyframe> where TKeyframe : IKeyframe {
  IReadOnlyList<TKeyframe> Definitions { get; }
  void Add(TKeyframe keyframe);
}