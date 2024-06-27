using System;

using fin.schema.matrix;

using jsystem.G3D_Binary_File_Format;

using schema.binary;

#pragma warning disable CS8604

namespace jsystem.GCN;

public partial class BMD {
  public class EVP1Section {
    public const string Signature = "EVP1";
    public DataBlockHeader Header;
    public ushort Count;
    public ushort Padding;
    public uint[] Offsets;
    public byte[] Counts;
    public Matrix3x4f[] InverseBindMatrices { get; set; }
    public MultiMatrix[] WeightedIndices;

    public EVP1Section(IBinaryReader br, out bool OK) {
        long position1 = br.Position;
        bool OK1;
        this.Header = new DataBlockHeader(br, "EVP1", out OK1);
        if (!OK1) {
          OK = false;
        } else {
          this.Count = br.ReadUInt16();
          this.Padding = br.ReadUInt16();
          this.Offsets = br.ReadUInt32s(4);
          long position2 = br.Position;
          br.Position = position1 + (long) this.Offsets[0];
          this.Counts = br.ReadBytes((int) this.Count);

          br.Position = position1 + (long) this.Offsets[1];
          this.WeightedIndices = new MultiMatrix[(int) this.Count];
          int val1 = 0;
          for (int index1 = 0; index1 < (int) this.Count; ++index1) {
            this.WeightedIndices[index1] = new MultiMatrix();
            this.WeightedIndices[index1].Indices =
                new ushort[(int) this.Counts[index1]];
            for (int index2 = 0; index2 < (int) this.Counts[index1]; ++index2) {
              this.WeightedIndices[index1].Indices[index2] = br.ReadUInt16();
              val1 = Math.Max(val1,
                              (int) this.WeightedIndices[index1]
                                        .Indices[index2] + 1);
            }
          }

          br.Position = position1 + (long) this.Offsets[2];
          for (int index1 = 0; index1 < (int) this.Count; ++index1) {
            this.WeightedIndices[index1].Weights =
                new float[(int) this.Counts[index1]];
            for (int index2 = 0; index2 < (int) this.Counts[index1]; ++index2)
              this.WeightedIndices[index1].Weights[index2] = br.ReadSingle();
          }

          br.Position = position1 + (long) this.Offsets[3];
          this.InverseBindMatrices = new Matrix3x4f[val1];
          for (int index = 0; index < val1; ++index) {
            this.InverseBindMatrices[index] = br.ReadNew<Matrix3x4f>();
          }

          br.Position = position1 + (long) this.Header.size;
          OK = true;
        }
      }

    public class MultiMatrix {
      public float[] Weights;
      public ushort[] Indices;
    }
  }
}