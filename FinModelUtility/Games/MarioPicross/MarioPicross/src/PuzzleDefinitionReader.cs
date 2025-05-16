using System.IO.Hashing;
using System.Runtime.InteropServices;
using System.Text;

using CommunityToolkit.Diagnostics;

using fin.io;
using fin.math;

using schema.binary;
using schema.binary.attributes;

namespace MarioPicross;

public class PuzzleDefinitionReader {
  public PuzzleDefinition[]? Read(IReadOnlyGenericFile romFile) {
    var romData = romFile.ReadAllBytes();
    var romCrc32 = Crc32.HashToUInt32(romData);
    if (!Constants.OFFSETS_BY_FILE_CRC_32.TryGetValue(
            romCrc32,
            out var offsets)) {
      return null;
    }

    using var br = new SchemaBinaryReader(romData, Endianness.BigEndian);

    br.Position = offsets.puzzleOffset;
    var puzzleDefinitions = br.ReadNews<PuzzleDefinition>(255);

    br.Position = offsets.nameOffset;
    ReadNames_(br, puzzleDefinitions);

    return puzzleDefinitions.Where(p => p.Name != "").ToArray();
  }

  private static void ReadNames_(
      IBinaryReader br,
      PuzzleDefinition[] puzzleDefinitions) {
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
public partial class PuzzleDefinition : IBinaryConvertible {
  [Skip]
  public string Name { get; set; }

  [SequenceLengthSource(15)]
  public ushort[] Rows { get; set; }

  public byte Width { get; set; }
  public byte Height { get; set; }

  public bool this[byte x, byte y] {
    get {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      return this.Rows[y].GetBit(x);
    }
    set {
      Guard.IsLessThan(x, this.Width);
      Guard.IsLessThan(y, this.Height);
      this.Rows[y]
          = value ? this.Rows[y].SetBitTo1(x) : this.Rows[y].SetBitTo0(x);
    }
  }
}