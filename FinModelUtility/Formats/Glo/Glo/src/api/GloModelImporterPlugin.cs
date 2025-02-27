using fin.io;
using fin.model;
using fin.model.io;

namespace glo.api;

public class GloModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Glo";

  public string Description
    => "Piko Interactive's model format for Glover's Steam release.";

  public IReadOnlyList<string> KnownPlatforms => ["PC"];
  public IReadOnlyList<string> KnownGames => ["Glover"];
  public IReadOnlyList<string> MainFileExtensions => [".glo"];
  public IReadOnlyList<string> FileExtensions => this.MainFileExtensions;

  public IModel Import(IEnumerable<IReadOnlyTreeFile> files,
                       float frameRate = 30) {
      var gloFile = files.WithFileType(".glo").Single();

      // TODO: Support passing in texture directory
      var textureDirectory = gloFile.AssertGetParent();

      var gloBundle =
          new GloModelFileBundle(gloFile, [textureDirectory]);

      return new GloModelImporter().Import(gloBundle);
    }
}