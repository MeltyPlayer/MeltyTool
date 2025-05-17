using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

using CommunityToolkit.Diagnostics;

using fin.io;
using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace MariosPicross;

public class PuzzleDefinitionReader {
  public IPuzzleDefinition[]? Read(IReadOnlyGenericFile romFile) {
    var romData = romFile.ReadAllBytes();
    var romCrc32 = Crc32.HashToUInt32(romData);
    if (!Constants.OFFSETS_BY_FILE_CRC_32.TryGetValue(
            romCrc32,
            out var allOffsets)) {
      return null;
    }

    using var br = new SchemaBinaryReader(romData, Endianness.BigEndian);

    var allPuzzleDefinitions = new List<IPuzzleDefinition>();
    for (var offsetsI = 0; offsetsI < allOffsets.Length; ++offsetsI) {
      var offsets = allOffsets[offsetsI];

      br.Position = offsets.puzzleOffset;
      var puzzleDefinitions = new IPuzzleDefinition[offsets.puzzleCount];
      for (var i = 0; i < offsets.puzzleCount; ++i) {
        puzzleDefinitions[i] = br.ReadNew<PuzzleDefinition>();
      }

      if (offsets.merge != PuzzleMergeType.UNMERGED) {
        var unmergedPuzzleDefinitions = puzzleDefinitions.AsSpan();
        var puzzleDefinitions30x30 = new IPuzzleDefinition[unmergedPuzzleDefinitions.Length / 4];
        for (var i = 0; i < unmergedPuzzleDefinitions.Length - 3; i += 4) {
          puzzleDefinitions30x30[i / 4]
              = new PuzzleDefinition30x30(unmergedPuzzleDefinitions.Slice(i, 4));
        }

        if (offsets.merge == PuzzleMergeType.MERGE_30) {
          puzzleDefinitions = puzzleDefinitions30x30;
        } else {
          var puzzleDefinitions60x60 = new IPuzzleDefinition[puzzleDefinitions30x30.Length / 4];
          for (var i = 0; i < puzzleDefinitions30x30.Length - 3; i += 4) {
            puzzleDefinitions60x60[i / 4]
                = new PuzzleDefinition60x60(puzzleDefinitions30x30.AsSpan(i, 4));
          }
          puzzleDefinitions = puzzleDefinitions60x60;
        }
      }

      var nameOffset = offsets.nameOffset;
      if (nameOffset != null) {
        br.Position = nameOffset.Value;
        ReadNames_(br, puzzleDefinitions);
        puzzleDefinitions = puzzleDefinitions.Where(p => p.Name != "").ToArray();
      } else {
        for (var i = 0; i < puzzleDefinitions.Length; ++i) {
          puzzleDefinitions[i].Name = $"{offsetsI}_{i}";
        }
      }

      allPuzzleDefinitions.AddRange(puzzleDefinitions);
    }

    return allPuzzleDefinitions.ToArray();
  }

  private static void ReadNames_(
      IBinaryReader br,
      IPuzzleDefinition[] puzzleDefinitions) {
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

    for (var i = puzzleNames.Count; i < puzzleDefinitions.Length; ++i) {
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

    foreach (var (puzzleDefinition, puzzleName) in puzzleDefinitions.Zip(
                 puzzleNames)) {
      puzzleDefinition.Name = puzzleName;
    }
  }
}

/// <summary>
///   Shamelessly stolen from:
///   https://www.zophar.net/fileuploads/3/21546xutra/picrossleveldata.txt
///   https://github.com/sopoforic/cgrr-mariospicross/blob/master/mariospicross.py
/// </summary>
[BinarySchema]
public partial class PuzzleDefinition : IPuzzleDefinition, IBinaryConvertible {
  [Skip]
  public string Name { get; set; }

  [SequenceLengthSource(15)]
  public ushort[] Rows { get; set; }

  public byte Width { get; set; }
  public byte Height { get; set; }

  public bool this[int x, int y] {
    get {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      return this.Rows[y].GetBit(15 - x);
    }
    set {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      this.Rows[y]
          = value ? this.Rows[y].SetBitTo1(x) : this.Rows[y].SetBitTo0(x);
    }
  }
}