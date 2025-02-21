using CommandLine;

using uni.games;
using uni.games.animal_crossing;
using uni.games.battalion_wars_1;
using uni.games.battalion_wars_2;
using uni.games.chibi_robo;
using uni.games.glover;
using uni.games.halo_wars;
using uni.games.luigis_mansion;
using uni.games.luigis_mansion_3d;
using uni.games.majoras_mask_3d;
using uni.games.mario_kart_double_dash;
using uni.games.midnight_club_2;
using uni.games.ocarina_of_time_3d;
using uni.games.paper_mario_the_thousand_year_door;
using uni.games.pikmin_1;
using uni.games.pikmin_2;
using uni.games.professor_layton_vs_phoenix_wright;
using uni.games.super_mario_64_ds;
using uni.games.super_mario_sunshine;
using uni.games.super_smash_bros_melee;
using uni.games.wind_waker;

namespace uni.cli;

public interface IMassExporterOptions {
  IMassExporter CreateMassExporter();
}

public interface IMassExporterOptions<out TMassExporter> : IMassExporterOptions
    where TMassExporter : IMassExporter, new() {
  IMassExporter IMassExporterOptions.CreateMassExporter()
    => this.CreateMassExporter();

  new TMassExporter CreateMassExporter() => new();
}

[Verb("animal_crossing",
      HelpText = "Export models en-masse from Animal Crossing.")]
public class AnimalCrossingOptions
    : IMassExporterOptions<AnimalCrossingMassExporter>;

[Verb("battalion_wars_1",
      HelpText = "Export models en-masse from Battalion Wars 1.")]
public class BattalionWars1Options
    : IMassExporterOptions<BattalionWars1MassExporter>;

[Verb("battalion_wars_2",
      HelpText = "Export models en-masse from Battalion Wars 2.")]
public class BattalionWars2Options
    : IMassExporterOptions<BattalionWars2MassExporter>;

[Verb("chibi_robo", HelpText = "Export models en-masse from Chibi-Robo!")]
public class ChibiRoboOptions : IMassExporterOptions<ChibiRoboMassExporter>;

[Verb("glover",
      HelpText = "Export models en-masse from Glover.")]
public class GloverOptions : IMassExporterOptions<GloverMassExporter>;

[Verb("halo_wars",
      HelpText = "Export models en-masse from Halo Wars.")]
public class HaloWarsOptions : IMassExporterOptions<HaloWarsMassExporter>;

[Verb("luigis_mansion",
      HelpText = "Export models en-masse from Luigi's Mansion.")]
public class LuigisMansionOptions
    : IMassExporterOptions<LuigisMansionMassExporter>;

[Verb("luigis_mansion_3d",
      HelpText = "Export models en-masse from Luigi's Mansion 3D.")]
public class LuigisMansion3dOptions
    : IMassExporterOptions<LuigisMansion3dMassExporter>;

[Verb("majoras_mask_3d",
      HelpText = "Export models en-masse from Majora's Mask 3D.")]
public class MajorasMask3dOptions
    : IMassExporterOptions<MajorasMask3dMassExporter>;

[Verb("mario_kart_double_dash",
      HelpText = "Export models en-masse from Mario Kart: Double Dash.")]
public class MarioKartDoubleDashOptions
    : IMassExporterOptions<MarioKartDoubleDashMassExporter>;

[Verb("midnight_club_2",
      HelpText = "Export models en-masse from Midnight Club 2.")]
public class MidnightClub2Options
    : IMassExporterOptions<MidnightClub2MassExporter>;

[Verb("ocarina_of_time_3d",
      HelpText = "Export models en-masse from Ocarina of Time 3D.")]
public class OcarinaOfTime3dOptions
    : IMassExporterOptions<OcarinaOfTime3dMassExporter>;

[Verb("paper_mario_the_thousand_year_door",
      HelpText
          = "Export models en-masse from Paper Mario: The Thousand Year Door.")]
public class PaperMarioTheThousandYearDoorOptions
    : IMassExporterOptions<PaperMarioTheThousandYearDoorMassExporter>;

[Verb("pikmin_1", HelpText = "Export models en-masse from Pikmin 1.")]
public class Pikmin1Options
    : IMassExporterOptions<Pikmin1MassExporter>;

[Verb("pikmin_2", HelpText = "Export models en-masse from Pikmin 2.")]
public class Pikmin2Options : IMassExporterOptions<Pikmin2MassExporter>;

[Verb("professor_layton_vs_phoenix_wright",
      HelpText
          = "Export models en-masse from Professor Layton vs. Phoenix Wright.")]
public class ProfessorLaytonVsPhoenixWrightOptions
    : IMassExporterOptions<ProfessorLaytonVsPhoenixWrightMassExporter>;

[Verb("super_mario_64_ds",
      HelpText = "Export models en-masse from Super Mario 64 DS.")]
public class SuperMario64DsOptions
    : IMassExporterOptions<SuperMario64DsMassExporter>;

[Verb("super_mario_sunshine",
      HelpText = "Export models en-masse from Super Mario Sunshine.")]
public class SuperMarioSunshineOptions
    : IMassExporterOptions<SuperMarioSunshineMassExporter>;

[Verb("super_smash_bros_melee",
      HelpText = "Export models en-masse from Super Smash Bros. Melee.")]
public class SuperSmashBrosMeleeOptions
    : IMassExporterOptions<SuperSmashBrosMeleeMassExporter>;

[Verb("wind_waker",
      HelpText = "Export models en-masse from Wind Waker.")]
public class WindWakerOptions : IMassExporterOptions<WindWakerMassExporter>;