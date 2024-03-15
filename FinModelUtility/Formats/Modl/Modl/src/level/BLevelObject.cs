using modl.xml.level;

namespace modl.level {
  public abstract class BLevelObject {
    public uint Id { get; set; }

    protected virtual void Populate(XmlLevelObject xmlObject, Level level)
      => throw new NotImplementedException();
  }
}