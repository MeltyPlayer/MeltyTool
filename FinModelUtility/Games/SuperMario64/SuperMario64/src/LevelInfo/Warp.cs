using System.ComponentModel;

namespace sm64.LevelInfo;

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
    get { return warpFrom_ID; }
    set { warpFrom_ID = value; }
  }

  private byte warpTo_LevelID;

  [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
  [Browsable(true)]
  [DisplayName("To Level")]
  public byte WarpTo_LevelID {
    get { return warpTo_LevelID; }
    set { warpTo_LevelID = value; }
  }

  private byte warpTo_AreaID;

  [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
  [Browsable(true)]
  [DisplayName("To Area")]
  public byte WarpTo_AreaID {
    get { return warpTo_AreaID; }
    set { warpTo_AreaID = value; }
  }

  private byte warpTo_WarpID;

  [CustomSortedCategory("Connect Warps", 1, NUM_OF_CATERGORIES)]
  [Browsable(true)]
  [DisplayName("To ID")]
  public byte WarpTo_WarpID {
    get { return warpTo_WarpID; }
    set { warpTo_WarpID = value; }
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
        if (entry.Value == WarpTo_LevelID)
          return entry.Key + " (" + warpTo_AreaID + ")";
      }
      return "Unknown" + " (" + warpTo_AreaID + ")";
    }

  private string getWarpName() {
      if (isPaintingWarp) {
        return " [to " + getLevelName() + "]";
      } else {
        switch (WarpFrom_ID) {
          case 0xF0:
            return " (Success)" + " [to " + getLevelName() + "]";
          case 0xF1:
            return " (Failure)" + " [to " + getLevelName() + "]";
          case 0xF2:
          case 0xF3:
            return " (Special)" + " [to " + getLevelName() + "]";
          default:
            return " [to " + getLevelName() + "]";
        }
      }
    }

  public override string ToString() {
      //isPaintingWarp
      string warpName = "Warp 0x";
      if (isPaintingWarp)
        warpName = "Painting 0x";

      warpName += WarpFrom_ID.ToString("X2") + getWarpName();

      return warpName;
    }
}