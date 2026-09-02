using fin.archives;
using fin.io;
using fin.io.archive;
using fin.io.bundles;
using fin.util.progress;

using natsume.api;

namespace uni.games.harvest_moon_back_to_nature;

public sealed class HarvestMoonBackToNatureFileBundleGatherer
    : BPs1FileBundleGatherer {
  public override string Name => "harvest_moon_back_to_nature";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
    var root = fileHierarchy.Root;

    var result = new HdtImporter().ExtractIntoAndMaybeCleanUp(
        new HdtFileBundle(
            root.AssertGetExistingFile("a_file.hdt").Impl,
            root.AssertGetExistingFile("a_file.bin").Impl,
            [
                "ObjCg/gf.bin",
                "ObjCg/face.bin",
                "ObjCg/messege0.bin",
                "ObjCg/messege1.bin",
                "ObjCg/messege2.bin",
                "ObjCg/messege3.bin",
                "ObjCg/messege4.bin",
                "data/font/Font_all.tex",
                "Obj/DEBUG.bin",
                "ObjCg/sound.stm",
                "Obj/PdaSamp.bin",
                "Obj/GF_FARM.bin",
                "ObjCg/event_s.bin",
                "ObjCg/evch.bin",
                "ObjCg/Status.Bin",
                "Obj/saveload.bin",
                "ObjCg/Slp.Bin",
                "Obj/GF_MG1.Bin",
                "ObjCg/MG1.Bin",
                "Obj/GF_MG2.Bin",
                "ObjCg/MG2.Bin",
                "Obj/GF_MG3.Bin",
                "ObjCg/MG3.Bin",
                "Obj/GF_MG4.Bin",
                "ObjCg/MG4.Bin",
                "Obj/GF_MG5.Bin",
                "ObjCg/MG5.Bin",
                "Obj/GF_TITLE.Bin",
                "Obj/GF_STAFF.Bin",
                "ObjCg/MesEv.Bin",
                "Obj/GF_swind.bin",
                "Obj/gf_mcard.bin"
            ]),
        root.Impl,
        false);
    if (result == ArchiveExtractionResult.NEWLY_EXTRACTED) {
      fileHierarchy.RefreshRootAndUpdateCache();
    }
  }
}