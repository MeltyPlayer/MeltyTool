using fin.data.dictionaries;
using fin.io;

using level5.schema;

using schema.binary;


namespace uni.games.professor_layton_vs_phoenix_wright;

internal class XcArchiveExtractor {
  public void ExtractIntoDirectory(IReadOnlyTreeFile xcFile,
                                   ISystemDirectory dstDirectory) {
    dstDirectory = new FinDirectory(
        Path.Join(dstDirectory.FullPath, xcFile.NameWithoutExtension));
    if (dstDirectory is {Exists: true, IsEmpty: false}) {
      return;
    }

    dstDirectory.Create();

    var xc = xcFile.ReadNew<Xc>(Endianness.LittleEndian);
    foreach (var (extension, files) in xc.FilesByExtension.GetPairs()) {
      foreach (var file in files) {
        var dstFile = new FinFile(Path.Join(dstDirectory.FullPath, file.Name));
        dstFile.WriteAllBytes(file.Data);
      }
    }
  }
}