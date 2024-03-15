namespace modl.xml.level {
  public enum XmlLevelAttributeType {
    INT8,
    UINT_8,
    UINT_16,
    UINT_32,
    FLOAT,
    VECTOR_XZ,
    VECTOR_4,
    U8_COLOR,
    MATRIX_4X4
  }

  public class XmlLevelAttribute : BXmlLevelTypedItemList<XmlLevelAttributeType>, IXmlLevelObjectField;
}