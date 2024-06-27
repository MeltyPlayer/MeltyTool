using fin.io;
using fin.model.io;

namespace hw.api {
  public class XtdModelFileBundle : IHaloWarsFileBundle, IModelFileBundle {
    public XtdModelFileBundle(IReadOnlyTreeFile xtdFile,
                              IReadOnlyTreeFile xttFile) {
      this.XtdFile = xtdFile;
      this.XttFile = xttFile;
    }

    public string GameName => "halo_wars";
    public IReadOnlyTreeFile MainFile => this.XtdFile;
    public IReadOnlyTreeFile XttFile { get; }
    public IReadOnlyTreeFile XtdFile { get; }

    public bool UseLowLevelExporter => true;
    public bool ForceGarbageCollection => true;
  }
}