using fin.math;

namespace gx.displayList;

public class GxVertexDescriptor(uint value) : BVertexDescriptor(value) {
  protected override IEnumerable<(GxVertexAttribute, GxAttributeType?)>
      GetEnumerator(uint value) {
    if (value.GetBit(0)) {
      yield return (GxVertexAttribute.PosMatIdx, null);
    }

    value >>= 1;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.Tex0MatIdx + i, null);
      }

      value >>= 1;
    }

    var positionFormat = (GxAttributeType) (value & 3);
    if (positionFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Position, positionFormat);
    }

    value >>= 2;

    var normalFormat = (GxAttributeType) (value & 3);
    if (normalFormat != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Normal, normalFormat);
    }

    value >>= 2;

    var colorFormat0 = (GxAttributeType) (value & 3);
    if (colorFormat0 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Color0, colorFormat0);
    }

    value >>= 2;

    var colorFormat1 = (GxAttributeType) (value & 3);
    if (colorFormat1 != GxAttributeType.NOT_PRESENT) {
      yield return (GxVertexAttribute.Color1, colorFormat1);
    }

    value >>= 2;

    for (uint i = 0; i < 8; ++i) {
      if (value.GetBit(0)) {
        yield return (GxVertexAttribute.Tex0Coord + i, null);
      }

      value >>= 1;
    }

    if (value != 0) {
      throw new NotImplementedException();
    }
  }
}