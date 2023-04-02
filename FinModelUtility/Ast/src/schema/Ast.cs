using schema.binary;


namespace ast.schema {
  public partial class Ast : IBinaryDeserializable {
    public StrmHeader StrmHeader { get; } = new();

    public IReadOnlyList<short>[] ChannelData { get; private set; }

    public void Read(IEndianBinaryReader er) {
      this.StrmHeader.Read(er);

      switch (this.StrmHeader.Format) {
        case AstAudioFormat.ADPCM: {
          this.ReadAdpcm_(er);
          break;
        }
        case AstAudioFormat.PCM16: {
          this.ReadPcm16_(er);
          break;
        }
        default: throw new NotImplementedException();
      }
    }
  }
}