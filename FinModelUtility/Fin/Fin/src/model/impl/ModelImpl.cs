using System;

namespace fin.model.impl {
  // TODO: Add logic for optimizing the model.
  public partial class ModelImpl<TVertex>
      : IModel<ISkin<TVertex>> where TVertex : IVertex {
    public ModelImpl(Func<int, Position, TVertex> vertexCreator) {
      this.Skin = new SkinImpl(vertexCreator);
      this.AnimationManager = new AnimationManagerImpl(this);
    }

    // TODO: Rewrite this to take in options instead.
    public ModelImpl(int vertexCount,
                     Func<int, Position, TVertex> vertexCreator) {
      this.Skin = new SkinImpl(vertexCount, vertexCreator);
      this.AnimationManager = new AnimationManagerImpl(this);
    }
  }

  public class ModelImpl : ModelImpl<NormalTangentMultiColorMultiUvVertexImpl> {
    public ModelImpl() : base(
        (index, position)
            => new NormalTangentMultiColorMultiUvVertexImpl(
                index,
                position)) { }

    // TODO: Rewrite this to take in options instead.
    public ModelImpl(int vertexCount) :
        base(vertexCount,
             (index, position)
                 => new NormalTangentMultiColorMultiUvVertexImpl(
                     index,
                     position)) { }
  }
}