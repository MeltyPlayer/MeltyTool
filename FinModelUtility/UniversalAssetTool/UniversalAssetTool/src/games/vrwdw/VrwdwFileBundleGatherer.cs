using fin.audio.io.importers.midi;
using fin.common;
using fin.io;
using fin.io.bundles;
using fin.util.progress;

using vrml.api;


namespace uni.games.vrwdw;

public class VrwdwFileBundleGatherer : BPrereqsFileBundleGatherer {
  public override string Name => "vrwdw";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    foreach (var wrlFile in fileHierarchy.Root.GetFilesWithFileType(".wrl")) {
      organizer.Add(new VrmlModelFileBundle {
          WrlFile = wrlFile,
      }.Annotate(wrlFile));
      organizer.Add(new VrmlSceneFileBundle {
          WrlFile = wrlFile,
      }.Annotate(wrlFile));
    }

    foreach (var midFile in fileHierarchy.Root.GetFilesWithFileType(".mid")) {
      organizer.Add(
          new MidiAudioFileBundle(midFile, CommonFiles.WINDOWS_SOUNDFONT_FILE)
              .Annotate(midFile));
    }
  }
}