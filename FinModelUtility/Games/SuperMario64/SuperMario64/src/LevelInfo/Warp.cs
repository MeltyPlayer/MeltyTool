using System.ComponentModel;

namespace sm64.LevelInfo {
  public class Warp {
    public Warp(bool isPaintingWarp) {
      this.isPaintingWarp = isPaintingWarp;
    }

    private const ushort NUM_OF_CATERGORIES = 2;
    private bool isPaintingWarp = false;

    private byte warpFrom_ID;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("From ID")]
    public byte WarpFrom_ID {
      get { return this.warpFrom_ID; }
      set { this.warpFrom_ID = value; }
    }

    private byte warpTo_LevelID;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("To Level")]
    public byte WarpTo_LevelID {
      get { return this.warpTo_LevelID; }
      set { this.warpTo_LevelID = value; }
    }

    private byte warpTo_AreaID;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("To Area")]
    public byte WarpTo_AreaID {
      get { return this.warpTo_AreaID; }
      set { this.warpTo_AreaID = value; }
    }

    private byte warpTo_WarpID;

    [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [DisplayName("To ID")]
    public byte WarpTo_WarpID {
      get { return this.warpTo_WarpID; }
      set { this.warpTo_WarpID = value; }
    }

    [CustomSortedCategory("Info", 2, NUM_OF_CATERGORIES)]
    [Browsable(true)]
    [Description("Location inside the ROM file")]
    [DisplayName("Address")]
    [ReadOnly(true)]
    public string Address { get; set; }

    public void MakeReadOnly() {
      TypeDescriptor.AddAttributes(
          this,
          [new ReadOnlyAttribute(true)]);
    }

    private string getLevelName() {
      ROM rom = ROM.Instance;
      foreach (KeyValuePair<string, ushort> entry in rom.levelIDs) {
        if (entry.Value == this.WarpTo_LevelID)
          return entry.Key + " (" + this.warpTo_AreaID + ")";
      }
      return "Unknown" + " (" + this.warpTo_AreaID + ")";
    }

    private string getWarpName() {
      if (this.isPaintingWarp) {
        return " [to " + this.getLevelName() + "]";
      } else {
        switch (this.WarpFrom_ID) {
          case 0xF0:
            return " (Success)" + " [to " + this.getLevelName() + "]";
          case 0xF1:
            return " (Failure)" + " [to " + this.getLevelName() + "]";
          case 0xF2:
          case 0xF3:
            return " (Special)" + " [to " + this.getLevelName() + "]";
          default:
            return " [to " + this.getLevelName() + "]";
        }
      }
    }

    public override string ToString() {
      //isPaintingWarp
      string warpName = "Warp 0x";
      if (this.isPaintingWarp)
        warpName = "Painting 0x";

      warpName += this.WarpFrom_ID.ToString("X2") + this.getWarpName();

      return warpName;
    }
  }
}