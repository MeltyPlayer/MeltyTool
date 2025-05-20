using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

using fin.io;
using fin.picross;

using schema.binary;

namespace MariosPicross;

public class GameBoyPicrossDefinitionReader {
  public IPicrossDefinition[]? Read(IReadOnlyGenericFile romFile) {
    var romData = romFile.ReadAllBytes();
    var romCrc32 = Crc32.HashToUInt32(romData);
    if (!Constants.CONSTANTS_BY_FILE_CRC_32.TryGetValue(
            romCrc32,
            out var allOffsets)) {
      return null;
    }

    using var br = new SchemaBinaryReader(romData, Endianness.BigEndian);

    var allPicrossDefinitions = new List<IPicrossDefinition>();
    for (var offsetsI = 0; offsetsI < allOffsets.Length; ++offsetsI) {
      var offsets = allOffsets[offsetsI];

      br.Position = offsets.puzzleOffset;
      var picrossDefinitions = new IPicrossDefinition[offsets.puzzleCount];
      for (var i = 0; i < offsets.puzzleCount; ++i) {
        picrossDefinitions[i] = br.ReadNew<GameBoyPicrossDefinition>();
      }

      if (offsets.merge != PuzzleMergeType.UNMERGED) {
        var unmergedPicrossDefinitions = picrossDefinitions.AsSpan();
        var picrossDefinitions30x30
            = new IPicrossDefinition[unmergedPicrossDefinitions.Length / 4];
        for (var i = 0; i < unmergedPicrossDefinitions.Length - 3; i += 4) {
          picrossDefinitions30x30[i / 4]
              = new PicrossDefinition30X30(
                  unmergedPicrossDefinitions.Slice(i, 4));
        }

        if (offsets.merge == PuzzleMergeType.MERGE_30) {
          picrossDefinitions = picrossDefinitions30x30;
        } else {
          var picrossDefinitions60x60
              = new IPicrossDefinition[picrossDefinitions30x30.Length / 4];
          for (var i = 0; i < picrossDefinitions30x30.Length - 3; i += 4) {
            picrossDefinitions60x60[i / 4]
                = new PicrossDefinition60X60(
                    picrossDefinitions30x30.AsSpan(i, 4));
          }

          picrossDefinitions = picrossDefinitions60x60;
        }
      }

      var nameOffset = offsets.nameOffset;
      if (nameOffset != null) {
        br.Position = nameOffset.Value;
        ReadNames_(br, picrossDefinitions);
        picrossDefinitions
            = picrossDefinitions.Where(p => p.Name != "").ToArray();
      } else {
        for (var i = 0; i < picrossDefinitions.Length; ++i) {
          picrossDefinitions[i].Name = $"{offsetsI}_{i}";
        }
      }

      allPicrossDefinitions.AddRange(picrossDefinitions);
    }

    return allPicrossDefinitions.ToArray();
  }

  private static void ReadNames_(
      IBinaryReader br,
      IPicrossDefinition[] picrossDefinitions) {
    var textBytes = br.ReadBytes(br.Length - br.Position).AsSpan();
    var textShorts = MemoryMarshal.Cast<byte, ushort>(textBytes);

    Func<ReadOnlySpan<ushort>, ushort, string> getStrAt = (values, diff) => {
      var weirdStuff = false;
      var sb = new StringBuilder();
      foreach (var v in values) {
        var c = (char) (v - diff);

        if (c is >= 'G' and <= (char) ('Z' + ('G' - 'A'))) {
          c = (char) (c - 'G' + 'a');
        }

        if (c is >= 'a' and <= 'z') {
          if (weirdStuff && sb.Length > 0) {
            sb.Append(' ');
          }

          if (c is >= 'G' and <= (char) ('Z' + ('G' - 'A'))) {
            sb.Append((char) (c - 'G' + 'a'));
          } else {
            sb.Append(c);
          }

          weirdStuff = false;
        } else {
          weirdStuff = true;
        }
      }

      return sb.ToString();
    };

    List<string> puzzleNames = [
        "n", "l", "e", "t", "s", "w", "o", "r", "k"
    ];

    for (var i = puzzleNames.Count; i < picrossDefinitions.Length; ++i) {
      var isNewWord = false;
      var currentWord = "";
      while (!isNewWord) {
        var endIndex = textShorts.IndexOf((ushort) 65535);
        var values = textShorts.Slice(0, endIndex);

        if (currentWord.Length > 0) {
          currentWord += " ";
        }

        currentWord += getStrAt(values, 100);

        var bytesEndIndex = 2 * (endIndex + 1);
        while (textBytes[bytesEndIndex] == 0) {
          isNewWord = true;
          bytesEndIndex++;
        }

        textBytes = textBytes.Slice(bytesEndIndex);
        textShorts = MemoryMarshal.Cast<byte, ushort>(textBytes);
      }

      puzzleNames.Add(currentWord);
    }

    foreach (var (puzzleDefinition, puzzleName) in picrossDefinitions.Zip(
                 puzzleNames)) {
      puzzleDefinition.Name = puzzleName;
    }
  }
}