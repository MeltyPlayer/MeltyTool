using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

using fin.animation;
using fin.animation.interpolation;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public IAnimationManager AnimationManager { get; }

  private partial class AnimationManagerImpl : IAnimationManager {
    private readonly IModel model_;

    private readonly IList<IModelAnimation> animations_ =
        new List<IModelAnimation>();

    public AnimationManagerImpl(IModel model) {
      this.model_ = model;
      this.Animations =
          new ReadOnlyCollection<IModelAnimation>(this.animations_);
    }


    public IReadOnlyList<IModelAnimation> Animations { get; }

    public IModelAnimation AddAnimation() {
      var animation = new ModelAnimationImpl(this.model_.Skeleton.Count());
      this.animations_.Add(animation);
      return animation;
    }

    public void RemoveAnimation(IModelAnimation animation)
      => this.animations_.Remove(animation);
  }

  private partial class ModelAnimationImpl(int boneCount) : IModelAnimation {
    private SharedInterpolationConfig sharedInterpolationConfig_ = new() {
        Looping = true
    };

    public string Name { get; set; }

    public int FrameCount {
      get => this.sharedInterpolationConfig_.AnimationLength;
      set => this.sharedInterpolationConfig_.AnimationLength = value;
    }

    public float FrameRate { get; set; }

    public bool UseLoopingInterpolation {
      get => this.sharedInterpolationConfig_.Looping;
      set => this.sharedInterpolationConfig_.Looping = value;
    }

    public AnimationInterpolationMagFilter AnimationInterpolationMagFilter {
      get;
      set;
    }

    // TODO: Allow setting looping behavior (once, back and forth, etc.)
  }
}