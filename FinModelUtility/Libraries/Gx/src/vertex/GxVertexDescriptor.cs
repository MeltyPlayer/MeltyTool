using System.Collections;

using fin.math;

namespace gx.vertex;

public struct GxVertexDescriptor(uint value)
    : IEnumerable<(GxVertexAttribute, GxAttributeType?)> {
  private IEnumerable<(GxVertexAttribute, GxAttributeType?)>?
      cachedEnumerable_;

  public uint Value {
    get;
    set {
      field = value;
      this.cachedEnumerable_ = null;
    }
  } = value;

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxVertexAttribute, GxAttributeType?)> GetEnumerator() {
    if (this.cachedEnumerable_ == null) {
      var attributeList =
          new LinkedList<(GxVertexAttribute, GxAttributeType?)>();
      this.cachedEnumerable_ = attributeList;

      // Read flags from value
      var value = this.Value;

      if (value.GetBit(0)) {
        attributeList.AddLast((GxVertexAttribute.PosMatIdx, null));
      }

      value >>= 1;

      for (uint i = 0; i < 8; ++i) {
        if (value.GetBit(0)) {
          attributeList.AddLast((GxVertexAttribute.Tex0MatIdx + i, null));
        }

        value >>= 1;
      }

      var positionFormat = (GxAttributeType) (value & 3);
      if (positionFormat != GxAttributeType.NOT_PRESENT) {
        attributeList.AddLast((GxVertexAttribute.Position, positionFormat));
      }

      value >>= 2;

      var normalFormat = (GxAttributeType) (value & 3);
      if (normalFormat != GxAttributeType.NOT_PRESENT) {
        attributeList.AddLast((GxVertexAttribute.Normal, normalFormat));
      }

      value >>= 2;

      var colorFormat0 = (GxAttributeType) (value & 3);
      if (colorFormat0 != GxAttributeType.NOT_PRESENT) {
        attributeList.AddLast((GxVertexAttribute.Color0, colorFormat0));
      }

      value >>= 2;

      var colorFormat1 = (GxAttributeType) (value & 3);
      if (colorFormat1 != GxAttributeType.NOT_PRESENT) {
        attributeList.AddLast((GxVertexAttribute.Color1, colorFormat1));
      }

      value >>= 2;

      for (uint i = 0; i < 8; ++i) {
        if (value.GetBit(0)) {
          attributeList.AddLast((GxVertexAttribute.Tex0Coord + i, null));
        }

        value >>= 1;
      }

      if (value != 0) {
        throw new NotImplementedException();
      }
    }

    return this.cachedEnumerable_.GetEnumerator();
  }
}