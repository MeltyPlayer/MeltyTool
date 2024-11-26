using fin.io;

using schema.binary;
using schema.util.streams;

namespace uni.games.custom_robo;

public class BinArchiveExtractor {
  public void Extract(IReadOnlyGenericFile binFile,
                      ISystemDirectory outDirectory) {
    using var br =
        new SchemaBinaryReader(binFile.OpenRead(), Endianness.BigEndian);
    if (br.ReadString(4) != "SFD") {
      return;
    }

    this.ExtractEntries_(br, outDirectory, "root");
  }

  private void ExtractEntries_(IBinaryReader br,
                               ISystemDirectory outputDirectory,
                               string name,
                               int depth = 0) {
    var baseOffset = br.Position;
    var weirdNumber = br.ReadInt32();

    // Standard DAT
    if (weirdNumber == br.Length - 1) {
      br.Position = baseOffset;
      this.ExtractFromStartOfDat_(br, outputDirectory, name);
      return;
    }

    if (weirdNumber == 0x40) {
      // TODO: No idea what this header is.
      br.Position = baseOffset + 0x20;
      this.ExtractFromStartOfDat_(br, outputDirectory, name);
      return;
    }

    for (var i = 0; i < weirdNumber; i++) {
      var offset = br.ReadUInt32();
      var length = br.ReadInt32();

      if (depth == 0) {
        br.Position = offset;
        br.PushLocalSpace();
        this.ExtractEntries_(br, outputDirectory, $"{name}_{i}", depth + 1);
        br.PopLocalSpace();
      }
    }

    if (depth > 0) {
      br.Align(0x10);
      this.ExtractFromStartOfDat_(br, outputDirectory, name);
    }
  }

  private void ExtractFromStartOfDat_(IBinaryReader br,
                                      ISystemDirectory outputDirectory,
                                      string name) {
    var fileSize = br.ReadInt32();
    br.Position -= 4;

    try {
      br.Subread(
          fileSize,
          () => {
            var outFile
                = new FinFile(
                    Path.Join(outputDirectory.FullPath, $"{name}.dat"));
            using var fs = outFile.OpenWrite();
            br.CopyTo(fs);
          });
    } catch (Exception e) {
      ;
    }
  }
}