using System.Globalization;

using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

using fin.archives;
using fin.io;

namespace hm64.api;

public sealed record Hm64RomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle<Hm64RomFileBundle> {
  public static Hm64RomFileBundle FromFile(IReadOnlyTreeFile file) => new(file);
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/harvestwhisperer/hm64-decomp/blob/58900b4b770b24e6982316c6e88d4d12b8eea84c/tools/modding/map/prep_blender.py
/// </summary>
public sealed partial class Hm64FileTableImporter(
    IReadOnlyTreeFile mapAddressesFile)
    : BSimpleArchiveImporter<Hm64RomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      Hm64RomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    using var mapAddressesCsv = new CsvReader(
        mapAddressesFile.OpenReadAsText(),
        new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = false,
        });
    var mapDatas = mapAddressesCsv
                   .GetRecords<MapData>()
                   .Select(mapData => {
                     var offset
                         = int.Parse(mapData.OffsetHex, NumberStyles.HexNumber);
                     return (offset, mapData.Name);
                   })
                   .ToArray();

    var mapsArchiveDir = builderRoot.AddSubdir("maps");

    for (var i = 0; i < mapDatas.Length; ++i) {
      var (startAddr, mapName) = mapDatas[i];

      // Skip empty maps
      if (mapName.StartsWith("empty-map") || mapName is "end") {
        continue;
      }

      var endAddr = mapDatas[i + 1].offset;

      mapsArchiveDir.AddFile($"{mapName}.bin", startAddr, endAddr - startAddr);
    }
  }

  public sealed class MapData {
    [Index(0)]
    public string OffsetHex { get; set; }

    [Index(1)]
    public string Name { get; set; }
  }
}