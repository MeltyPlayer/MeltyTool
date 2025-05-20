using fin.data;

namespace fin.picross;

public interface IPicrossClues {
  IReadOnlyList<IReadOnlyList<byte>> Columns { get; }
  IReadOnlyList<IReadOnlyList<byte>> Rows { get; }
}

public class PicrossCluesGenerator {
  public IPicrossClues GenerateClues(IPicrossDefinition picrossDefinition) {
    var width = picrossDefinition.Width;
    var height = picrossDefinition.Height;

    var columns = new IReadOnlyList<byte>[width];
    for (var x = 0; x < width; ++x) {
      columns[x] = GetClues_(picrossDefinition.GetColumn(x));
    }

    var rows = new IReadOnlyList<byte>[height];
    for (var y = 0; y < height; ++y) {
      rows[y] = GetClues_(picrossDefinition.GetRow(y));
    }

    return new PicrossClues { Columns = columns, Rows = rows };
  }

  private static byte[] GetClues_(IEnumerable<bool> cells) {
    var length = (byte) 0;
    var lengths = new LinkedList<byte>();
    foreach (var cell in cells) {
      if (cell) {
        ++length;
      } else if (length > 0) {
        lengths.AddLast(length);
        length = 0;
      }
    }

    if (length != 0) {
      lengths.AddLast(length);
    }

    return lengths.ToArray();
  }

  private class PicrossClues : IPicrossClues {
    public required IReadOnlyList<IReadOnlyList<byte>> Columns { get; init; }
    public required IReadOnlyList<IReadOnlyList<byte>> Rows { get; init; }
  }
}