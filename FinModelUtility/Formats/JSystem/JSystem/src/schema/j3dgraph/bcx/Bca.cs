// Decompiled with JetBrains decompiler
// Type: MKDS_Course_Modifier.GCN.BCA
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System;
using System.IO;
using System.Linq;

using jsystem.G3D_Binary_File_Format;

using schema.binary;
using schema.binary.attributes;

namespace jsystem.schema.j3dgraph.bcx;

/// <summary>
///   BCA files define joint animations where each frame is defined.
/// </summary>
[Endianness(Endianness.BigEndian)]
public partial class Bca : IBcx {
  public BcaHeader Header;
  public ANF1Section ANF1;

  public Bca(byte[] file) {
      using var br = new SchemaBinaryReader((Stream)new MemoryStream(file),
                                            Endianness.BigEndian);
      this.Header = br.ReadNew<BcaHeader>();
      this.ANF1 = new ANF1Section(br, out _);
    }

  public IAnx1 Anx1 => this.ANF1;

  [BinarySchema]
  public partial class BcaHeader : IBinaryConvertible {
    private readonly string magic_ = "J3D1bca1";

    [WSizeOfStreamInBytes]
    private uint fileSize_;

    private readonly uint sectionCount_ = 1;

    [SequenceLengthSource(16)]
    private byte[] padding_;
  }

  public partial class ANF1Section : IAnx1 {
    public const string Signature = "ANF1";
    public DataBlockHeader Header;
    public byte LoopFlags;
    public byte AngleMultiplier;
    public ushort AnimLength;
    public ushort NrJoints;
    public ushort NrScale;
    public ushort NrRot;
    public ushort NrTrans;
    public uint JointOffset;
    public uint ScaleOffset;
    public uint RotOffset;
    public uint TransOffset;
    public float[] Scale;
    public short[] Rotation;
    public float[] Translation;

    public ANF1Section(IBinaryReader br, out bool OK) {
        bool OK1;

        this.Header = new DataBlockHeader(br, "ANF1", out OK1);
        if (!OK1) {
          OK = false;
        } else {
          this.LoopFlags = br.ReadByte();
          this.AngleMultiplier = br.ReadByte();
          this.AnimLength = br.ReadUInt16();
          this.NrJoints = br.ReadUInt16();
          this.NrScale = br.ReadUInt16();
          this.NrRot = br.ReadUInt16();
          this.NrTrans = br.ReadUInt16();
          this.JointOffset = br.ReadUInt32();
          this.ScaleOffset = br.ReadUInt32();
          this.RotOffset = br.ReadUInt32();
          this.TransOffset = br.ReadUInt32();

          br.Position = (long)(32U + this.ScaleOffset);
          this.Scale = br.ReadSingles((int)this.NrScale);
          br.Position = (long)(32U + this.RotOffset);
          this.Rotation = br.ReadInt16s((int)this.NrRot);
          br.Position = (long)(32U + this.TransOffset);
          this.Translation = br.ReadSingles((int)this.NrTrans);
          float rotScale = (float)(1 * Math.PI / 32768f);
          br.Position = (long)(32U + this.JointOffset);
          this.Joints = new AnimatedJoint[(int)this.NrJoints];
          for (int index = 0; index < (int)this.NrJoints; ++index) {
            var animatedJoint = new AnimatedJoint(br);
            animatedJoint.SetValues(this.Scale,
                                    this.Rotation,
                                    this.Translation,
                                    rotScale);
            this.Joints[index] = animatedJoint;
          }
          OK = true;
        }
      }

    public int FrameCount => this.AnimLength;
    public IAnimatedJoint[] Joints { get; }

    public partial class AnimatedJoint : IAnimatedJoint {
      public AnimComponent[] axes;

      public AnimatedJoint(IBinaryReader br) {
          this.axes = new AnimComponent[3];
          for (var i = 0; i < axes.Length; ++i) {
            this.axes[i] = br.ReadNew<AnimComponent>();
          }
        }

      public IJointAnim Values { get; private set; }

      public void SetValues(
          float[] scales,
          short[] rotations,
          float[] translations,
          float totScale) {
          this.Values =
              new JointAnim(
                  this,
                  scales,
                  rotations,
                  translations,
                  totScale);
        }

      public float GetAnimValue(IJointAnimKey[] keys, float t) {
          if (keys.Length == 0)
            return 0.0f;
          return keys.Length == 1 ? keys[0].Value : keys[(int)t].Value;
        }

      [BinarySchema]
      public partial class AnimComponent : IBinaryConvertible {
        public AnimIndex S { get; } = new();
        public AnimIndex R { get; } = new();
        public AnimIndex T { get; } = new();

        [BinarySchema]
        public partial class AnimIndex : IBinaryConvertible {
          public ushort Count;
          public ushort Index;
        }
      }

      public class JointAnim : IJointAnim {
        public JointAnim(
            AnimatedJoint joint,
            float[] scales,
            short[] rotations,
            float[] translations,
            float rotScale) {
            this.Scales = joint.axes.Select(
                axisSrc => {
                  this.SetKeysSt_(out var axis, scales, axisSrc.S);
                  return axis;
                }).ToArray();
            this.Rotations = joint.axes.Select(
                axisSrc => {
                  this.SetKeysR_(out var axis, rotations, rotScale, axisSrc.R);
                  return axis;
                }).ToArray();
            this.Translations = joint.axes.Select(
                axisSrc => {
                  this.SetKeysSt_(out var axis, translations, axisSrc.T);
                  return axis;
                }).ToArray();
          }

        public IJointAnimKey[][] Scales { get; }
        public IJointAnimKey[][] Rotations { get; }
        public IJointAnimKey[][] Translations { get; }

        private void SetKeysSt_(
            out IJointAnimKey[] dst,
            float[] src,
            AnimComponent.AnimIndex component) {
            dst = new IJointAnimKey[component.Count];
            if (component.Count <= 0)
              throw new Exception("Count <= 0");
            if (component.Count == 1) {
              dst[0] = new Key(0, src[component.Index]);
            } else {
              for (var index = 0; index < component.Count; ++index)
                dst[index] =
                    new Key(index, src[component.Index + index]);
            }
          }

        private void SetKeysR_(
            out IJointAnimKey[] dst,
            short[] src,
            float rotScale,
            AnimComponent.AnimIndex component) {
            dst = new IJointAnimKey[component.Count];
            if (component.Count <= 0)
              throw new Exception("Count <= 0");
            if (component.Count == 1) {
              dst[0] =
                  new Key(0, src[component.Index] * rotScale);
            } else {
              for (var index = 0; index < component.Count; ++index)
                dst[index] =
                    new Key(index, src[component.Index + index] *
                            rotScale);
            }
          }

        private sealed class Key : IJointAnimKey {
          public Key(int frame, float value) {
              this.Frame = frame;
              this.Value = value;
            }

          public int Frame { get; }
          public float Value { get; }
        }
      }
    }
  }
}