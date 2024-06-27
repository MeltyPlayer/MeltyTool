using System.Collections;

using fin.math;

namespace gx;

public struct GxVertexDescriptor :
    IEnumerable<(GxVertexAttribute, GxAttributeType?)> {
  private IEnumerable<(GxVertexAttribute, GxAttributeType?)>?
      cachedEnumerable_;
    
  public GxVertexDescriptor(uint value) {
    this.Value = value;
  }

  public uint Value { get; }

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

      for (var i = 0; i < 8; ++i) {
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

      for (var i = 0; i < 8; ++i) {
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

public enum GxVertexAttribute {
  PosMatIdx,

  Tex0MatIdx,
  Tex1MatIdx,
  Tex2MatIdx,
  Tex3MatIdx,
  Tex4MatIdx,
  Tex5MatIdx,
  Tex6MatIdx,
  Tex7MatIdx,

  Position,
  Normal,

  Color0,
  Color1,

  Tex0Coord,
  Tex1Coord,
  Tex2Coord,
  Tex3Coord,
  Tex4Coord,
  Tex5Coord,
  Tex6Coord,
  Tex7Coord,
}