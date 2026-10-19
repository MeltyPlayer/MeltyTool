namespace hpcos.api;

/// <summary>
///   Harry Potter and the Chamber of Secrets stores the vast majority of its
///   files in a single directory. This sorts GEOs into a much more easily
///   navigable hierarchy of virtual directories (see
///   IFileBundle.VirtualDirectoryPath), without moving any files.
/// </summary>
public static class GeoModelFileOrganizer {
  private const string UNCATEGORIZED = "uncategorized";

  private const string CHARACTERS = "characters";
  private const string CREATURES = CHARACTERS + "/creatures";
  private const string HARRY = CHARACTERS + "/harry";
  private const string NPCS = CHARACTERS + "/npcs";

  private const string FX = "fx";
  private const string SPELLS = FX + "/spells";
  private const string PROPS = "props";
  private const string DOORS = PROPS + "/doors";

  private const string PLANTS = PROPS + "/plants";
  private const string VEHICLES = PROPS + "/vehicles";

  private const string LOCATIONS = "locations";
  private const string BURROW = LOCATIONS + "/burrow";
  private const string DIAGON_ALLEY = LOCATIONS + "/diagon_alley";
  private const string HOGWARTS = LOCATIONS + "/hogwarts";
  private const string HOGWARTS_DUNGEONS = HOGWARTS + "/dungeons";
  private const string HOGWARTS_GROUNDS = HOGWARTS + "/grounds";

  /// <summary>
  ///   Indexed by floor number, where 0 is the ground floor.
  /// </summary>
  private static readonly string[] HOGWARTS_FLOORS = [
      HOGWARTS + "/0th_floor",
      HOGWARTS + "/1st_floor",
      HOGWARTS + "/2nd_floor",
      HOGWARTS + "/3rd_floor",
      HOGWARTS + "/4th_floor",
      HOGWARTS + "/5th_floor",
      HOGWARTS + "/6th_floor",
      HOGWARTS + "/7th_floor",
  ];

  /// <summary>
  ///   Meshes that haven't been sorted into a category yet.
  /// </summary>
  private const string PENDING = "pending";

  private const string UI = "ui";
  private const string UI_INVENTORY = UI + "/inventory";
  private const string UI_LOADING_SCREENS = UI + "/loading_screens";
  private const string UI_MAPS = UI + "/maps";
  private const string UI_WIZARD_CARDS = UI + "/wizard_cards";

  /// <summary>
  ///   Determines which folder of the file tree a GEO is shown in, by its name.
  /// </summary>
  public static string GetOrganizedGeoPath(string name) => name switch {
    "Aragog_Spider"
        or "AviforsBird"
        or "Basilisk"
        or "Billywig"
        or "Bookcase"
        or "Bowtruckle"
        or "CagedImp"
        or "ChocFrog"
        or "Fawkes"
        or "FireCrab"
        or "Gargoyle"
        or "GloryHand"
        or "GloryHole"
        or "Gnome"
        or "Gytrash"
        or "Hedwig"
        or "Imp"
        or "MediumSpider"
        or "MMPets"
        or "MorphGhosts"
        or "ScreamingBook"
        or "Small_Spider"
        or "Spider"
        or "SqCaged"
        or "Trevor"
        or "washer"
        or "WhompingImps"
        or "Yorik" => CREATURES,

    "harry"
        or "HarryGYL"
        or "HaryHog"
        or "HaryQuid" => HARRY,

    "BloodyBaron"
        or "Borgin"
        or "BurChars"
        or "COS_Ginny"
        or "Crabbe"
        or "DAWizard"
        or "Dedalus"
        or "DiagGeorge"
        or "DiagGinny"
        or "DiagMolly"
        or "DiagRon"
        or "Doris"
        or "DuelMALF"
        or "Dumble"
        or "Dumble190"
        or "EthnicMalePupil"
        or "EthnicMalePupilLP"
        or "FblotWiz"
        or "FemaleGhost"
        or "FemalePrefectSkin"
        or "FemalePupilSkin"
        or "FemalePupilSkinLP"
        or "Flitwick"
        or "Friar"
        or "George"
        or "Ginny"
        or "GJWizard"
        or "Goblin"
        or "Goyle"
        or "hagrid"
        or "Hermione"
        or "Hooch"
        or "JacknJill"
        or "LCWitch"
        or "LCWizard"
        or "LeakyHagrid"
        or "Lockhart"
        or "Lucius"
        or "MalePrefectA1"
        or "MalePrefectA2"
        or "MalePrefectA3"
        or "MalePrefectSkin"
        or "Malfoy"
        or "McGonagall"
        or "MenagWiz"
        or "MoaningMyrtle"
        or "Molly"
        or "MrMulpep"
        or "Neville"
        or "Nick"
        or "Percy"
        or "quidmalfoy"
        or "QuidPlayers"
        or "QuidPlayersF"
        or "racersF"
        or "racersM"
        or "Ron"
        or "SeatedFemalePupil"
        or "SeatedMalePupil"
        or "Snape"
        or "Sprout"
        or "Tom"
        or "TomRiddle"
        or "Turner"
        or "Wood" => NPCS,

    "GhostBox"
        or "rings" => FX,
    "DuelDude"
        or _ when name.StartsWith("Harry", StringComparison.Ordinal) => SPELLS, // Yes, these are spell FX even though they start with Harry

    "Burrow" => BURROW,

    "B_Burkes"
        or "DiagonAlly"
        or "F_Blotts"
        or "G_Japes"
        or "Gringott"
        or "LeakyC"
        or "MagMenag"
        or "Mulpepp" => DIAGON_ALLEY,

    "StealthDun" => HOGWARTS_DUNGEONS,

    "AraArena"
        or "AraForest"
        or "AraLair"
        or "Herbology"
        or "Herbology_Night"
        or "Hogwarts"
        or "Hogwarts_Night"
        or "Quiddtch"
        or "WGytrash"
        or "Whomping"
        or "WImpArea"
        or "WWiggenweld" => HOGWARTS_GROUNDS,

    "EntHall"
        or "EntHall_Night"
        or "GrtHall"
        or "StaircasGeom"
        or "StaircasGeom_Night" => HOGWARTS_FLOORS[0],
    "Avivors" => HOGWARTS_FLOORS[1],
    "ChambOfS"
        or "ChambOfS_Night"
        or "Incendio"
        or "Library"
        or "LibrPotn"
        or "LibrPotn_Night"
        or "StealthLib"
        or "StealthLib_Night" => HOGWARTS_FLOORS[2],
    "Dada"
        or "DDoffice"
        or "DDoffice_Night"
        or "Expelmus" => HOGWARTS_FLOORS[3],
    "GriffRoom"
        or "GriffRoom_Night" => HOGWARTS_FLOORS[7],
    _ when TryToGetLandingFloor_(name, out int floor)
        => HOGWARTS_FLOORS[floor],

      "book"
        or "Cages"
        or "Cask"
        or "COS_Items"
        or "crate"
        or "DADAbits"
        or "Desk"
        or "DiffindoRope"
        or "Dungbomb"
        or "FordAng"
        or "Gates"
        or "GlassJar"
        or "gntargets"
        or "Health"
        or "Herbprops"
        or "ImpCage"
        or "Nimbus"
        or "NormalBook"
        or "Skurge"
        or "snitch"
        or "Staircas"
        or "Staircas_Night"
        or "Tapestry"
        or "Trunk"
        or "Cupboard" => PROPS,

    "anglia" => VEHICLES,

    "Puffapod"
    or "Tentacular" => PLANTS,

    "doorskins"
        or "doubledoorskins" => DOORS,

    "inv_records" => UNCATEGORIZED,
    _ when name.StartsWith("inv", StringComparison.Ordinal) => UI_INVENTORY,
    _ when GeoFiles.IsLoadingScreen(name) => UI_LOADING_SCREENS,
    _ when name.StartsWith("Map_", StringComparison.Ordinal) => UI_MAPS,
    _ when name.StartsWith("Wizardcard", StringComparison.Ordinal)
        => UI_WIZARD_CARDS,

    _ => UNCATEGORIZED
  };

  /// <summary>
  ///   Determines which folder of the file tree a mesh that GEO layouts don't
  ///   place (doors, pickups, effects, etc.) is shown in, by its base name.
  /// </summary>
  public static string GetOrganizedMeshPath(string meshBaseName) => meshBaseName switch {
    // Plants
    "_PINGABLE_MUSHROOM01"
      or "BIRCH_TREE"
      or "CEDAR_TREE"
      or "RODEDENDRON"
      or "SMALL_TREE"
      or "EYE"
      or "TREES"
      or "KNOT"
      or "LARGEBRANCH"
      or "WBL_LARGEBRANCH"
      or "WRL_LARGEBRANCH"
      or "WRS_ROOT"
      or "WWO_CLAWL1"
      or "WWO_CLAWR1"
      or "WWOTREEKNOTAREA" => PLANTS,
    _ when meshBaseName.StartsWith("ROOT_", StringComparison.Ordinal) => PLANTS,
    _ when meshBaseName.StartsWith("HEX_TOPIARY", StringComparison.Ordinal) => PLANTS,

    // Props
    "APPLE"
      or "AZTECMAN"
      or "BAB_LOFTLADDER_PD"
      or "BAB_LOFTLADDER_PD1"
      or "BLOB"
      or "BLOB_ORANGE"
      or "BOO_PURPLEBOOK_JP"
      or "BUR_DUSTBIN_MD"
      or "BUR_SACK_MD"
      or "CHEST"
      or "CIC_SPELLRING01_CS"
      or "CIC_SPELLRING02_CS"
      or "CIC_SPELLRING03_CS"
      or "EXC_FLAMERCANNONBASE_JP"
      or "EYEBALL"
      or "FEATHER_RED"
      or "GOLDCAULDRON"
      or "GOLDGLOBE"
      or "GSP_SCRIBE02_MD"
      or "HOURGLASS"
      or "HOURGLASS_NUMBERS"
      or "LKY_ONEWAYPANEL_JP"
      or "MEN_CARDBOX3_JP"
      or "MSC_POTIONSKITBAG_AB"
      or "MTL_HOGFLAMER_ML1"
      or "NAUGHTYBOX"
      or "PAI_LUMOSGREEN_MD"
      or "PINGAVIFORSVASE"
      or "PINGPORTRAIT"
      or "PUSHBLOCKFRAME"
      or "PUSHPULL"
      or "ROCK_HELD"
      or "ROCK_HELD1"
      or "ROLLINGBARREL"
      or "ROLLINGORANGEVASE"
      or "STAIRS_ROTATING"
      or "STAIRS_VERTICAL"
      or "STANDARD_SPELLS"
      or "STARCHART"
      or "TAPESTRY01"
      or "TAPESTRY02"
      or "TOPHAT"
      or "TORCH"
      or "GHOSTBOX"
      or "VAPORITION"
      or "TROPHYSHIELD" => PROPS,
    _ when meshBaseName.StartsWith("CON_TALL", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("FLIP_", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.Contains("CAGE", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.Contains("WEB", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.EndsWith("_PENNANT", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("GOLDVASE", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("LOG", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("MSC_EAFLIPENDOSHIELD", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("PNG_CIDERJUG", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("PURCHASEOBJ_", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("SPT_", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("STUDDEDBOX", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("TWIG", StringComparison.Ordinal) => PROPS,

    // Doors
    "GATE"
      or "LEFT_GATE"
      or "RIGHT_GATE" => DOORS,
    _ when meshBaseName.Contains("DOOR", StringComparison.Ordinal) => DOORS,

    // Switches
    "PUSHSWITCH_BUTTON" => PROPS,
    _ when meshBaseName.StartsWith("FLIPSWITCH_", StringComparison.Ordinal) => PROPS,
    _ when meshBaseName.StartsWith("INCENDIOSWITCH", StringComparison.Ordinal) => PROPS,

    // Creatures
    "FIREFLY01"
      or "FIREFLYTRAILS"
      or "CROW"
      or "NEWT_BODY"
      or "PLANKTON01"
      or "PLANKTON02"
      or "RABBIT_BODY"
      or "RABBIT_HEAD"
      or "SNAIL_ANTENNAE"
      or "SNAIL_BODY" => CREATURES,

    // Vehicles
    "CAR"
      or "CART_BODY"
      or "MINECART"
      or "WHEELS_BACK"
      or "WHEELS_FRONT"
       => VEHICLES,

    // Furniture
    "CUPBOARDB"
      or "MSC_OWLSTAND_AB" => PROPS,

    // FX
    "EMBER"
      or "WWA_FLASHINGHEADLIGHT_JP"
      or "FORCEFIELD"
      or "FORCEFIELD_CHUNK" => FX,
    _ when meshBaseName.StartsWith("DIABLO", StringComparison.Ordinal) => FX,
    _ when meshBaseName.StartsWith("EFX_", StringComparison.Ordinal) => FX,
    "FLAME"
      or "GHOST_SPARK"
      or "MIST"
      or "PNG_CIDERSPRAY_JP"
      or "SHOCK_WAVE"
      or "SMOKE01"
      or "SPIDERCHUNK"
      or "VASEDEBRIS" => FX,
    _ when meshBaseName.StartsWith("BLUEPARTICLE", StringComparison.Ordinal) => FX,
    _ when meshBaseName.StartsWith("TICKERTAPE", StringComparison.Ordinal) => FX,

    // Spells
    "EXPELLIARMUS_SPARK" => SPELLS,

    _ => UNCATEGORIZED,
  };

  /// <summary>
  ///   Staircase landings are named "Landing{floor}..." (1-7), but the night
  ///   variants of some are lowercase, e.g. "landing1a_night".
  /// </summary>
  private static bool TryToGetLandingFloor_(string name, out int floor) {
    const string prefix = "Landing";
    floor = name.Length > prefix.Length &&
            name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
        ? name[prefix.Length] - '0'
        : -1;
    return floor is >= 1 and <= 7;
  }
}
