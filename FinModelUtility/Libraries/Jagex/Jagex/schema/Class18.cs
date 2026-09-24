// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

using schema.binary;

namespace jagex.schema;

public sealed class Class18 {

  public Class18(IBinaryReader br) {
    int anInt341 = br.ReadByte();
    anIntArray342 = new int[anInt341];
    anIntArrayArray343 = new int[anInt341][];
    for (int j = 0; j < anInt341; j++) {
      anIntArray342[j] = br.ReadByte();
    }

    for (int k = 0; k < anInt341; k++) {
      int l = br.ReadByte();
      anIntArrayArray343[k] = new int[l];
      for (int i1 = 0; i1 < l; i1++) {
        anIntArrayArray343[k][i1] = br.ReadByte();
      }

    }

  }

  public readonly int[] anIntArray342;
  public readonly int[][] anIntArrayArray343;
}