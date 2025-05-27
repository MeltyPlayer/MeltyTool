using fin.data;
using fin.data.indexable;

namespace fin.picross;

public interface IPicrossClue : IIndexable {
  bool IsForColumn { get; }
  byte ColumnOrRowIndex { get; }
  byte CorrectStartIndex { get; }
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
      columns[x] = GetClues_(ref index,
                             true,
                             (byte) x,
                             picrossDefinition.GetColumn(x));
    }

    var rows = new IReadOnlyList<IPicrossClue>[height];
    for (var y = 0; y < height; ++y) {
      rows[y] = GetClues_(ref index,
                          false,
                          (byte) y,
                          picrossDefinition.GetRow(y));
    }

    return new PicrossClues { Columns = columns, Rows = rows };
  }

  private static IPicrossClue[] GetClues_(ref int index,
                                          bool isForColumn,
                                          byte columnOrRowIndex,
                                          IEnumerable<bool> cells) {
    byte i = 0;
    var length = (byte) 0;
    var clues = new LinkedList<IPicrossClue>();
    foreach (var cell in cells) {
      if (cell) {
        ++length;
      } else if (length > 0) {
        clues.AddLast(
            new PicrossClue(index++,
                            isForColumn,
                            columnOrRowIndex,
                            (byte) (i - length),
                            length));
        length = 0;
      }

      ++i;
    }

    if (clues.Count == 0 || length != 0) {
      clues.AddLast(
          new PicrossClue(index++,
                          isForColumn,
                          columnOrRowIndex,
                          length == 0 ? (byte) 0 : (byte) (i - length),
                          length));
    }

    return clues.ToArray();
  }

  public record PicrossClue(
      int Index,
      bool IsForColumn,
      byte ColumnOrRowIndex,
      byte CorrectStartIndex,
      byte Length)
      : IPicrossClue;

  private class PicrossClues : IPicrossClues {
    public required IReadOnlyList<IReadOnlyList<IPicrossClue>> Columns {
      get;
      init;
    }

    public required IReadOnlyList<IReadOnlyList<IPicrossClue>> Rows {
      get;
      init;
    }
  }
}