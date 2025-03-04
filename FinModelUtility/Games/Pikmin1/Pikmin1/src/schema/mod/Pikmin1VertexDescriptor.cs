using System.Collections.Generic;

using gx.displayList;

namespace pikmin1.schema.mod {
  public class Pikmin1VertexDescriptor(uint value, bool hasNormals)
      : BVertexDescriptor(value) {
    public bool UseNbt { get; private set; }

    protected override IEnumerable<(GxVertexAttribute, GxAttributeType?)>
        GetEnumerator(uint value) {
      if ((value & 0b1) == 1) {
        yield return (GxVertexAttribute.PosMatIdx, null);
      }
      value >>= 1;

      if ((value & 0b1) == 1) {
        yield return (GxVertexAttribute.Tex1MatIdx, null);
      }
      value >>= 1;

      // Position is implied to be always enabled
      yield return (GxVertexAttribute.Position, GxAttributeType.INDEX_16);

      if (hasNormals) {
        yield return (GxVertexAttribute.Normal, GxAttributeType.INDEX_16);
      }

      if ((value & 0b1) == 1) {
        yield return (GxVertexAttribute.Color0, GxAttributeType.INDEX_16);
      }
      value >>= 1;

      for (uint i = 0; i < 8; ++i) {
        if ((value & 0b1) == 1) {
          yield return (GxVertexAttribute.Tex0Coord + i,
                        GxAttributeType.INDEX_16);
        }

        value >>= 1;
      }

      this.UseNbt = (value & 0x20) != 0;
    }
  }
}