using System.Numerics;

using modl.level.xml;
using modl.xml.level;

namespace modl.level;

public class Waypoint : BLevelObject {
  public Waypoint? NextWp { get; set; }
  public uint Flags { get; set; }
  public float PauseTime { get; set; }
  public float Speed { get; set; }
  public Waypoint? OptionalNextWp1 { get; set; }
  public Waypoint? OptionalNextWp2 { get; set; }
  public Matrix4x4 Mat { get; set; }
  public uint SystemFlags { get; set; }

  protected override void Populate(
      XmlLevelObject xmlObject,
      Level level) {
    this.NextWp = level.Get<Waypoint?>(xmlObject.GetPointerId("NextWP"));
    this.Flags = xmlObject.GetAttributeUInt32("Flags");
    this.PauseTime = xmlObject.GetAttributeFloat("mPauseTime");
    this.Speed = xmlObject.GetAttributeFloat("mSpeed");
    this.OptionalNextWp1
        = level.Get<Waypoint?>(xmlObject.GetPointerId("mOptionalNextWP1"));
    this.OptionalNextWp2
        = level.Get<Waypoint?>(xmlObject.GetPointerId("mOptionalNextWP2"));
    this.Mat = xmlObject.GetAttributeMatrix4x4("Mat");
  }
}