using fin.util.strings;

using schema.binary;

namespace modl.schema;

public static class SectionHeaderUtil {
  public static void ReadName(
      IBinaryReader br,
      out string sectionName) {
    br.PushMemberEndianness(Endianness.LittleEndian);
    sectionName = br.ReadString(4).Reverse();
    br.PopEndianness();
  }

  public static void AssertName(
      IBinaryReader br,
      string expectedSectionName) {
    br.PushMemberEndianness(Endianness.LittleEndian);
    br.AssertString(expectedSectionName.Reverse());
    br.PopEndianness();
  }


  public static void ReadSize(
      IBinaryReader br,
      out uint size) {
    br.PushMemberEndianness(Endianness.LittleEndian);
    size = br.ReadUInt32();
    br.PopEndianness();
  }

  public static void AssertSize(
      IBinaryReader br,
      uint expectedSize) {
    br.PushMemberEndianness(Endianness.LittleEndian);
    br.AssertUInt32(expectedSize);
    br.PopEndianness();
  }


  public static void ReadNameAndSize(
      IBinaryReader br,
      out string sectionName,
      out uint size) {
    SectionHeaderUtil.ReadName(br, out sectionName);
    SectionHeaderUtil.ReadSize(br, out size);
  }

  public static void AssertNameAndReadSize(
      IBinaryReader br,
      string expectedSectionName,
      out uint size) {
    SectionHeaderUtil.AssertName(br, expectedSectionName);
    SectionHeaderUtil.ReadSize(br, out size);
  }

  public static void AssertNameAndSize(
      IBinaryReader br,
      string expectedSectionName,
      uint expectedSize) {
    SectionHeaderUtil.AssertName(br, expectedSectionName);
    SectionHeaderUtil.AssertSize(br, expectedSize);
  }
}