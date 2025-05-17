namespace MariosPicross;

public enum PuzzleMergeType {
  UNMERGED,
  MERGE_30,
  MERGE_60,
}

public static class Constants {
  public static readonly Dictionary<uint, (uint puzzleOffset, uint? nameOffset,
          int puzzleCount, PuzzleMergeType merge)[]>
      OFFSETS_BY_FILE_CRC_32
          = new() {
              // Mario's Picross
              [0xF2D652AD] = [(0x92b0, 0xd934, 255, PuzzleMergeType.UNMERGED)],
              // Mario's Picross 2
              [0xF5AA5902] = [
                  // Easy Picross
                  (0x1FB88, null, 10, PuzzleMergeType.UNMERGED),
                  // Mario's Picross
                  (0x1C000, null, 4 * 100, PuzzleMergeType.MERGE_30),
                  // Mario's Picross Final
                  (0x1C000 + 0x20 * 4 * 100, null, 4 * 4,
                   PuzzleMergeType.MERGE_60),
                  // Wario's Picross
                  (0x54000, null, 4 * 100, PuzzleMergeType.MERGE_30),
                  // Wario's Picross Final
                  (0x54000 + 0x20 * 4 * 100, null, 4 * 4,
                   PuzzleMergeType.MERGE_60),
              ],
          };
}