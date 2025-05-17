namespace MariosPicross;

public static class Constants {
  public static readonly Dictionary<uint, (uint puzzleOffset, uint? nameOffset, int puzzleCount, bool merge)>
      OFFSETS_BY_FILE_CRC_32
          = new() {
              // Mario's Picross
              [0xF2D652AD] = (0x92b0, 0xd934, 255, false),
              // Mario's Picross 2
              [0xF5AA5902] = (0x54000, null, 400, true),
          };
}