using fin.math;

using gx;
using gx.displayList;

namespace modl.schema.modl.common;

public class BattalionWarsVertexDescriptor(uint value)
    : BVertexDescriptor(value) {
  protected override IEnumerable<(GxVertexAttribute, GxAttributeType?,
          GxColorComponentType?)>
      GetEnumerator(uint value) {
    if (value.GetBit(0)) {
      yield return (GxVertexAttribute.PosMatIdx, null, null);
    }

    value >>= 1;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.Tex0MatIdx + i, null, null);
      }

      value >>= 1;
    }

    var positionFormat = (GxAttributeType) (value & 3);
    if (positionFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Position, positionFormat, null);
    }

    value >>= 2;

    var normalFormat = (GxAttributeType) (value & 3);
    if (normalFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Normal, normalFormat, null);
    }

    value >>= 2;

    var colorFormat0 = (GxAttributeType) (value & 3);
    if (colorFormat0 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Color0, colorFormat0, null);
    }

    value >>= 2;

    var colorFormat1 = (GxAttributeType) (value & 3);
    if (colorFormat1 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Color1, colorFormat1, null);
    }

    value >>= 2;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.Tex0Coord + i, null, null);
      }

      value >>= 1;
    }

    if (value != 0) {
      throw new NotImplementedException();
    }
  }
}