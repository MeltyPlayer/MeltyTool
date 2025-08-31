using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.data.indexable;
using fin.util.optional;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  private partial class ModelAnimationImpl {
    private readonly IndexableDictionary<IReadOnlyMesh, IMeshTracks>
        meshTracks_ = new();

    public bool HasAnyMeshTracks { get; private set; }

    public IReadOnlyIndexableDictionary<IReadOnlyMesh, IMeshTracks> MeshTracks
      => this.meshTracks_;

    public IMeshTracks AddMeshTracks(IReadOnlyMesh mesh) {
      this.HasAnyMeshTracks = true;
      return this.meshTracks_[mesh]
          = new MeshTracksImpl(mesh, this.sharedInterpolationConfig_);
    }
  }

  private class MeshTracksImpl(
      IReadOnlyMesh mesh,
      ISharedInterpolationConfig sharedConfig)
      : IMeshTracks {
    public IReadOnlyMesh Mesh => mesh;

    public IStairStepKeyframes<MeshDisplayState> DisplayStates { get; }
      = new StairStepKeyframes<MeshDisplayState>(
          sharedConfig,
          new IndividualInterpolationConfig<MeshDisplayState>
              { DefaultValue = Optional.Of(() => mesh.DefaultDisplayState) });
  }
}