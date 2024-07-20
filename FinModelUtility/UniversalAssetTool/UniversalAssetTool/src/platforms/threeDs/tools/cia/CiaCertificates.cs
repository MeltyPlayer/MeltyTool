using schema.binary;
using schema.binary.attributes;

namespace uni.platforms.threeDs.tools.cia;

public enum EsSigType : uint {
  RSA4096_SHA1 = 0x00010000, /* RSA 4096 bit signature */
  RSA2048_SHA1 = 0x00010001, /* RSA 2048 bit signature */
  ECC_SHA1 = 0x00010002, /* ECC signature 512 bits */
  RSA4096_SHA256 = 0x00010003, /* RSA 4096 bit sig using SHA-256 */
  RSA2048_SHA256 = 0x00010004, /* RSA 2048 bit sig using SHA-256 */ // note that Switch Ticket has this word swapped
  ECC_SHA256 = 0x00010005, /* ECC sig 512 bits using SHA-256 */
  HMAC_SHA1 = 0x00010006, /* HMAC-SHA1 160 bit signature */
};

public partial class CiaCertificates : IChildOf<Cia>, IBinaryDeserializable {
  public Cia Parent { get; set; }

  public EsSigType SigType { get; set; }

  public void Read(IBinaryReader br) {
      var startingPosition = br.Position;

      this.SigType = (EsSigType) br.ReadUInt32();

      br.Position = startingPosition + this.Parent.Header.CertificateSize;
    }
}