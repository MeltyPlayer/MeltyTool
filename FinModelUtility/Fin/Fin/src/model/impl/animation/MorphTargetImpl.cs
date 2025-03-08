using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class AnimationManagerImpl {
    private readonly List<IMorphTarget> morphTargets_ = new();

    public IReadOnlyList<IMorphTarget> MorphTargets => this.morphTargets_;

    public IMorphTarget AddMorphTarget() {
      var morphTarget = new MorphTargetImpl();
      this.morphTargets_.Add(morphTarget);
      return morphTarget;
    }
  }

  private class MorphTargetImpl : IMorphTarget {
    private readonly IndexableDictionary<IReadOnlyVertex, Vector3>
        positionMorphs_ = new();

    private readonly IndexableDictionary<IReadOnlyVertex, Vector3> normalMorphs_
        = new();

    public string Name { get; set; }

    public IReadOnlyIndexableDictionary<IReadOnlyVertex, Vector3> PositionMorphs
      => this.positionMorphs_;

    public IReadOnlyIndexableDictionary<IReadOnlyVertex, Vector3> NormalMorphs
      => this.normalMorphs_;

    public IMorphTarget SetNewLocalPosition(IReadOnlyVertex vertex, Vector3 position) {
      this.positionMorphs_[vertex] = position;
      return this;
    }

    public IMorphTarget SetNewLocalNormal(IReadOnlyVertex vertex, Vector3 normal) {
      this.normalMorphs_[vertex] = normal;
      return this;
    }
  }
}