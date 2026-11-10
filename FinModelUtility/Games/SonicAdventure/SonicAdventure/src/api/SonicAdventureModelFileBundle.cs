using fin.io;
using fin.model.io;

namespace sonicadventure.api;

public record SonicAdventureModelFileBundle(
    string HumanReadableName,
    IReadOnlyTreeFile ModelFile,
    uint ModelFileKey,
    uint ModelFileOffset,
    IReadOnlyTreeFile TextureFile)
    : IModelFileBundle<SonicAdventureModelFileBundle,
        SonicAdventureModelFileImporter> {
  public IReadOnlyTreeFile MainFile => this.ModelFile;
}