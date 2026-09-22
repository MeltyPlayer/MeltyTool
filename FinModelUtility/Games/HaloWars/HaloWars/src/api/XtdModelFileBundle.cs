using fin.io;
using fin.model.io;

namespace hw.api;

public sealed class XtdModelFileBundle(
    IReadOnlyTreeFile xtdFile,
    IReadOnlyTreeFile xttFile)
    : IModelFileBundle<XtdModelFileBundle, XtdModelImporter> {
  public IReadOnlyTreeFile MainFile => this.XtdFile;
  public IReadOnlyTreeFile XttFile { get; } = xttFile;
  public IReadOnlyTreeFile XtdFile { get; } = xtdFile;

  public bool UseLowLevelExporter => true;
  public bool ForceGarbageCollection => true;
}