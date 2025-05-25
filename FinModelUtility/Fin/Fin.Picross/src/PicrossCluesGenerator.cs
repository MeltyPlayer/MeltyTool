using fin.data;
using fin.data.indexable;

namespace fin.picross;

public interface IPicrossClue : IIndexable {
  byte Length { get; }
}

public interface IPicrossClues {
  IReadOnlyList<IReadOnlyList<IPicrossClue>> Columns { get; }
  IReadOnlyList<IReadOnlyList<IPicrossClue>> Rows { get; }
}

public class PicrossCluesGenerator {
  public IPicrossClues GenerateClues(
      IReadOnlyPicrossDefinition picrossDefinition) {
    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var index = 0;

    var columns = new IReadOnlyList<IPicrossClue>[width];
    for (var x = 0; x < width; ++x) {
      columns[x] = GetClues_(ref index, picrossDefinition.GetColumn(x));
    }

    var rows = new IReadOnlyList<IPicrossClue>[height];
    for (var y = 0; y < height; ++y) {
      rows[y] = GetClues_(ref index, picrossDefinition.GetRow(y));
    }

    return new PicrossClues { Columns = columns, Rows = rows };
  }

  private static IPicrossClue[] GetClues_(ref int index, IEnumerable<bool> cells) {
    var length = (byte) 0;
    var clues = new LinkedList<IPicrossClue>();
    foreach (var cell in cells) {
      if (cell) {
        ++length;
      } else if (length > 0) {
        clues.AddLast(new PicrossClue(index++, length));
        length = 0;
      }
    }

    if (clues.Count == 0 || length != 0) {
      clues.AddLast(new PicrossClue(index++, length));
    }

    return clues.ToArray();
  }

  public record PicrossClue(int Index, byte Length) : IPicrossClue;

  private class PicrossClues : IPicrossClues {
    public required IReadOnlyList<IReadOnlyList<IPicrossClue>> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyList<IPicrossClue>> Rows { get; init; }
  }
}