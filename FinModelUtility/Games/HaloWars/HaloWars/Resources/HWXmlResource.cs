using System.Xml.Linq;

namespace HaloWarsTools;

public class HWXmlResource : HWResource {
  protected XElement XmlData { get; private set; }

  protected override void Load(byte[] bytes) {
    this.XmlData = XElement.Load(this.AbsolutePath);
  }
}