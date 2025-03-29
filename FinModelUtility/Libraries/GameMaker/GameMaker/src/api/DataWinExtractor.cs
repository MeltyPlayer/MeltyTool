using System.Drawing;

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

    var spriteSheets = dataWin.Txtr.Elements;
    var sprites = dataWin.Sprt.Elements;

    var txtrDirectory = dstDirectory.GetOrCreateSubdir("txtr");
    for (var i = 0; i < spriteSheets.Length; ++i) {
      var spriteSheet = spriteSheets[i];
      var spriteSheetFile
          = new FinFile(Path.Join(txtrDirectory.FullPath, $"{i}.png"));
      using var spriteSheetFileStream = spriteSheetFile.OpenWrite();
      spriteSheet.Image.ExportToStream(spriteSheetFileStream,
                                       LocalImageFormat.PNG);
    }

    var sprtDirectory = dstDirectory.GetOrCreateSubdir("sprt");
    foreach (var sprite in sprites) {
      var frames = sprite.Frames;
      for (var i = 0; i < frames.Length; ++i) {
        var frame = frames[i];
        var spriteSheet = spriteSheets[frame.SheetId];
        using var frameImage = spriteSheet.Image.SubImage(
            new Rectangle(frame.X,
                          frame.Y,
                          frame.Width,
                          frame.Height));

        var frameName = frames.Length == 1
            ? sprite.Name
            : $"{sprite.Name}_{i}";

        var frameFile = new FinFile(Path.Join(sprtDirectory.FullPath,
                                              $"{frameName}.png"));
        using var frameFileStream = frameFile.OpenWrite();
        frameImage.ExportToStream(frameFileStream, LocalImageFormat.PNG);
      }
    }
  }
}