using System;
using System.Collections;
using System.Collections.Generic;

using fin.util.asserts;

using gx;

namespace mod.schema.mod;

public class VertexDescriptor : IEnumerable<(GxAttribute, GxAttributeType?)> {
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

  public IEnumerator<(GxAttribute, GxAttributeType?)> GetEnumerator()
    => this.ActiveAttributes();

  IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

  public IEnumerator<(GxAttribute, GxAttributeType?)> ActiveAttributes() {
      foreach (var attr in Enum.GetValues<GxAttribute>()) {
        if (!this.Exists(attr)) {
          continue;
        }

        if (attr is >= GxAttribute.POS and <= GxAttribute.TEX7) {
          yield return (attr, this.GetFormat(attr));
        } else {
          yield return (attr, null);
        }
      }
    }

  public bool Exists(GxAttribute enumval) {
      if (enumval == GxAttribute.NULL) {
        return false;
      }

      if (enumval is >= GxAttribute.TEX0MTXIDX and <= GxAttribute.TEX7MTXIDX) {
        var texMatId = enumval - GxAttribute.TEX0MTXIDX;
        return this.texMat[texMatId];
      }

      if (enumval is >= GxAttribute.TEX0 and <= GxAttribute.TEX7) {
        var texCoordId = enumval - GxAttribute.TEX0;
        return this.texcoord[texCoordId] != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxAttribute.PNMTXIDX) {
        return this.posMat;
      }

      if (enumval == GxAttribute.POS) {
        return this.position != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxAttribute.NRM) {
        return this.normal != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxAttribute.CLR0) {
        return this.color0 != GxAttributeType.NOT_PRESENT;
      }

      if (enumval == GxAttribute.CLR1) {
        return this.color1 != GxAttributeType.NOT_PRESENT;
      }

      if (enumval >= GxAttribute.POS_MTX_ARRAY) {
        return false;
      }

      Asserts.Fail($"Unknown enum for exists: {enumval}");
      return false;
    }

  public GxAttributeType GetFormat(GxAttribute enumval) {
      if (enumval == GxAttribute.POS) {
        return this.position;
      }

      if (enumval == GxAttribute.NRM) {
        return this.normal;
      }

      if (enumval == GxAttribute.CLR0) {
        return this.color0;
      }

      if (enumval == GxAttribute.CLR1) {
        return this.color1;
      }

      if (enumval is >= GxAttribute.TEX0 and <= GxAttribute.TEX7) {
        var texcoordid = enumval - GxAttribute.TEX0;
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

  /*def __str__(self) :
      return str([x for x in self.active_attributes()])

  def write_dmd(self, f):
      f.write("vcd ")
      for  attr in self.all_attributes():
          if attr is None:
              f.write("0 ")
          else:
              attr, fmt = attr
              if fmt is None:
                  f.write("1 ")
              elif fmt == VTXFMT.INDEX16:
                  f.write("1 ")
              else:
                  f.write("1 ")
      f.write("\n")*/
}