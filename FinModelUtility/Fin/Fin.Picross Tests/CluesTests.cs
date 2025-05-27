using fin.data;

namespace fin.picross;

using Assert = NUnit.Framework.Legacy.ClassicAssert;

public class CluesTests {
  [Test]
  public void TestAllEmpty() {
    var picrossDefinition = new PicrossDefinition(5, 10);

    var clues = new PicrossCluesGenerator().GenerateClues(picrossDefinition);

    var columns = clues.Columns;
    Assert.AreEqual(picrossDefinition.Width, columns.Count);
    for (var x = 0; x < picrossDefinition.Width; x++) {
      var column = columns[x];
      Assert.AreEqual(1, column.Count);

      var columnClue = column[0];
      Assert.AreEqual(true, columnClue.IsForColumn);
      Assert.AreEqual(x, columnClue.ColumnOrRowIndex);
      Assert.AreEqual(0, columnClue.CorrectStartIndex);
      Assert.AreEqual(0, columnClue.Length);
    }

    var rows = clues.Rows;
    Assert.AreEqual(picrossDefinition.Height, rows.Count);
    for (var y = 0; y < picrossDefinition.Height; y++) {
      var row = rows[y];
      Assert.AreEqual(1, row.Count);

      var rowClue = row[0];
      Assert.AreEqual(false, rowClue.IsForColumn);
      Assert.AreEqual(y, rowClue.ColumnOrRowIndex);
      Assert.AreEqual(0, rowClue.CorrectStartIndex);
      Assert.AreEqual(0, rowClue.Length);
    }
  }

  [Test]
  public void TestAllFull() {
    var picrossDefinition = new PicrossDefinition(5, 10);
    picrossDefinition.Fill(true);

    var clues = new PicrossCluesGenerator().GenerateClues(picrossDefinition);

    var columns = clues.Columns;
    Assert.AreEqual(picrossDefinition.Width, columns.Count);
    for (var x = 0; x < picrossDefinition.Width; x++) {
      var column = columns[x];
      Assert.AreEqual(1, column.Count);

      var columnClue = column[0];
      Assert.AreEqual(true, columnClue.IsForColumn);
      Assert.AreEqual(x, columnClue.ColumnOrRowIndex);
      Assert.AreEqual(0, columnClue.CorrectStartIndex);
      Assert.AreEqual(picrossDefinition.Height, columnClue.Length);
    }

    var rows = clues.Rows;
    Assert.AreEqual(picrossDefinition.Height, rows.Count);
    for (var y = 0; y < picrossDefinition.Height; y++) {
      var row = rows[y];
      Assert.AreEqual(1, row.Count);

      var rowClue = row[0];
      Assert.AreEqual(false, rowClue.IsForColumn);
      Assert.AreEqual(y, rowClue.ColumnOrRowIndex);
      Assert.AreEqual(0, rowClue.CorrectStartIndex);
      Assert.AreEqual(picrossDefinition.Width, rowClue.Length);
    }
  }

  [Test]
  public void TestCheckerboard() {
    var picrossDefinition = new PicrossDefinition(6, 10);

    for (var y = 0; y < picrossDefinition.Height; ++y) {
      for (var x = 0; x < picrossDefinition.Width; ++x) {
        picrossDefinition[x, y] = (x + y) % 2 == 0;
      }
    }

    var clues = new PicrossCluesGenerator().GenerateClues(picrossDefinition);

    var columns = clues.Columns;
    Assert.AreEqual(picrossDefinition.Width, columns.Count);
    for (var x = 0; x < picrossDefinition.Width; x++) {
      var column = columns[x];
      Assert.AreEqual(picrossDefinition.Height / 2, column.Count);

      for (var i = 0; i < column.Count; ++i) {
        var columnClue = column[i];
        Assert.AreEqual(true, columnClue.IsForColumn);
        Assert.AreEqual(x, columnClue.ColumnOrRowIndex);
        Assert.AreEqual(2 * i + (x % 2 == 0 ? 0 : 1),
                        columnClue.CorrectStartIndex);
        Assert.AreEqual(1, columnClue.Length);
      }
    }

    var rows = clues.Rows;
    Assert.AreEqual(picrossDefinition.Height, rows.Count);
    for (var y = 0; y < picrossDefinition.Height; y++) {
      var row = rows[y];
      Assert.AreEqual(picrossDefinition.Width / 2, row.Count);

      for (var i = 0; i < row.Count; ++i) {
        var columnClue = row[i];
        Assert.AreEqual(false, columnClue.IsForColumn);
        Assert.AreEqual(y, columnClue.ColumnOrRowIndex);
        Assert.AreEqual(2 * i + (y % 2 == 0 ? 0 : 1),
                        columnClue.CorrectStartIndex);
        Assert.AreEqual(1, columnClue.Length);
      }
    }
  }
}