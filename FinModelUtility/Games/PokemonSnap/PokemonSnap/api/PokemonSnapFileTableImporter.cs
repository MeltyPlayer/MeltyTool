using fin.archives;
using fin.io;

using schema.binary;

namespace pokemonSnap.api;

using OverlaySpec = (uint rom, uint ram, uint length);

public sealed record PokemonSnapRomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/magcius/noclip.website/blob/main/src/PokemonSnap/tools/extractor.ts
/// </summary>
public sealed class PokemonSnapFileTableImporter
    : BSimpleArchiveImporter<PokemonSnapRomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      PokemonSnapRomFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    var sceneTuples = new SceneTuple[] {
        new(0xe, "common", (0x5959C, 0x800ADBEC, 83 * 0x14)),
        new(0x10,
            "beach",
            (0x13C780, 0x801B0310, 0x26530),
            0x8011B914,
            0x802CBEE4,
            0x80318F00),
        new(0x12,
            "tunnel",
            (0x1D1D90, 0x8018BC50, 0x240E0),
            0x8011E6CC,
            0x802EDFAC,
            0x80326EE0),
        new(0x14,
            "cave",
            (0x27AB80, 0x801AEDF0, 0x1F610),
            0x8012A0E8,
            0x802C6234,
            0x80317610),
        new(0x16,
            "river",
            (0x30AF90, 0x8019AEE0, 0x1BC80),
            0x8012AC90,
            0x802E271C,
            0x80321560),
        new(0x18,
            "volcano",
            (0x3D0560, 0x801A9900, 0x25E70),
            0x800FFFB8,
            0X802E0D44,
            0x8031D4D0),
        new(0x1a,
            "valley",
            (0x47CF30, 0x80186B10, 0x2B230),
            0x80100720,
            0x802D282C,
            0x8031F9C0),
        new(0x1c,
            "rainbow_cloud",
            (0x4EC000, 0x80139C50, 0x04610),
            0x800F5DA0,
            0x8034AB34)
    };

    var particleAddresses = new[] {
        0xAB5860,
        0xAB85E0,
        0xABE7A0,
        0xAC6890,
        0xAC8510,
        0xACF6F0,
        0xAD0E00,
        0xADD310,
        0xADEC60,
    };

    var scenesArchiveDir = builderRoot.AddSubdir("scenes");
    foreach (var sceneTuple in sceneTuples) {
      var offset = 0x57580 + sceneTuple.Id * 0x24;

      romBr.Position = offset;
      var romStart = romBr.ReadUInt32();
      var romEnd = romBr.ReadUInt32();
      var startAddress = romBr.ReadUInt32();

      romBr.Position = offset + 0x24;
      var codeRomStart = romBr.ReadUInt32();
      var codeRomEnd = romBr.ReadUInt32();
      var codeStartAddress = romBr.ReadUInt32();

      var particleIndex = (sceneTuple.Id - 14) >> 1;
      var particleStart = particleAddresses[particleIndex];
      var particleEnd = particleAddresses[particleIndex + 1];

      var sceneArchiveDir = scenesArchiveDir.AddSubdir(sceneTuple.Name);
      sceneArchiveDir.AddFile("data.bin", romStart, romEnd - romStart);
      sceneArchiveDir.AddFile("code.bin",
                              codeRomStart,
                              codeRomEnd - codeRomStart);
      sceneArchiveDir.AddFile("particle.bin",
                              particleStart,
                              particleEnd - particleStart);
      sceneArchiveDir.AddFile("photo.bin",
                              sceneTuple.Photo.rom,
                              sceneTuple.Photo.length);
      sceneArchiveDir.AddJsonFile("metadata.json", sceneTuple);
    }

    var pokemonTuples = new PokemonTuple[] {
        new("magikarp",
            (0x731B0, 0x800F5D90, 0xA200),
            (0x54B5D0, 0x8034E130, 0x20D0),
            (rom: 0x82F8E0, 0x803B1F80, 0x3080)
        ),
        new("pikachu",
            (0x7D3B0, 0x800FFF90, 0x1B0C0),
            (0x54D6A0, 0x803476A0, 0x6A90),
            (0x832960, 0x803AD580, 0x4A00)
        ),
        new("bulbasaur",
            (0x99F70, 0x8011CB50, 0xD570),
            (0x557050, 0x8033F6C0, 0x50C0),
            (0x83A1E0, 0x803A71B0, 0x3550)
        ),
        new("zubat",
            (0x98470, 0x8011B050, 0x1B00),
            (0x554130, 0x80344780, 0x2F20),
            (0x837360, 0x803AA700, 0x2E80)
        )
    };

    var pokemonArchiveDir = builderRoot.AddSubdir("pokemon");
    foreach (var pokemonTuple in pokemonTuples) {
      var pokemonArchivveDir = pokemonArchiveDir.AddSubdir(pokemonTuple.Name);
      pokemonArchivveDir.AddFile("data.bin", pokemonTuple.Data.rom, pokemonTuple.Data.length);
      pokemonArchivveDir.AddFile("code.bin", pokemonTuple.Code.rom, pokemonTuple.Code.length);
      pokemonArchivveDir.AddFile("photo.bin", pokemonTuple.Photo.rom, pokemonTuple.Photo.length);
      pokemonArchivveDir.AddJsonFile("metadata.json", pokemonTuple);
    }
  }

  private record SceneTuple(
      byte Id,
      string Name,
      OverlaySpec Photo,
      uint Header = 0,
      uint ObjectStart = 0,
      uint CollisionStart = 0);

  private record PokemonTuple(
      string Name,
      OverlaySpec Data,
      OverlaySpec Code,
      OverlaySpec Photo);
}