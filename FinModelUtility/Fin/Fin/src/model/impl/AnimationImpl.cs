using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

using fin.animation;
using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.tracks;
using fin.data.indexable;
using fin.math.interpolation;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public IAnimationManager AnimationManager { get; }

  private class AnimationManagerImpl : IAnimationManager {
    private readonly IModel model_;

    private readonly IList<IModelAnimation> animations_ =
        new List<IModelAnimation>();

    private readonly IList<IMorphTarget> morphTargets_ =
        new List<IMorphTarget>();

    public AnimationManagerImpl(IModel model) {
      this.model_ = model;
      this.Animations =
          new ReadOnlyCollection<IModelAnimation>(this.animations_);
      this.MorphTargets =
          new ReadOnlyCollection<IMorphTarget>(this.morphTargets_);
    }


    public IReadOnlyList<IModelAnimation> Animations { get; }

    public IModelAnimation AddAnimation() {
      var animation = new ModelAnimationImpl(this.model_.Skeleton.Count());
      this.animations_.Add(animation);
      return animation;
    }

    public void RemoveAnimation(IModelAnimation animation)
      => this.animations_.Remove(animation);


    public IReadOnlyList<IMorphTarget> MorphTargets { get; }

    public IMorphTarget AddMorphTarget() {
      var morphTarget = new MorphTargetImpl();
      this.morphTargets_.Add(morphTarget);
      return morphTarget;
    }

    private class MorphTargetImpl : IMorphTarget {
      private Dictionary<IReadOnlyVertex, Vector3> morphs_ = new();

      public MorphTargetImpl() {
        this.Morphs =
            new ReadOnlyDictionary<IReadOnlyVertex, Vector3>(this.morphs_);
      }

      public string Name { get; set; }
      public IReadOnlyDictionary<IReadOnlyVertex, Vector3> Morphs { get; }

      public IMorphTarget MoveTo(IReadOnlyVertex vertex, Vector3 position) {
        this.morphs_[vertex] = position;
        return this;
      }
    }
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