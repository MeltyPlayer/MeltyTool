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
      Assert.AreEqual(0, column[0].Length);
    }

    var rows = clues.Rows;
    Assert.AreEqual(picrossDefinition.Height, rows.Count);
    for (var y = 0; y < picrossDefinition.Height; y++) {
      var row = rows[y];
      Assert.AreEqual(1, row.Count);
      Assert.AreEqual(0, row[0].Length);
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
      Assert.AreEqual(picrossDefinition.Height, column[0].Length);
    }

    var rows = clues.Rows;
    Assert.AreEqual(picrossDefinition.Height, rows.Count);
    for (var y = 0; y < picrossDefinition.Height; y++) {
      var row = rows[y];
      Assert.AreEqual(1, row.Count);
      Assert.AreEqual(picrossDefinition.Width, row[0].Length);
    }
  }
}
