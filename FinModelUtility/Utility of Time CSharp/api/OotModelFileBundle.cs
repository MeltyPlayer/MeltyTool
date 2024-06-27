using fin.io;
using fin.model.io;

using UoT.memory;

namespace UoT.api {
  public class OotModelFileBundle(
      IReadOnlyTreeDirectory directory,
      IReadOnlyTreeFile ootRom,
      IZFile zFile) : IModelFileBundle {
    public string GameName => "ocarina_of_time";

    public IReadOnlyTreeFile? MainFile => null;
    public IReadOnlyTreeDirectory Directory { get; } = directory;

    public IReadOnlyTreeFile OotRom { get; } = ootRom;
    public IZFile ZFile { get; } = zFile;

    string IUiFile.HumanReadableName => ZFile.FileName;
    public string TrueFullPath => this.OotRom.FullPath;
  }
}