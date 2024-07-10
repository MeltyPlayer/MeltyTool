using System.Collections.Generic;
using System.Numerics;

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
    private readonly Dictionary<IReadOnlyVertex, Vector3> morphs_ = new();

    public string Name { get; set; }
    public IReadOnlyDictionary<IReadOnlyVertex, Vector3> Morphs => this.morphs_;

    public IMorphTarget MoveTo(IReadOnlyVertex vertex, Vector3 position) {
      this.morphs_[vertex] = position;
      return this;
    }
  }
}