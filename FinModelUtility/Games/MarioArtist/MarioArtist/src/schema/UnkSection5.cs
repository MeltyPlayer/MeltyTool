using CommunityToolkit.HighPerformance;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.schema;

[BinarySchema]
public partial class UnkSection5 : IBinaryDeserializable {
  public SubUnkSection5[] Subs { get; }
    = Enumerable.Range(0, 4)
                .Select(_ => new SubUnkSection5())
                .ToArray();
}

[BinarySchema]
public partial class SubUnkSection5 : IBinaryDeserializable {
  public uint UnkAddress { get; set; }
  public uint ChosenPartId { get; set; }

  [IntegerFormat(SchemaIntegerType.UINT32)]
  public bool IsEnabled { get; set; }

  public uint Unk1 { get; set; }
  public uint Unk2 { get; set; }
}