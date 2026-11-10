namespace mp64.api;

public enum GameVersion {
  MARIO_PARTY_1_USA,
  MARIO_PARTY_1_JPN,
  MARIO_PARTY_1_PAL,

  MARIO_PARTY_2_USA,
  MARIO_PARTY_2_JPN,
  MARIO_PARTY_2_PAL,

  MARIO_PARTY_3_USA,
  MARIO_PARTY_3_JPN,
  MARIO_PARTY_3_PAL,
}

public enum Game {
  MARIO_PARTY_1,
  MARIO_PARTY_2,
  MARIO_PARTY_3,
}

public static class GameUtils {
  extension(GameVersion) {
    public static GameVersion FromRomCode(ReadOnlySpan<char> code) {
      return code switch {
          "CLBE" => GameVersion.MARIO_PARTY_1_USA,
          "CLBJ" => GameVersion.MARIO_PARTY_1_JPN,
          "NLBP" => GameVersion.MARIO_PARTY_1_PAL,
          
          "NMWE" => GameVersion.MARIO_PARTY_2_USA,
          "NMWJ" => GameVersion.MARIO_PARTY_2_JPN,
          "NMWP" => GameVersion.MARIO_PARTY_2_PAL,
          
          "NMVE" => GameVersion.MARIO_PARTY_3_USA,
          "NMVJ" => GameVersion.MARIO_PARTY_3_JPN,
          "NMVP" => GameVersion.MARIO_PARTY_3_PAL,
      };
    }
  }

  extension(GameVersion gameVersion) {
    public Game GetGame()
      => gameVersion switch {
          GameVersion.MARIO_PARTY_1_USA => Game.MARIO_PARTY_1,
          GameVersion.MARIO_PARTY_1_JPN => Game.MARIO_PARTY_1,
          GameVersion.MARIO_PARTY_1_PAL => Game.MARIO_PARTY_1,

          GameVersion.MARIO_PARTY_2_USA => Game.MARIO_PARTY_2,
          GameVersion.MARIO_PARTY_2_JPN => Game.MARIO_PARTY_2,
          GameVersion.MARIO_PARTY_2_PAL => Game.MARIO_PARTY_2,

          GameVersion.MARIO_PARTY_3_USA => Game.MARIO_PARTY_3,
          GameVersion.MARIO_PARTY_3_JPN => Game.MARIO_PARTY_3,
          GameVersion.MARIO_PARTY_3_PAL => Game.MARIO_PARTY_3,

          _                             => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null)
      };

    public (uint upper, uint lower)[] GetMainFsOffsets() {
      return gameVersion switch {
          GameVersion.MARIO_PARTY_1_USA => [
              // Default 0x0031C7E0
              (0x157a6, 0x157aa),
              (0x3c016, 0x3c01e),
          ],
          GameVersion.MARIO_PARTY_1_JPN => [
              (0x156d6, 0x156da),
              (0x3bf32, 0x3bf3a),
          ],
          GameVersion.MARIO_PARTY_1_PAL => [
              // Default 0x003373C0
              (0x160f6, 0x160fa),
              (0x3d83a, 0x3d83e),
          ],

          GameVersion.MARIO_PARTY_2_USA => [
              // Default 0x0041DD30
              (0x416e6, 0x416ee),
          ],
          GameVersion.MARIO_PARTY_2_JPN => [
              // Default 0x00417540
              (0x40d8a, 0x40d92),
          ],

          GameVersion.MARIO_PARTY_3_USA => [
              (0x3619e, 0x361a6),
          ],
          GameVersion.MARIO_PARTY_3_JPN => [
              // Default 0x005517F0
              (0x3618a, 0x36192),
          ],
          _ => throw new ArgumentOutOfRangeException(nameof(gameVersion), gameVersion, null)
      };
    }
  }
}