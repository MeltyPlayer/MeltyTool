namespace mp64.api;

public enum Game {
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

public static class GameUtils {
  extension(Game) {
    public static Game FromRomCode(ReadOnlySpan<char> code) {
      return code switch {
          "CLBE" => Game.MARIO_PARTY_1_USA,
          "CLBJ" => Game.MARIO_PARTY_1_JPN,
          "NLBP" => Game.MARIO_PARTY_1_PAL,
          
          "NMWE" => Game.MARIO_PARTY_2_USA,
          "NMWJ" => Game.MARIO_PARTY_2_JPN,
          "NMWP" => Game.MARIO_PARTY_2_PAL,
          
          "NMVE" => Game.MARIO_PARTY_3_USA,
          "NMVJ" => Game.MARIO_PARTY_3_JPN,
          "NMVP" => Game.MARIO_PARTY_3_PAL,
      };
    }
  }

  extension(Game game) {
    public (uint upper, uint lower)[] GetMainFsOffsets() {
      return game switch {
          Game.MARIO_PARTY_1_USA => [
              // Default 0x0031C7E0
              (0x157a6, 0x157aa),
              (0x3c016, 0x3c01e),
          ],
          Game.MARIO_PARTY_1_JPN => [
              (0x156d6, 0x156da),
              (0x3bf32, 0x3bf3a),
          ],
          Game.MARIO_PARTY_1_PAL => [
              // Default 0x003373C0
              (0x160f6, 0x160fa),
              (0x3d83a, 0x3d83e),
          ],

          Game.MARIO_PARTY_2_USA => [
              // Default 0x0041DD30
              (0x416e6, 0x416ee),
          ],
          Game.MARIO_PARTY_2_JPN => [
              // Default 0x00417540
              (0x40d8a, 0x40d92),
          ],

          Game.MARIO_PARTY_3_USA => [
              (0x3619e, 0x361a6),
          ],
          Game.MARIO_PARTY_3_JPN => [
              // Default 0x005517F0
              (0x3618a, 0x36192),
          ],
          _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
      };
    }
  }
}