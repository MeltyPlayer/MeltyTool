using fin.archives;
using fin.io;

using LinkDotNet.StringBuilder;

using mp64.schema.mainfs;

using schema.binary;

namespace mp64.api;

public sealed record Mp64RomFileBundle(IReadOnlyTreeFile MainFile)
    : ISimpleArchiveFileBundle;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/PartyPlanner64/PartyPlanner64/blob/980d043217823d0e5901ee93d3a50e36f728db10/packages/lib/fs/mainfs.ts
/// </summary>
public sealed class Mp64MainFsImporter
    : BSimpleArchiveImporter<Mp64RomFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      Mp64RomFileBundle bundle,
      ISet<IReadOnlyStandaloneFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.MainFile.OpenRead();

    var romBr = new SchemaBinaryReader(readStream, Endianness.BigEndian);

    romBr.Position = 0x3b;
    var romCode = romBr.ReadString(4);
    var gameVersion = GameVersion.FromRomCode(romCode);
    var game = gameVersion.GetGame();
    
    var mainFsOffsets = gameVersion.GetMainFsOffsets();
    var fsOffset = GetRomOffset_(romBr, mainFsOffsets);

    readStream.Position = fsOffset;

    var mainFs = romBr.ReadNew<MainFs>();

    var dirPtrs = mainFs.Directories;
    for (var d = 0; d < dirPtrs.Length; ++d) {
      var dir = dirPtrs[d].Value;

      var dirName = GetDirName_(game, d) ?? $"{d}";
      var builderDir = builderRoot.AddSubdir(dirName);

      var filePtrs = dir.Files;
      for (var f = 0; f < filePtrs.Length; ++f) {
        var file = filePtrs[f].Value;

        var data = file.DecompressedBytes;

        using var nameSb = new ValueStringBuilder();
        nameSb.Append(GetFileName_(game, d, f) ?? $"{f}");
        if (MagicTextUtil.Verify(data, "FORM")) {
          nameSb.Append(".form");
        } else if (MagicTextUtil.Verify(data, "MTNX")) {
          nameSb.Append(".mtnx");
        } else {
          nameSb.Append(".bin");
        }

        builderDir.AddFile(nameSb.ToString(), data);
      }
    }
  }

  private static uint GetRomOffset_(
      IBinaryReader br,
      (uint, uint)[] mainFsOffsets) {
    var (upper, lower) = mainFsOffsets[0];

    upper = br.SubreadUInt16At(upper);
    lower = br.SubreadUInt16At(lower);

    var offset = (upper << 16) | lower;

    if ((lower & 0x8000) != 0) {
      // Account for signed addition workaround.
      offset -= 0x00010000;
    }

    return offset;
  }

  private static string? GetDirName_(Game game, int dirI)
    => game switch {
        Game.MARIO_PARTY_2 => dirI switch {
            2  => "mario",
            3  => "luigi",
            4  => "yoshi",
            5  => "wario",
            6  => "donkey_kong",
            7  => "peach",
            9  => "menu",
            10 => "board",
            11 => "mini_game_info",
            12 => "battle_results",
            14 => "intro",
            _  => null,
        },
        _ => null,
    };

  private static string? GetFileName_(Game game, int dirI, int fileI)
    => game switch {
        Game.MARIO_PARTY_2 => dirI switch {
            >= 2 and <= 7 => GetCharacterFileName_(fileI),
            _             => null,
        },
        _ => null,
    };

  private static string? GetCharacterFileName_(int fileI)
    => fileI switch {
        0  => "stand",
        1  => "walk",
        2  => "run",
        3  => "punch",
        4  => "jump",
        5  => "kick",
        6  => "hip_drop",
        7  => "wake_up",
        8  => "stand_dizzy",
        9  => "swim",
        10 => "get_up_pain",
        11 => "swim_drown",
        12 => "swim_dead",
        13 => "swim_knocked_back",
        14 => "swim_tread",
        15 => "walk_slow",
        16 => "get_up_from_hip_drop",
        17 => "get_up_from_fall",
        18 => "fall",
        19 => "stand_carry",
        20 => "pant",
        21 => "put_down",
        22 => "sit_dizzy",
        23 => "hit_block",
        // TODO: Good lord there's like a billion more

        // Models
        // TODO: Is this right?
        209 => "default_baked",
        210 => "default",
        211 => "default_low_poly",
        212 => "western_low_poly",
        213 => "western",
        214 => "pirate_low_poly",
        215 => "pirate",
        216 => "horror_low_poly",
        217 => "horror",
        218 => "space_low_poly",
        219 => "space",
        220 => "adventure_low_poly",
        221 => "adventure",
        _   => null,
    };
}