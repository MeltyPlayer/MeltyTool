using System.Drawing;

namespace UoT {
  public static class TunicConstants {
    /**
     * Selected tunic id @ 15E6D0
     * - 17: kokiri
     * - 18: goron
     * - 19: zora
     *
     * Doesn't impact color until unpause.
     */
    public const int TUNIC_COLOR_DL_RDRAM_OFFSET = 0x1B478C;

    public static readonly Color TUNIC_COLOR_KOKIRI =
        Color.FromArgb(0x1E, 0x69, 0x1B);

    public static readonly Color TUNIC_COLOR_GORON =
        Color.FromArgb(0x64, 0x14, 0x00);

    public static readonly Color TUNIC_COLOR_ZORA =
        Color.FromArgb(0x00, 0x3C, 0x64);

    public const int TUNIC_TYPE_RDRAM_OFFSET = 0x2246FC;

    public enum TunicType {
      KOKIRI = 0,
      GORON = 1,
      ZORA = 2,
    }
  }
}