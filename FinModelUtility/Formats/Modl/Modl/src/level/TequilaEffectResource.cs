using modl.level.xml;
using modl.xml.level;

namespace modl.level {
  public class TequilaEffectResource : BLevelObject {
    public string Name { get; set; }

    protected override void Populate(
        XmlLevelObject xmlLevelObject,
        Level level) {
      this.Name = xmlLevelObject.GetAttributeString("mName");
    }
  }
}