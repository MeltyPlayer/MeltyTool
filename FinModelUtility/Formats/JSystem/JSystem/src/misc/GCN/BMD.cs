using System.Collections.Generic;
using System.IO;

using jsystem._3D_Formats;
using jsystem.schema.j3dgraph.bmd;
using jsystem.schema.j3dgraph.bmd.drw1;
using jsystem.schema.j3dgraph.bmd.inf1;
using jsystem.schema.j3dgraph.bmd.jnt1;
using jsystem.schema.j3dgraph.bmd.tex1;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class BMD {
  public BmdHeader Header;
  public Inf1 INF1 { get; set; }
  public VTX1Section VTX1;
  public EVP1Section EVP1;
  public Drw1 DRW1 { get; set; }
  public Jnt1 JNT1 { get; set; }
  public SHP1Section SHP1;
  public MAT3Section MAT3;
  public Tex1 TEX1 { get; set; }

  public BMD(byte[] file) {
    using var br =
        new SchemaBinaryReader((Stream) new MemoryStream(file),
                               Endianness.BigEndian);
    this.Header = br.ReadNew<BmdHeader>();

    bool OK;
    while (!br.Eof) {
      switch (br.ReadString(4)) {
        case nameof(this.INF1):
          br.Position -= 4L;
          this.INF1 = br.ReadNew<Inf1>();
          break;
        case nameof(this.VTX1):
          br.Position -= 4L;
          this.VTX1 = new VTX1Section(br, out OK);
          if (!OK) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 4");
            return;
          } else
            break;
        case nameof(this.EVP1):
          br.Position -= 4L;
          this.EVP1 = new EVP1Section(br, out OK);
          if (!OK) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 4");
            return;
          } else
            break;
        case nameof(this.DRW1):
          br.Position -= 4L;
          this.DRW1 = br.ReadNew<Drw1>();
          break;
        case nameof(this.JNT1):
          br.Position -= 4L;
          this.JNT1 = br.ReadNew<Jnt1>();
          break;
        case nameof(this.SHP1):
          br.Position -= 4L;
          this.SHP1 = new SHP1Section(br, out OK);
          if (!OK) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 7");
            return;
          } else
            break;
        case "MAT1":
        case "MAT2":
        case nameof(this.MAT3):
          br.Position -= 4L;
          this.MAT3 = new MAT3Section(br, out OK);
          if (!OK) {
            // TODO: Message box
            //int num2 = (int) System.Windows.Forms.MessageBox.Show("Error 8");
            return;
          } else
            break;
        case nameof(this.TEX1):
          br.Position -= 4L;
          this.TEX1 = br.ReadNew<Tex1>();
          break;
        default:
          return;
      }
    }
  }

  public MA.Node[] GetJoints() {
    var nodeIndexStack = new Stack<int>();
    nodeIndexStack.Push(-1);
    var nodeList = new List<MA.Node>();
    int nodeIndex = -1;
    foreach (Inf1Entry entry in this.INF1.Data.Entries) {
      switch (entry.Type) {
        case Inf1EntryType.TERMINATOR:
          goto label_7;
        case Inf1EntryType.HIERARCHY_DOWN:
          nodeIndexStack.Push(nodeIndex);
          break;
        case Inf1EntryType.HIERARCHY_UP:
          nodeIndexStack.Pop();
          break;
        case Inf1EntryType.JOINT:
          var jnt1 = this.JNT1.Data;
          var jointIndex = jnt1.RemapTable[entry.Index];
          nodeList.Add(new MA.Node(
                           jnt1.Joints[jointIndex],
                           jnt1.StringTable[jointIndex],
                           nodeIndexStack.Peek()));
          nodeIndex = entry.Index;
          break;
        case Inf1EntryType.MATERIAL:
          break;
        case Inf1EntryType.SHAPE:
          break;
      }
    }

    label_7:
    return nodeList.ToArray();
  }
}