using schema.binary;

namespace ast.schema;

public partial class Ast : IBinaryDeserializable {
  public StrmHeader StrmHeader { get; } = new();

  public IReadOnlyList<short>[] ChannelData { get; private set; }

  public void Read(IBinaryReader br) {
    this.StrmHeader.Read(br);

    switch (this.StrmHeader.Format) {
      case AstAudioFormat.ADPCM: {
        this.ReadAdpcm_(br);
        break;
      }
      case AstAudioFormat.PCM16: {
        this.ReadPcm16_(br);
        break;
      }
      default: throw new NotImplementedException();
    }
  }
}