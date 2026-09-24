// Decompiled by Jad v1.5.8f. Copyright 2001 Pavel Kouznetsov.
// Jad home page: http://www.kpdus.com/jad.html
// Decompiler options: packimports(3) 

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

namespace jagex.schema;

public class Animable {
  public virtual void method443(int i, int j, int k, int l, int i1, int j1, int k1, int l1, int i2) {
    Model model = getRotatedModel();
    if (model != null) {
      modelHeight = model.modelHeight;
      model.method443(i, j, k, l, i1, j1, k1, l1, i2);
    }
  }

  Model getRotatedModel() {
    return null;
  }

  public Animable() {
    modelHeight = 1000;
  }

  protected Class33[] aClass33Array1425;
  public int modelHeight;
}