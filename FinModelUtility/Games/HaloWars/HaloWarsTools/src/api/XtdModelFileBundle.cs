using fin.io;
using fin.model.io;

namespace hw.api;

public class XtdModelFileBundle(
    IReadOnlyTreeFile xtdFile,
    IReadOnlyTreeFile xttFile)
    : IHaloWarsFileBundle, IModelFileBundle {
  public string GameName => "halo_wars";
  public IReadOnlyTreeFile MainFile => this.XtdFile;
  public IReadOnlyTreeFile XttFile { get; } = xttFile;
  public IReadOnlyTreeFile XtdFile { get; } = xtdFile;

  public bool UseLowLevelExporter => true;
  public bool ForceGarbageCollection => true;
}