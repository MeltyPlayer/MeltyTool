using fin.io;
using fin.model.io;

namespace glo.api;

public class GloModelFileBundle : IModelFileBundle {
  public GloModelFileBundle(
      IReadOnlyTreeFile gloFile,
      IReadOnlyList<IReadOnlyTreeDirectory> textureDirectories) {
    this.GloFile = gloFile;
    this.TextureDirectories = textureDirectories;
  }

  public string GameName => "glover";
  public IReadOnlyTreeFile MainFile => this.GloFile;

  public IReadOnlyTreeFile GloFile { get; }
  public IReadOnlyList<IReadOnlyTreeDirectory> TextureDirectories { get; }
}