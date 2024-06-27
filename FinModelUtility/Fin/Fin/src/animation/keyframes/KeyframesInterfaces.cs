using System.Collections.Generic;

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

public interface IReadOnlyKeyframes<out TKeyframe> where TKeyframe : IKeyframe {
  IReadOnlyList<TKeyframe> Definitions { get; }
}

public interface IKeyframes<TKeyframe> : IReadOnlyKeyframes<TKeyframe>
    where TKeyframe : IKeyframe {
  void Add(TKeyframe keyframe);
}