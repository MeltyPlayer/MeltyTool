using System.Numerics;
using System.Runtime.InteropServices;

using modl.xml.level;

namespace modl.level.xml {
  public static class XmlExtensions {
    public static string GetPointerId(this XmlLevelObject xmlObject,
                                      string name)
      => xmlObject.WithNameAndType<XmlLevelPointer>(name).Item;

    public static IReadOnlyList<string> GetResourceIds(
        this XmlLevelObject xmlObject,
        string name)
      => xmlObject.WithNameAndType<XmlLevelResource>(name).Items;

    public static string GetAttributeString(
        this XmlLevelObject xmlObject,
        string name)
      => xmlObject.WithNameAndType<XmlLevelAttribute>(name).Items.Single();

    public static byte GetAttributeUInt8(this XmlLevelObject xmlObject,
                                         string name)
      => byte.Parse(xmlObject.GetAttributeString(name));

    public static ushort GetAttributeUInt16(this XmlLevelObject xmlObject,
                                            string name)
      => ushort.Parse(xmlObject.GetAttributeString(name));

    public static uint GetAttributeUInt32(this XmlLevelObject xmlObject,
                                          string name)
      => uint.Parse(xmlObject.GetAttributeString(name));

    public static float GetAttributeFloat(this XmlLevelObject xmlObject,
                                          string name)
      => float.Parse(xmlObject.GetAttributeString(name));

    public static Vector4 GetAttributeVector4(this XmlLevelObject xmlObject,
                                              string name)
      => xmlObject.GetAttributeFloats_<Vector4>(name);

    public static Vector2 GetAttributeVectorXz(this XmlLevelObject xmlObject,
                                               string name)
      => xmlObject.GetAttributeFloats_<Vector2>(name);

    public static Matrix4x4 GetAttributeMatrix4x4(this XmlLevelObject xmlObject,
                                                 string name)
      => xmlObject.GetAttributeFloats_<Matrix4x4>(name);

    private static unsafe T GetAttributeFloats_<T>(
        this XmlLevelObject xmlObject,
        string name)
        where T : struct {
      var size = sizeof(T);

      T obj = default;
      Span<T> valSpan = MemoryMarshal.CreateSpan(ref obj, 1);
      var span = MemoryMarshal.Cast<T, float>(valSpan);

      var i = 0;
      foreach (var value in xmlObject.GetAttributeString(name)
                                     .Split(",")
                                     .Select(float.Parse)) {
        span[i] = value;
      }

      return obj;
    }

    public static TType WithNameAndType<TType>(this XmlLevelObject xmlObject,
                                               string name)
        where TType : IXmlLevelObjectField
      => xmlObject.Fields
                  .OfType<TType>()
                  .Single(attribute => attribute.Name == name);
  }
}