using System;

using fin.math.transform;
using fin.model;

namespace fin.scene.components;

public interface IModelManager;

public interface ITransformedModelManager : IModelManager {
  ITransform3d Transform { get; }
}

public interface IAnimatedModelManager : ITransformedModelManager {
  IAnimationManager AnimationManager { get; }
}

public readonly struct AnimationPlaybackArgs {
  public required IReadOnlyAnimation Animation { get; init; }
  public IndefiniteRepeatArgs RepeatArgs { get; init; }
  public Action? OnCompletion { get; init; }
}

public interface IAnimationManager {
  float AnimationSpeed { get; set; }

  // TODO: Support getting a promise when an animation completes or is interrupted 
  void PlayAnimation(AnimationPlaybackArgs args);

  void InterpolateToAnimation(IAnimation animation,
                              float seconds,
                              bool interruptible);
}

public readonly struct SequenceAnimationStepArgs {
  public required IReadOnlyAnimation Animation { get; init; }
  public required DefiniteRepeatArgs RepeatArgs { get; init; }
  public Action? OnCompletion { get; init; }
}

public static class AnimationManagerExtensions {
  extension(IAnimationManager animationManager) {
    public void PlaySequence(
        params ReadOnlySpan<SequenceAnimationStepArgs> steps) { }
  }
}