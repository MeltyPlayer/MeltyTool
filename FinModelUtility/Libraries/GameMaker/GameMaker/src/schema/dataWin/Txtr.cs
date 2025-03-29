using schema.binary.attributes;
using schema.binary;

namespace gm.schema.dataWin;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/puggsoy/GMS-Explorer/blob/master/GMS%20Explorer/Chunks/TXTR.cs
/// </summary>
public class Txtr : IBinaryDeserializable {
  public TextureFile[] Spritesheets { get; set; }

  public void Read(IBinaryReader br) {
    var addressCount = br.ReadUInt32();
    this.Spritesheets = new TextureFile[addressCount];

    for (var i = 0; i < addressCount; ++i) {
      var address = br.ReadUInt32();

      var tmp = br.Position;
      br.Position = address;
 
      this.Spritesheets[i] = br.ReadNew<TextureFile>();

      br.Position = tmp;
    }
  }
}