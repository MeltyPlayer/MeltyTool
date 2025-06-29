using System.Reflection;

using fin.image;
using fin.image.formats;
using fin.io;
using fin.testing;

using SixLabors.ImageSharp.PixelFormats;

namespace fin.picross.solver;

public class PicrossSolverGoldenTests {
  /*[Test]
  [TestCaseSource(nameof(GetGoldenFilesForMarioPicross1_))]
  public void TestPicrossSolverForMariosPicross1(
      IReadOnlySystemFile goldenPicrossFile)
    => this.AssertGolden(goldenPicrossFile);

  [Test]
  [TestCaseSource(nameof(GetGoldenFilesForMarioPicross2_))]
  public void TestPicrossSolverForMariosPicross2(
      IReadOnlySystemFile goldenPicrossFile)
    => this.AssertGolden(goldenPicrossFile);*/

  private static IReadOnlySystemFile[] GetGoldenFilesForMarioPicross1_()
    => GetGoldenFilesForGame_("marios_picross_1");

  private static IReadOnlySystemFile[] GetGoldenFilesForMarioPicross2_()
    => GetGoldenFilesForGame_("marios_picross_2");

  private static IReadOnlySystemFile[] GetGoldenFilesForGame_(string name)
    => GoldenAssert
       .GetRootGoldensDirectory(Assembly.GetExecutingAssembly())
       .AssertGetExistingSubdir(name)
       .GetExistingFiles()
       .ToArray();

  public void AssertGolden(IReadOnlySystemFile goldenPicrossFile) {
    var picrossDefinition = PicrossDefinition.FromImageFile(goldenPicrossFile);

    var allMoveSets
        = new PicrossSolver().Solve(picrossDefinition, out var finalBoardState);

    var isSolved = finalBoardState.GetCompletionState() ==
                   PicrossCompletionState.CORRECT;

    if (!isSolved) {
      using var cellImage = new Rgb24Image(PixelFormat.RGB888,
                                           picrossDefinition.Width,
                                           picrossDefinition.Height);

      {
        using var fastLock = cellImage.Lock();
        var pixels = fastLock.Pixels;

        var width = picrossDefinition.Width;
        for (var y = 0; y < picrossDefinition.Height; y++) {
          for (var x = 0; x < width; x++) {
            var expectedState = picrossDefinition[x, y];
            var actualState = finalBoardState[x, y].Status;

            var r = expectedState ? 255 : 0;
            var g = actualState == PicrossCellStatus.KNOWN_FILLED ? 255 : 0;
            var b = actualState == PicrossCellStatus.KNOWN_EMPTY ? 255 : 0;

            pixels[y * width + x] = new Rgb24((byte) r, (byte) g, (byte) b);
          }
        }
      }

      using var clueImage = new Rgb24Image(PixelFormat.RGB888,
                                           picrossDefinition.Width,
                                           picrossDefinition.Height);

      {
        using var fastLock = clueImage.Lock();
        var pixels = fastLock.Pixels;

        var width = picrossDefinition.Width;
        for (var y = 0; y < picrossDefinition.Height; y++) {
          for (var x = 0; x < width; x++) {
            var expectedState = picrossDefinition[x, y];
            var cell = finalBoardState[x, y];

            var r = expectedState ? 255 : 0;
            var g = cell.ColumnClue != null ? 255 : 0;
            var b = cell.RowClue != null ? 255 : 0;

            pixels[y * width + x] = new Rgb24((byte) r, (byte) g, (byte) b);
          }
        }
      }


      var suffix = $"_{allMoveSets.Count}";

      var cellFile = new FinFile(
          $"C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\Fin\\Fin.Picross Tests\\debug\\{picrossDefinition.Name}_cell{suffix}.png");
      {
        using var fs = cellFile.OpenWrite();
        cellImage.ExportToStream(fs, LocalImageFormat.PNG);
      }
      var clueFile = new FinFile(
          $"C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\Fin\\Fin.Picross Tests\\debug\\{picrossDefinition.Name}_clue{suffix}.png");
      {
        using var fs = clueFile.OpenWrite();
        clueImage.ExportToStream(fs, LocalImageFormat.PNG);
      }
    }

    Assert.That(isSolved, Is.EqualTo(true));
  }
}