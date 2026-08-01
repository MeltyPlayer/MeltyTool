using schema.binary;
using schema.binary.attributes;

namespace hm64.schema.mesh;

public class Mesh : IBinaryDeserializable, IChildOf<Map> {
  public Map Parent { get; set; }

  public TileDefinition[] TileDefinitions { get; set; }

  public void Read(IBinaryReader br) {
    br.PushLocalSpace();

    var maxTileIndex = this.Parent.Grid.TileIndices.Max();
    this.TileDefinitions = new TileDefinition[maxTileIndex];
    for (var i = 0; i < this.TileDefinitions.Length; ++i) {
      this.TileDefinitions[i] = br.SubreadAt(br.ReadUInt32(), br.ReadNew<TileDefinition>);
    }

    br.PopLocalSpace();
  }
}