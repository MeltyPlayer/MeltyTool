// Decompiled with JetBrains decompiler
// Type: MKDS_Course_Modifier._3D_Formats.MA
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe


using jsystem.schema.j3dgraph.bmd.jnt1;

namespace jsystem._3D_Formats;

public class MA {
  public class Node {
    public Node(
        Jnt1Entry entry,
        string name,
        int parentJointIndex) {
        this.Entry = entry;
        this.Name = name;
        this.ParentJointIndex = parentJointIndex;
      }

    public Jnt1Entry Entry { get; set; }
    public string Name { get; set; }
    public int ParentJointIndex { get; set; }
  }
}