using fin.io;
using fin.model;
using fin.model.io;

namespace marioartist.api;

public class TstltModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Tstlt";

  public string Description => "Mario Artist: Talent Studio model format.";

  public IReadOnlyList<string> KnownPlatforms { get; } =
    ["N64"];

  public IReadOnlyList<string> KnownGames { get; } = [
        "Mario Artist: Talent Studio"
    ];


  public IReadOnlyList<string> MainFileExtensions { get; } = [".tstlt"];
  public IReadOnlyList<string> FileExtensions { get; } = [".tstlt"];

  public IModel Import(IEnumerable<IReadOnlyTreeFile> files,
                       float frameRate = 30) {
      var filesArray = files.ToArray();
      var tstltFiles = filesArray.WithFileType(".tstlt").ToArray();

      var tstltBundle = new TstltModelFileBundle(tstltFiles[0]);

      var tstltImporter = new TstltModelLoader();
      return tstltImporter.Import(tstltBundle);
    }
}