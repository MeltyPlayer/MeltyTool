namespace MarioPicross;

public static class Constants {
  public static readonly Dictionary<uint, (uint puzzleOffset, uint nameOffset)>
      OFFSETS_BY_FILE_CRC_32
          = new() {
              [0xF2D652AD] = (0x92b0, 0xd934),
          };
}