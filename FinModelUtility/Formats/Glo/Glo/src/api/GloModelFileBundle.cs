using fin.io;
using fin.model.io;

namespace glo.api;

public class GloModelFileBundle(
    IReadOnlyTreeFile gloFile,
    IReadOnlyList<IReadOnlyTreeDirectory> textureDirectories)
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.GloFile;

  public IReadOnlyTreeFile GloFile { get; } = gloFile;
  public IReadOnlyList<IReadOnlyTreeDirectory> TextureDirectories { get; } = textureDirectories;
}