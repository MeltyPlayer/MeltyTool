using fin.io;
using fin.model;
using fin.model.io;

namespace glo.api;

public class GloModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Glo";

  public string Description
    => "Piko Interactive's model format for Glover's Steam release.";

  public IReadOnlyList<string> KnownPlatforms => new[] { "PC" };
  public IReadOnlyList<string> KnownGames => new[] { "Glover" };
  public IReadOnlyList<string> MainFileExtensions => new[] { ".glo" };
  public IReadOnlyList<string> FileExtensions => this.MainFileExtensions;

  public IModel Import(IEnumerable<IReadOnlySystemFile> files,
                       float frameRate = 30) {
      var gloFile = files.WithFileType(".glo").Single();

      // TODO: Support passing in texture directory
      var textureDirectory = gloFile.AssertGetParent();

      var gloBundle =
          new GloModelFileBundle(gloFile, new[] { textureDirectory });

      return new GloModelImporter().Import(gloBundle);
    }
}