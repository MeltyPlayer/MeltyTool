using System.Collections.Generic;
using System.Linq;

using fin.io;
using fin.model;
using fin.model.io;

namespace grezzo.api;

public class CmbModelImporterPlugin : IModelImporterPlugin {
  public string DisplayName => "Cmb";

  public string Description => "Grezzo's model format.";

  public IReadOnlyList<string> KnownPlatforms { get; } =
    ["3DS"];

  public IReadOnlyList<string> KnownGames { get; } = [
        "Ever Oasis",
        "Luigi's Mansion 3D",
        "Majora's Mask 3D",
        "Ocarina of Time 3D"
    ];


  public IReadOnlyList<string> MainFileExtensions { get; } = [".cmb"];

  public IReadOnlyList<string> FileExtensions { get; } =
    [".cmb", ".csab", ".ctxb", ".shpa"];

  public IModel Import(IEnumerable<IReadOnlySystemFile> files,
                       float frameRate = 30) {
      var filesArray = files.ToArray();
      var csabFiles = filesArray.WithFileType(".csab").ToArray();
      var cmbFile = filesArray.WithFileType(".cmb").Single();
      var ctxbFiles = filesArray.WithFileType(".ctxb").ToArray();
      var shpaFiles = filesArray.WithFileType(".shpa").ToArray();

      var cmbBundle = new CmbModelFileBundle(
          cmbFile,
          csabFiles,
          ctxbFiles,
          shpaFiles);

      var cmbImporter = new CmbModelImporter();
      return cmbImporter.Import(cmbBundle);
    }
}