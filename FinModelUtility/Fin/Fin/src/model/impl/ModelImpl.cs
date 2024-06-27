using System;
using System.Collections.Generic;
using System.Numerics;

using fin.io;
using fin.io.bundles;

namespace fin.model.impl {
  // TODO: Add logic for optimizing the model.
  public partial class ModelImpl<TVertex>
      : IModel<ISkin<TVertex>> where TVertex : IVertex {
    public ModelImpl(Func<int, Vector3, TVertex> vertexCreator) {
      this.Skin = new SkinImpl(vertexCreator);
      this.AnimationManager = new AnimationManagerImpl(this);
    }

    // TODO: Rewrite this to take in options instead.
    public ModelImpl(int vertexCount,
                     Func<int, Vector3, TVertex> vertexCreator) {
      this.Skin = new SkinImpl(vertexCount, vertexCreator);
      this.AnimationManager = new AnimationManagerImpl(this);
    }

    public required IFileBundle FileBundle { get; init; }
    public required IReadOnlySet<IReadOnlyGenericFile> Files { get; init; }
  }

  public class ModelImpl : ModelImpl<NormalTangentMultiColorMultiUvVertexImpl> {
    public static ModelImpl CreateForViewer()
      => new() {
          FileBundle = null,
          Files = new HashSet<IReadOnlyGenericFile>()
      };

    public static ModelImpl CreateForViewer(int vertexCount)
      => new(vertexCount) {
          FileBundle = null,
          Files = new HashSet<IReadOnlyGenericFile>()
      };

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