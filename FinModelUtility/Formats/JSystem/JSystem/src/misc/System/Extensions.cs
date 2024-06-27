// Decompiled with JetBrains decompiler
// Type: System.Extensions
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System.Drawing;

using schema.binary;

namespace System;

public static class Extensions {
  public static Color ReadColor16(this IBinaryReader br) {
      short num1 = br.ReadInt16();
      short num2 = br.ReadInt16();
      short num3 = br.ReadInt16();
      return Color.FromArgb(
          (int) Math.Abs(br.ReadInt16()) & (int) byte.MaxValue,
          (int) Math.Abs(num1) & (int) byte.MaxValue,
          (int) Math.Abs(num2) & (int) byte.MaxValue,
          (int) Math.Abs(num3) & (int) byte.MaxValue);
    }

  public static Color ReadColor8(this IBinaryReader br) {
      int red = (int) br.ReadByte();
      int green = (int) br.ReadByte();
      int blue = (int) br.ReadByte();
      return Color.FromArgb((int) br.ReadByte(), red, green, blue);
    }
}