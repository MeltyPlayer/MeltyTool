using fin.image;
using fin.io;

using gm.schema.dataWin;

namespace gm.api;

public class DataWinExtractor {
  public void Extract(IReadOnlyGenericFile srcFile,
                      ISystemDirectory dstDirectory) {
    if (!dstDirectory.IsEmpty) {
      return;
    }

    dstDirectory.Create();

    var dataWin = srcFile.ReadNew<DataWin>();

    var txtrDirectory = dstDirectory.GetOrCreateSubdir("txtr");
    var spritesheets = dataWin.Txtr.Spritesheets;
    for (var i = 0; i < spritesheets.Length; ++i) {
      var spritesheet = spritesheets[i];
      var spritesheetFile
          = new FinFile(Path.Join(txtrDirectory.FullPath, $"{i}.png"));
      using var spritesheetFileStream = spritesheetFile.OpenWrite();
      spritesheet.Image.ExportToStream(spritesheetFileStream,
                                       LocalImageFormat.PNG);
    }
  }
}