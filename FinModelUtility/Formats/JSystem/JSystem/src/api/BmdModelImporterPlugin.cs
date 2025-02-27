using System.Collections.Generic;
using System.Linq;

using fin.io;
using fin.model;
using fin.model.io;


namespace jsystem.api;

public class BmdModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Bmd";

  public string Description
    => "Nintendo's JStudio model format.";

  public IReadOnlyList<string> KnownPlatforms { get; } =
    ["GameCube", "Wii"];

  public IReadOnlyList<string> KnownGames { get; } = [
      "Mario Kart: Double Dash", "Pikmin 2", "Super Mario Sunshine"
  ];

  public IReadOnlyList<string> MainFileExtensions { get; } = [".bmd"];

  public IReadOnlyList<string> FileExtensions { get; } =
    [".bca", ".bck", ".bmd", ".bti"];

  public IModel Import(
      IEnumerable<IReadOnlySystemFile> files,
      float frameRate = 30) {
    var filesArray = files.ToArray();

    var bcxFiles = filesArray.WithFileTypes(".bca", ".bck").ToArray();
    var bmdFile = filesArray.WithFileType(".bmd").Single();
    var btiFiles = filesArray.WithFileType(".bti").ToArray();

    var bmdBundle = new BmdModelFileBundle {
        BmdFile = bmdFile,
        BcxFiles = bcxFiles,
        BtiFiles = btiFiles,
        FrameRate = frameRate,
    };

    var bmdImporter = new BmdModelImporter();
    return bmdImporter.Import(bmdBundle);
  }
}