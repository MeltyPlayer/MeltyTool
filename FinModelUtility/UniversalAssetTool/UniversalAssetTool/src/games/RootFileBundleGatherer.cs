using fin.io.bundles;
using fin.util.progress;

using uni.config;
using uni.games.animal_crossing;
using uni.games.animal_crossing_wild_world;
using uni.games.battalion_wars_1;
using uni.games.battalion_wars_2;
using uni.games.chibi_robo;
using uni.games.dead_space_1;
using uni.games.dead_space_2;
using uni.games.dead_space_3;
using uni.games.doshin_the_giant;
using uni.games.ever_oasis;
using uni.games.glover;
using uni.games.great_ace_attorney;
using uni.games.halo_wars;
using uni.games.luigis_mansion;
using uni.games.luigis_mansion_3d;
using uni.games.majoras_mask_3d;
using uni.games.mario_kart_double_dash;
using uni.games.midnight_club_2;
using uni.games.nintendogs_labrador_and_friends;
using uni.games.ocarina_of_time;
using uni.games.ocarina_of_time_3d;
using uni.games.paper_mario_directors_cut;
using uni.games.paper_mario_the_thousand_year_door;
using uni.games.pikmin_1;
using uni.games.pikmin_2;
using uni.games.professor_layton_vs_phoenix_wright;
using uni.games.soulcalibur_ii;
using uni.games.super_mario_64;
using uni.games.super_mario_64_ds;
using uni.games.super_mario_sunshine;
using uni.games.super_smash_bros_melee;
using uni.games.timesplitters_2;
using uni.games.vrwdw;
using uni.games.wind_waker;

namespace uni.games;

public class RootFileBundleGatherer {
  public IFileBundleDirectory GatherAllFiles(
      IMutablePercentageProgress mutablePercentageProgress) {
    var gatherers = new IAnnotatedFileBundleGatherer[] {
        new AnimalCrossingFileBundleGatherer(),
        new AnimalCrossingWildWorldFileBundleGatherer(),
        new BattalionWars1FileBundleGatherer(),
        new BattalionWars2FileBundleGatherer(),
        new ChibiRoboFileBundleGatherer(),
        new DeadSpace1FileBundleGatherer(),
        new DeadSpace2FileBundleGatherer(),
        new DeadSpace3FileBundleGatherer(),
        new DoshinTheGiantFileBundleGatherer(),
        new EverOasisFileBundleGatherer(),
        new GloverFileBundleGatherer(),
        new GreatAceAttorneyFileBundleGatherer(),
        new HaloWarsFileBundleGatherer(),
        new LuigisMansionFileBundleGatherer(),
        new LuigisMansion3dFileBundleGatherer(),
        new MajorasMask3dFileBundleGatherer(),
        new MarioKartDoubleDashFileBundleGatherer(),
        new MidnightClub2FileBundleGatherer(),
        new NintendogsLabradorAndFriendsFileBundleGatherer(),
        new OcarinaOfTimeFileBundleGatherer(),
        new OcarinaOfTime3dFileBundleGatherer(),
        new PaperMarioDirectorsCutFileBundleGatherer(),
        new PaperMarioTheThousandYearDoorFileBundleGatherer(),
        new Pikmin1FileBundleGatherer(),
        new Pikmin2FileBundleGatherer(),
        new ProfessorLaytonVsPhoenixWrightFileBundleGatherer(),
        new SoulcaliburIIFileBundleGatherer(),
        new SuperMario64DsFileBundleGatherer(),
        new SuperMario64FileBundleGatherer(),
        new SuperMarioSunshineFileBundleGatherer(),
        new SuperSmashBrosMeleeFileBundleGatherer(),
        new Timesplitters2FileBundleGatherer(),
        new VrwdwFileBundleGatherer(),
        new WindWakerFileBundleGatherer(),
    };

    IAnnotatedFileBundleGatherer rootGatherer;
    if (Config.Instance.ExtractorSettings.UseMultithreadingToExtractRoms) {
      var accumulator = new ParallelAnnotatedFileBundleGathererAccumulator();
      foreach (var gatherer in gatherers) {
        accumulator.Add(gatherer);
      }

      rootGatherer = accumulator;
    } else {
      var accumulator = new AnnotatedFileBundleGathererAccumulator();
      foreach (var gatherer in gatherers) {
        accumulator.Add(gatherer);
      }

      rootGatherer = accumulator;
    }

    var organizer = new FileBundleTreeOrganizer();
    rootGatherer.GatherFileBundles(organizer, mutablePercentageProgress);
    return organizer.CleanUpAndGetRoot();
  }
}