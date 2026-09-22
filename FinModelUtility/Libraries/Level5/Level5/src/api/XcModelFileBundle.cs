using fin.io;
using fin.model.io;
using fin.util.enumerables;


namespace level5.api;

public sealed class XcModelFileBundle : IModelFileBundle<XcModelFileBundle, XcModelImporter> {
  public string? HumanReadableName { get; set; }

  public IReadOnlyTreeFile MainFile
    => this.ModelDirectory.AssertGetParent()
           .AssertGetExistingFile($"{this.ModelDirectory.Name}.xc");

  public IEnumerable<IReadOnlyGenericFile> Files
    => this.MainFile.Yield()
           .Concat(this.ModelDirectory.GetExistingFiles())
           .ConcatIfNonnull(
               this.AnimationDirectories?.SelectMany(
                   d => d.GetExistingFiles()));

  public IReadOnlyTreeDirectory ModelDirectory { get; set; }
  public IList<IReadOnlyTreeDirectory>? AnimationDirectories { get; set; }
}