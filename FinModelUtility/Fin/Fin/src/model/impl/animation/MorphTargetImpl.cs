using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class AnimationManagerImpl {
    private readonly List<IMorphTarget> morphTargets_ = [];

    public IReadOnlyList<IMorphTarget> MorphTargets => this.morphTargets_;

    public IReadOnlyMorphTarget DefaultMorphTarget { get; }
      = new MorphTargetImpl { Name = "default" };

    public IMorphTarget AddMorphTarget() {
      var morphTarget = new MorphTargetImpl();
      this.morphTargets_.Add(morphTarget);
      return morphTarget;
    }
  }

  private sealed class MorphTargetImpl : IMorphTarget {
    private readonly SparseIndexableDictionary<IReadOnlyVertex, Vector3>
        positionMorphs_;

    private readonly SparseIndexableDictionary<IReadOnlyVertex, Vector3>
        normalMorphs_;

    public MorphTargetImpl() : this([], []) { }

    private MorphTargetImpl(
        SparseIndexableDictionary<IReadOnlyVertex, Vector3> positionMorphs,
        SparseIndexableDictionary<IReadOnlyVertex, Vector3> normalMorphs) {
      this.positionMorphs_ = positionMorphs;
      this.normalMorphs_ = normalMorphs;
    }

    public string Name { get; set; }

    public IReadOnlySparseIndexableDictionary<IReadOnlyVertex, Vector3>
        PositionMorphs
      => this.positionMorphs_;

    public IReadOnlySparseIndexableDictionary<IReadOnlyVertex, Vector3>
        NormalMorphs
      => this.normalMorphs_;

    public IMorphTarget SetNewLocalPosition(IReadOnlyVertex vertex,
                                            Vector3 position) {
      this.positionMorphs_[vertex] = position;
      return this;
    }

    public IMorphTarget SetNewLocalNormal(IReadOnlyVertex vertex,
                                          Vector3 normal) {
      this.normalMorphs_[vertex] = normal;
      return this;
    }

    public IMorphTarget Clone()
      => new MorphTargetImpl(this.positionMorphs_.Clone(),
                             this.normalMorphs_.Clone());
  }
}