using schema.binary;

namespace uni.platforms.threeDs.tools.cia;

public enum CiaFormatVersion : ushort {
  DEFAULT = 0,
  SIMPLE = 0xFF,
}

[BinarySchema]
public partial class CiaHeader : IBinaryConvertible {
  public uint HeaderSize { get; set; }
  private readonly ushort padding_ = 0;
  public CiaFormatVersion FormatVersion { get; set; }
  public uint CertificateSize { get; set; }
  public uint TicketSize { get; set; }
  public uint TmdSize { get; set; }
  public uint FooterSize { get; set; }
  public uint ContentSize { get; set; }
}