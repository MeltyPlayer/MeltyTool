using fin.audio.io.importers.midi;
using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using vrml.api;


namespace uni.games.vrwdw;

public sealed class VrwdwFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "vrwdw";

  public override FileBundleGathererPlatform Platform
    => FileBundleGathererPlatform.DESKTOP;

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var wrlFile in fileHierarchy.Root.GetFilesWithFileType(".wrl")) {
      organizer.Add(new VrmlModelFileBundle { WrlFile = wrlFile.Impl });
      organizer.Add(new VrmlSceneFileBundle { WrlFile = wrlFile.Impl });
    }

    foreach (var midFile in fileHierarchy.Root.GetFilesWithFileType(".mid")) {
      organizer.Add(
          new MidiAudioFileBundle(midFile.Impl, CommonFiles.WINDOWS_SOUNDFONT_FILE));
    }
  }
}