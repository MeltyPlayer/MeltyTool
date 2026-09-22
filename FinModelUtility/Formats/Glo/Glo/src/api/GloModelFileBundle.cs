using fin.io;
using fin.model.io;

namespace glo.api;

public sealed class GloModelFileBundle(
    IReadOnlyTreeFile gloFile,
    IReadOnlyList<IReadOnlyTreeDirectory> textureDirectories)
    : IModelFileBundle<GloModelFileBundle, GloModelImporter> {
  public IReadOnlyTreeFile MainFile => this.GloFile;

  public IReadOnlyTreeFile GloFile { get; } = gloFile;
  public IReadOnlyList<IReadOnlyTreeDirectory> TextureDirectories { get; } = textureDirectories;
}