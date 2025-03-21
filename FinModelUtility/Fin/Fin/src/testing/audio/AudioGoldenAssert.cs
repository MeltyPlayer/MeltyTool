using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using fin.audio.io;
using fin.audio.io.exporters.ogg;
using fin.audio.io.importers;
using fin.io;
using fin.audio.stubbed;

namespace fin.testing.audio;

public static class AudioGoldenAssert {
  private static string EXTENSION = ".ogg";

  public static void AssertGolden<TAudioBundle>(
      IFileHierarchyDirectory goldenSubdir,
      IAudioImporter<TAudioBundle> audioImporter,
      Func<IFileHierarchyDirectory, TAudioBundle>
          gatherAudioBundleFromInputDirectory)
      where TAudioBundle : IAudioFileBundle {
    GoldenAssert.AssertGoldenFiles(
        goldenSubdir,
        (inputDirectory, targetDirectory) => {
          using var audioManager = new StubbedAudioManager();
          var audioBundle = gatherAudioBundleFromInputDirectory(inputDirectory);
          var audioBuffer
              = audioImporter.ImportAudio(audioManager, audioBundle);
          new OggAudioExporter()
              .ExportAudio(
                  audioBuffer[0],
                  new FinFile(
                      Path.Combine(targetDirectory.FullPath,
                                   $"{audioBundle.MainFile.NameWithoutExtension}{EXTENSION}")));
        });
  }
}