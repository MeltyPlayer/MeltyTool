using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using fin.animation;
using fin.data.indexable;
using fin.math.interpolation;

namespace fin.model.impl {
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

      private class ModelAnimationImpl : IModelAnimation {
        private readonly IndexableDictionary<IReadOnlyBone, IBoneTracks> boneTracks_;
        private readonly Dictionary<IReadOnlyMesh, IMeshTracks> meshTracks_ = new();

        public ModelAnimationImpl(int boneCount) {
          this.boneTracks_ =
              new IndexableDictionary<IReadOnlyBone, IBoneTracks>(boneCount);
        }

        public string Name { get; set; }

        public int FrameCount { get; set; }
        public float FrameRate { get; set; }
        public bool UseLoopingInterpolation { get; set; } = true;
        public AnimationInterpolationMagFilter AnimationInterpolationMagFilter { get; set; }

        public IReadOnlyIndexableDictionary<IReadOnlyBone, IBoneTracks> BoneTracks
          => this.boneTracks_;

        public IBoneTracks AddBoneTracks(IReadOnlyBone bone)
          => this.boneTracks_[bone] = new BoneTracksImpl(this, bone);

        public IReadOnlyDictionary<IReadOnlyMesh, IMeshTracks> MeshTracks
          => this.meshTracks_;

        public IMeshTracks AddMeshTracks(IReadOnlyMesh mesh)
          => this.meshTracks_[mesh] = new MeshTracksImpl(this);


        public IReadOnlyDictionary<IReadOnlyTexture, ITextureTracks> TextureTracks
          => throw new NotImplementedException();

        public ITextureTracks AddTextureTracks(IReadOnlyTexture texture) {
          throw new NotImplementedException();
        }


        // TODO: Allow setting looping behavior (once, back and forth, etc.)
      }


      public IReadOnlyList<IMorphTarget> MorphTargets { get; }

      public IMorphTarget AddMorphTarget() {
        var morphTarget = new MorphTargetImpl();
        this.morphTargets_.Add(morphTarget);
        return morphTarget;
      }

      private class MorphTargetImpl : IMorphTarget {
        private Dictionary<IReadOnlyVertex, Position> morphs_ = new();

        public MorphTargetImpl() {
          this.Morphs =
              new ReadOnlyDictionary<IReadOnlyVertex, Position>(this.morphs_);
        }

        public string Name { get; set; }
        public IReadOnlyDictionary<IReadOnlyVertex, Position> Morphs { get; }

        public IMorphTarget MoveTo(IReadOnlyVertex vertex, Position position) {
          this.morphs_[vertex] = position;
          return this;
        }
      }
    }

    public class BoneTracksImpl : IBoneTracks {
      public BoneTracksImpl(IAnimation animation, IReadOnlyBone bone) {
        this.Animation = animation;
        this.Bone = bone;
      }

      public override string ToString() => $"BoneTracks[{Bone}]";

      public IAnimation Animation { get; }
      public IReadOnlyBone Bone { get; }

      public IPositionTrack3d? Positions { get; private set; }
      public IRotationTrack3d? Rotations { get; private set; }
      public IScale3dTrack? Scales { get; private set; }

      public ICombinedPositionAxesTrack3d UseCombinedPositionAxesTrack(
          int initialCapacity)
        => (ICombinedPositionAxesTrack3d) (this.Positions =
            new CombinedPositionAxesTrack3dImpl(
                this.Animation,
                initialCapacity));

      public ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(
          int initialCapacity)
        => this.UseSeparatePositionAxesTrack(initialCapacity,
                                             initialCapacity,
                                             initialCapacity);

      public ISeparatePositionAxesTrack3d UseSeparatePositionAxesTrack(
          int initialXCapacity,
          int initialYCapacity,
          int initialZCapacity) {
        Span<int> initialAxisCapacities = stackalloc int[3];
        initialAxisCapacities[0] = initialXCapacity;
        initialAxisCapacities[1] = initialYCapacity;
        initialAxisCapacities[2] = initialZCapacity;

        return (ISeparatePositionAxesTrack3d) (this.Positions =
            new SeparatePositionAxesTrack3dImpl(
                this.Animation,
                this.Bone,
                initialAxisCapacities));
      }


      public IQuaternionRotationTrack3d UseQuaternionRotationTrack(
          int initialCapacity)
        => (IQuaternionRotationTrack3d) (this.Rotations =
            new QuaternionRotationTrack3dImpl(this.Animation, initialCapacity));

      public IQuaternionAxesRotationTrack3d UseQuaternionAxesRotationTrack()
        => (IQuaternionAxesRotationTrack3d) (this.Rotations =
            new QuaternionAxesRotationTrack3dImpl(this.Animation, this.Bone));


      public IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
          int initialCapacity)
        => this.UseEulerRadiansRotationTrack(initialCapacity,
                                             initialCapacity,
                                             initialCapacity);

      public IEulerRadiansRotationTrack3d UseEulerRadiansRotationTrack(
          int initialXCapacity,
          int initialYCapacity,
          int initialZCapacity) {
        Span<int> initialAxisCapacities = stackalloc int[3];
        initialAxisCapacities[0] = initialXCapacity;
        initialAxisCapacities[1] = initialYCapacity;
        initialAxisCapacities[2] = initialZCapacity;

        return (IEulerRadiansRotationTrack3d) (this.Rotations =
            new EulerRadiansRotationTrack3dImpl(
                this.Animation,
                this.Bone,
                initialAxisCapacities));
      }


      public IScale3dTrack UseScaleTrack(
          int initialCapacity)
        => this.UseScaleTrack(initialCapacity,
                              initialCapacity,
                              initialCapacity);

      public IScale3dTrack UseScaleTrack(
          int initialXCapacity,
          int initialYCapacity,
          int initialZCapacity) {
        Span<int> initialAxisCapacities = stackalloc int[3];
        initialAxisCapacities[0] = initialXCapacity;
        initialAxisCapacities[1] = initialYCapacity;
        initialAxisCapacities[2] = initialZCapacity;

        return this.Scales = new ScaleTrackImpl(
            this.Animation,
            this.Bone,
            initialAxisCapacities);
      }


      // TODO: Add pattern for specifying WITH given tracks
    }

    public class MeshTracksImpl : IMeshTracks {
      public IInputOutputTrack<MeshDisplayState,
          StairStepInterpolator<MeshDisplayState>> DisplayStates { get; }


      public MeshTracksImpl(IAnimation animation) {
        this.Animation = animation;
        this.DisplayStates =
            new InputOutputTrackImpl<MeshDisplayState,
                StairStepInterpolator<MeshDisplayState>>(
                animation,
                0,
                new StairStepInterpolator<MeshDisplayState>());
      }

      public IAnimation Animation { get; }
    }
  }
}