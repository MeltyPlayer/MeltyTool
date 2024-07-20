using schema.binary;
using schema.binary.attributes;

namespace uni.platforms.threeDs.tools.cia;

[BinarySchema]
public partial class Cia : IBinaryDeserializable {
  private const int SECTION_ALIGNMENT = 64;

  public CiaHeader Header { get; } = new();

  [Align(Cia.SECTION_ALIGNMENT)]
  public CiaCertificates Certificates { get; } = new();

  [Align(Cia.SECTION_ALIGNMENT)]
  public CiaTickets Tickets { get; } = new();

  [Align(Cia.SECTION_ALIGNMENT)]
  public CiaTmd Tmd { get; } = new();

  [Align(Cia.SECTION_ALIGNMENT)]
  public CiaContent Content { get; } = new();
}