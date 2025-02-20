using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;


namespace HaloWarsTools {
  public class HWScnResource : HWXmlResource {
    public HWObjectInstance[] Objects => ValueCache.Get(ImportObjects);

    public static new HWScnResource FromFile(HWContext context, string filename) {
      return GetOrCreateFromFile(context, filename, HWResourceType.Scn) as HWScnResource;
    }

    private HWObjectInstance[] ImportObjects() {
      var list = new List<HWObjectInstance>();

      foreach (var obj in XmlData.Descendants("Object")) {
        var editorName = obj.Attribute("EditorName")?.Value;
        if (editorName == null) {
          continue;
        }

        var objectId = obj.DescendantNodes().OfType<XText>().Last().Value.Trim();
        var position = ConvertVector(DeserializeVector3(obj.Attribute("Position").Value));
        var forward = ConvertVector(DeserializeVector3(obj.Attribute("Forward").Value));
        var right = ConvertVector(DeserializeVector3(obj.Attribute("Right").Value));

        var hwObj = new HWObjectInstance() {
            Name = editorName,
            Definition = HWObjectDefinition.GetOrCreateFromId(Context, objectId),
            Matrix = Matrix4x4.CreateWorld(position, forward, Vector3.Cross(forward, right))
        };

        list.Add(hwObj);
      }

      return list.ToArray();
    }

    private static Vector3 ConvertVector(Vector3 vector) {
      return new Vector3(vector.Z, vector.X, vector.Y);
    }
  }
}