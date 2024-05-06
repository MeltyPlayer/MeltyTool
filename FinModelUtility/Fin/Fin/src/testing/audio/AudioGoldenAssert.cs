using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.audio.io;
using fin.audio.io.exporters.ogg;
using fin.audio.io.importers;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.testing.audio.stubbed;

namespace fin.testing.audio {
  public static class AudioGoldenAssert {
    public static IEnumerable<TAudioBundle> GetGoldenAudioBundles<TAudioBundle>(
        ISystemDirectory rootGoldenDirectory,
        Func<IFileHierarchyDirectory, TAudioBundle>
            gatherAudioBundleFromInputDirectory)
        where TAudioBundle : IAudioFileBundle {
      foreach (var goldenSubdir in
               GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)) {
        var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
        var audioBundle = gatherAudioBundleFromInputDirectory(inputDirectory);

        yield return audioBundle;
      }
    }

    /// <summary>
    ///   Asserts model goldens. Assumes that directories will be stored as the following:
    ///
    ///   - {goldenDirectory}
    ///     - {goldenName1}
    ///       - input
    ///         - {raw golden files}
    ///       - output
    ///         - {exported files}
    ///     - {goldenName2}
    ///       ... 
    /// </summary>
    public static void AssertExportGoldens<TAudioBundle>(
        ISystemDirectory rootGoldenDirectory,
        IAudioImporter<TAudioBundle> audioImporter,
        Func<IFileHierarchyDirectory, TAudioBundle>
            gatherAudioBundleFromInputDirectory)
        where TAudioBundle : IAudioFileBundle {
      foreach (var goldenSubdir in
               GoldenAssert.GetGoldenDirectories(rootGoldenDirectory)) {
        AudioGoldenAssert.AssertGolden(goldenSubdir,
                                       audioImporter,
                                       gatherAudioBundleFromInputDirectory);
      }
    }

    private static string EXTENSION = ".ogg";

    public static void AssertGolden<TAudioBundle>(
        IFileHierarchyDirectory goldenSubdir,
        IAudioImporter<TAudioBundle> audioImporter,
        Func<IFileHierarchyDirectory, TAudioBundle>
            gatherAudioBundleFromInputDirectory)
        where TAudioBundle : IAudioFileBundle {
      using var audioManager = new StubbedAudioManager();

      var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
      var audioBundle = gatherAudioBundleFromInputDirectory(inputDirectory);

      var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");
      var hasGoldenExport =
          outputDirectory.GetFilesWithFileType(EXTENSION).Any();

      GoldenAssert.RunInTestDirectory(
          goldenSubdir,
          tmpDirectory => {
            var targetDirectory =
                hasGoldenExport ? tmpDirectory : outputDirectory.Impl;

            var audioBuffer
                = audioImporter.ImportAudio(audioManager, audioBundle);
            new OggAudioExporter()
                .ExportAudio(
                    audioBuffer,
                    new FinFile(
                        Path.Combine(targetDirectory.FullPath,
                                     $"{audioBundle.MainFile.NameWithoutExtension}{EXTENSION}")));

            if (hasGoldenExport) {
              GoldenAssert.AssertFilesInDirectoriesAreIdentical(
                  tmpDirectory,
                  outputDirectory.Impl);
            }
          });
    }
  }
}