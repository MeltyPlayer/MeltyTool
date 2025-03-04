using System;
using System.Collections;
using System.Collections.Generic;

using fin.util.asserts;

using gx.displayList;

namespace pikmin1.schema.mod {
  public class VertexDescriptor
      : IEnumerable<(GxVertexAttribute, GxAttributeType?)> {
    public bool posMat = false;

    public bool[] texMat = [
        false,
        false,
        false,
        false,

        false,
        false,
        false,
        false
    ];

    public GxAttributeType position = GxAttributeType.NOT_PRESENT;

    public GxAttributeType normal = GxAttributeType.NOT_PRESENT;
    public bool useNbt;

    public GxAttributeType color0 = GxAttributeType.NOT_PRESENT;
    public GxAttributeType color1 = GxAttributeType.NOT_PRESENT;

    public readonly GxAttributeType[] texcoord = [
        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT,

        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT,
        GxAttributeType.NOT_PRESENT
    ];

    public IEnumerator<(GxVertexAttribute, GxAttributeType?)> GetEnumerator()
      => this.ActiveAttributes();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public IEnumerator<(GxVertexAttribute, GxAttributeType?)>
        ActiveAttributes() {
      foreach (var attr in Enum.GetValues<GxVertexAttribute>()) {
        if (!this.Exists(attr)) {
          continue;
        }

        if (attr is >= GxVertexAttribute.Position
                    and <= GxVertexAttribute.Tex7Coord) {
          yield return (attr, this.GetFormat(attr));
        } else {
          yield return (attr, null);
        }
      }
    }

    public bool Exists(GxVertexAttribute enumval) {
      if (enumval == GxVertexAttribute.NULL) {
        return false;
      }

      if (enumval is >= GxVertexAttribute.Tex0MatIdx
                     and <= GxVertexAttribute.Tex7MatIdx) {
        var texMatId = enumval - GxVertexAttribute.Tex0MatIdx;
        return this.texMat[texMatId];
      }

      if (enumval is >= GxVertexAttribute.Tex0Coord
                     and <= GxVertexAttribute.Tex7Coord) {
        var texCoordId = enumval - GxVertexAttribute.Tex0Coord;
        return this.texcoord[texCoordId] != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxVertexAttribute.PosMatIdx) {
        return this.posMat;
      }

      if (enumval == GxVertexAttribute.Position) {
        return this.position != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxVertexAttribute.Normal) {
        return this.normal != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxVertexAttribute.Color0) {
        return this.color0 != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxVertexAttribute.Color1) {
        return this.color1 != GxAttributeType.NOT_PRESENT;
      }

      if (enumval >= GxVertexAttribute.POS_MTX_ARRAY) {
        return false;
      }

      Asserts.Fail($"Unknown enum for exists: {enumval}");
      return false;
    }

    public GxAttributeType GetFormat(GxVertexAttribute enumval) {
      if (enumval == GxVertexAttribute.Position) {
        return this.position;
      }

      if (enumval == GxVertexAttribute.Normal) {
        return this.normal;
      }

      if (enumval == GxVertexAttribute.Color0) {
        return this.color0;
      }

      if (enumval == GxVertexAttribute.Color1) {
        return this.color1;
      }

      if (enumval is >= GxVertexAttribute.Tex0Coord
                     and <= GxVertexAttribute.Tex7Coord) {
        var texcoordid = enumval - GxVertexAttribute.Tex0Coord;
        return this.texcoord[texcoordid];
      }

      Asserts.Fail($"Unknown enum for format: {enumval}");
      return GxAttributeType.NOT_PRESENT;
    }

    /*def from_value(self, val):
    self.posmat = (val & 0b1) == 1
    val = val >> 1
    for i in range(8) :
        self.texmat[i] = (val & 0b1) == 1
    val = val >> 1

    self.position = get_vtxformat(val & 0b11)
    val = val >> 2
    self.normal = get_vtxformat(val & 0b11)
    val = val >> 2
    self.color0 = get_vtxformat(val & 0b11)
    val = val >> 2
    self.color1 = get_vtxformat(val & 0b11)
    val = val >> 2
    for i in range(8) :
        self.texcoord[i] = get_vtxformat(val & 0b11)
    val = val >> 2*/

    public void FromPikmin1(uint val, bool hasNormals = false) {
      this.position =
          GxAttributeType.INDEX_16; // Position is implied to be always enabled

      this.posMat = (val & 0b1) == 1;
      val = val >> 1;

      this.texMat[1] = (val & 0b1) == 1;
      val = val >> 1;

      if ((val & 0b1) == 1) {
        this.color0 = GxAttributeType.INDEX_16;
      }

      val = val >> 1;

      for (var i = 0; i < 8; ++i) {
        if ((val & 0b1) == 1) {
          this.texcoord[i] = GxAttributeType.INDEX_16;
        }

        val = val >> 1;
      }

      this.useNbt = (val & 0x20) != 0;

      if (hasNormals) {
        this.normal = GxAttributeType.INDEX_16;
      }
    }
  }
}