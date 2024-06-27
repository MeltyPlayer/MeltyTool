using fin.io;
using fin.log;
using fin.util.asserts;

using uni.games;
using uni.util.cmd;

namespace uni.platforms.wii.tools;

public class Wit {
  public bool Run(ISystemFile romFile, out IFileHierarchy hierarchy) {
      Asserts.Equal(
          ".iso",
          romFile
              .FileType,
          $"Cannot dump ROM because it is not an ISO: {romFile}");
      Asserts.True(
          romFile.Exists,
          $"Cannot dump ROM because it does not exist: {romFile}");

      var didChange = false;
      if (ExtractorUtil.HasNotBeenExtractedYet(romFile,
                                               out var finalDirectory)) {
        didChange = true;
        this.DumpRom_(romFile, finalDirectory);
        Asserts.True(finalDirectory.Exists,
                     $"Directory was not created: {finalDirectory}");
      }

      hierarchy = FileHierarchy.From(romFile.NameWithoutExtension, finalDirectory);
      return didChange;
    }

  private void DumpRom_(ISystemFile romFile, ISystemDirectory outDirectory) {
      var logger = Logging.Create<Wit>();
      logger.LogInformation($"Dumping ROM {romFile}...");

      outDirectory.Delete();
      Files.RunInDirectory(
          romFile.AssertGetParent()!,
          () => {
            ProcessUtil.ExecuteBlocking(
                WiiToolsConstants.WIT_EXE,
                $"extract \"{romFile.FullPath}\" \"{outDirectory.FullPath}\"");
          });
    }
}