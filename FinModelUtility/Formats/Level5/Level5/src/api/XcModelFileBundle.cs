using fin.io;
using fin.model.io;
using fin.util.enumerables;

namespace level5.api;

public class XcModelFileBundle : IModelFileBundle {
  public string? HumanReadableName { get; set; }
  public required string GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.ModelXcFile;
  public IEnumerable<IReadOnlyGenericFile> Files
    => this.ModelXcFile.Yield()
           .ConcatIfNonnull(this.AnimationXcFiles);

  public IReadOnlyTreeFile ModelXcFile { get; set; }
  public IList<IReadOnlyTreeFile>? AnimationXcFiles { get; set; }
}