using fin.io;
using fin.log;

using schema.binary;
using schema.util.streams;

using visceral.schema.bigfile;

namespace visceral.api;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/gibbed/Gibbed.Visceral/blob/master/projects/Gibbed.Visceral.Setup/Project.cs#L85
/// </summary>
public class BighExtractor {
  private readonly ILogger logger_ = Logging.Create<BighExtractor>();

  public void Extract(IReadOnlyGenericFile bighFile,
                      IReadOnlyGenericFile filelistFile,
                      ISystemDirectory outputDir) {
    this.logger_.LogInformation(
        $"Extracting BIGH {bighFile.DisplayFullPath}...");

    var fileNameByHash = filelistFile
                         .ReadAllLines()
                         .ToDictionary(
                             l => HashFileName_(
                                 l.Replace('/', '\\').ToLowerInvariant()));

    var outputDirFullPath = outputDir.FullPath;

    using var bighBr = bighFile.OpenReadAsBinary(Endianness.BigEndian);
    var bigh = bighBr.ReadNew<Bigh>();
    foreach (var entryInfo in bigh.EntryInfos) {
      // TODO: Still extract these anyway, even though they don't have names?
      if (!fileNameByHash.TryGetValue(entryInfo.NameHash, out var fileName)) {
        continue;
      }

      var fullFileName = Path.Join(outputDirFullPath, fileName);

      var dstFile = new FinFile(fullFileName);
      var dstDir = new FinDirectory(dstFile.GetParentFullPath().ToString());
      dstDir.Create();
      
      using var fs = dstFile.OpenWrite();
      bighBr.SubreadAt(
          entryInfo.Offset,
          (int) entryInfo.Size,
          () => bighBr.CopyTo(fs));
    }
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/gibbed/Gibbed.Visceral/blob/master/projects/Gibbed.Visceral.FileFormats/StringHelpers.cs#L32
  /// </summary>
  private static uint HashFileName_(string fileName)
    => fileName.Aggregate<char, uint>(
        0,
        (current, t) => (current * 65599) + t);
}