using fin.io;
using fin.model.io;

using UoT.memory;

namespace UoT.api {
  public sealed class OotModelFileBundle(
      IReadOnlyTreeFile ootRom,
      IReadOnlyTreeFile zObjectFile,
      IZFile zFile) : IModelFileBundle<OotModelFileBundle, OotModelImporter> {
    public IReadOnlyTreeFile MainFile => zObjectFile;

    public IReadOnlyTreeFile OotRom => ootRom;
    public IZFile ZFile => zFile;
  }
}