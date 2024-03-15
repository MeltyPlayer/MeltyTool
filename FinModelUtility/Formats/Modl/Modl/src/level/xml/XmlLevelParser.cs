using System.IO.Compression;
using System.Xml;

using fin.io;
using fin.util.asserts;

using modl.api;
using modl.schema.xml;

namespace modl.xml.level {
  public class XmlLevelParser {
    public XmlLevel Parse(IReadOnlyGenericFile levelXmlFile,
                          GameVersion gameVersion) {
      Stream levelXmlStream;
      if (gameVersion == GameVersion.BW2) {
        using var gZipStream =
            new GZipStream(levelXmlFile.OpenRead(),
                           CompressionMode.Decompress);

        levelXmlStream = new MemoryStream();
        gZipStream.CopyTo(levelXmlStream);
        levelXmlStream.Position = 0;
      } else {
        levelXmlStream = levelXmlFile.OpenRead();
      }

      using var levelXmlReader = new StreamReader(levelXmlStream);
      var levelXml = new XmlDocument();
      levelXml.LoadXml(levelXmlReader.ReadToEnd());

      var xmlLevel = new XmlLevel();

      var instancesTag = levelXml["Instances"];
      foreach (var objectNode in instancesTag!.Children()) {
        xmlLevel.Instances.Add(this.ParseObject_(objectNode));
      }

      return xmlLevel;
    }

    private XmlLevelObject ParseObject_(XmlNode objectNode) {
      Asserts.Equal("Object", objectNode.LocalName);

      var xmlObject = new XmlLevelObject {
          Type = objectNode.GetAttributeValue("type"),
          Id = objectNode.GetAttributeValue("id"),
          Fields =
              objectNode
                  .Children()
                  .Select(fieldNode => fieldNode.LocalName switch {
                      "Attribute" => (IXmlLevelObjectField) this.ParseAttribute_(fieldNode),
                      "Pointer"   => this.ParsePointer_(fieldNode),
                      "Enum"      => this.ParseEnum_(fieldNode),
                      "Resource"  => this.ParseResource_(fieldNode),
                  }).ToArray(),
      };

    return xmlObject;
  }

  private XmlLevelAttribute ParseAttribute_(XmlNode attributeNode) {
    Asserts.Equal("Attribute", attributeNode.LocalName);
    return new XmlLevelAttribute {
        Name = attributeNode.GetAttributeValue("name"),
        Type = this.ParseAttributeType_(
            attributeNode.GetAttributeValue("type")),
        Items = this.ParseItems_(attributeNode),
    };
  }

  private XmlLevelAttributeType ParseAttributeType_(string attributeType)
    => attributeType switch {
        "sInt8"      => XmlLevelAttributeType.INT8,
        "sUInt8"     => XmlLevelAttributeType.UINT_8,
        "sUInt16"    => XmlLevelAttributeType.UINT_16,
        "sUInt32"    => XmlLevelAttributeType.UINT_32,
        "sFloat"     => XmlLevelAttributeType.FLOAT,
        "sFloat32"   => XmlLevelAttributeType.FLOAT,
        "sVectorXZ"  => XmlLevelAttributeType.VECTOR_XZ,
        "sVector4"   => XmlLevelAttributeType.VECTOR_4,
        "sU8Color"   => XmlLevelAttributeType.U8_COLOR,
        "sMatrix4x4" => XmlLevelAttributeType.MATRIX_4X4,
    };

  private XmlLevelPointer ParsePointer_(XmlNode pointerNode) {
    Asserts.Equal("Pointer", pointerNode.LocalName);
    return new XmlLevelPointer {
        Name = pointerNode.GetAttributeValue("name"),
        Type = pointerNode.GetAttributeValue("type"),
        Item = this.ParseItem_(pointerNode),
    };
  }

  private XmlLevelEnum ParseEnum_(XmlNode enumNode) {
    Asserts.Equal("Enum", enumNode.LocalName);
    return new XmlLevelEnum {
        Name = enumNode.GetAttributeValue("name"),
        Type = enumNode.GetAttributeValue("type"),
        Item = this.ParseItem_(enumNode),
    };
  }

  private XmlLevelResource ParseResource_(XmlNode resourceNode) {
    Asserts.Equal("Resource", resourceNode.LocalName);
    return new XmlLevelResource {
        Name = resourceNode.GetAttributeValue("name"),
        Type = resourceNode.GetAttributeValue("type"),
        Items = this.ParseItems_(resourceNode),
    };
  }

  private string ParseItem_(XmlNode parentNode) {
    var items = this.ParseItems_(parentNode);
    Asserts.Equal(1, items.Count);
    return items[0];
  }

  private IReadOnlyList<string> ParseItems_(XmlNode parentNode) {
    var itemNodes = parentNode.Children().ToArray();

    var expectedCount = parentNode.GetAttributeValue("elements");
    Asserts.Equal(expectedCount, itemNodes.Length);

    var items = new List<string>();
    foreach (var itemNode in itemNodes) {
      Asserts.Equal("Item", itemNode.LocalName);

      items.Add(itemNode.InnerText);
    }

    return items;
  }
}

public static class XmlExtensions {
  public static IEnumerable<XmlNode> Children(
      this XmlNode xmlNode) => xmlNode.Cast<XmlNode>();

  public static IEnumerable<XmlNode> Children(
      this XmlNodeList xmlNodeList) => xmlNodeList.Cast<XmlNode>();
}

}