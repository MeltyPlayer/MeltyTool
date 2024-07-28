// Decompiled with JetBrains decompiler
// Type: MKDS_Course_Modifier.GCN.BCK
// Assembly: MKDS Course Modifier, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DAEF8B62-698B-42D0-BEDD-3770EB8C9FE8
// Assembly location: R:\Documents\CSharpWorkspace\Pikmin2Utility\MKDS Course Modifier\MKDS Course Modifier.exe

using System;
using System.IO;
using System.Linq;

using fin.util.asserts;

using jsystem.G3D_Binary_File_Format;

using schema.binary;
using schema.binary.attributes;

namespace jsystem.schema.j3dgraph.bcx;

/// <summary>
///   BCK files define joint animations with sparse keyframes.
///
///   https://wiki.cloudmodding.com/tww/BCK
/// </summary>
[Endianness(Endianness.BigEndian)]
public partial class Bck : IBcx {
  public BckHeader Header;
  public ANK1Section ANK1;

  public Bck(byte[] file) {
      using var br =
          new SchemaBinaryReader((Stream)new MemoryStream(file),
                                 Endianness.BigEndian);
      this.Header = br.ReadNew<BckHeader>();
      this.ANK1 = new ANK1Section(br, out _);
    }

  public IAnx1 Anx1 => this.ANK1;

  [BinarySchema]
  public partial class BckHeader : IBinaryConvertible {
    private readonly string magic_ = "J3D1bck1";

    [WSizeOfStreamInBytes]
    private uint fileSize_;

    private readonly uint sectionCount_ = 1;

    [SequenceLengthSource(16)]
    private byte[] padding_;
  }

  public enum LoopMode : byte {
    ONCE = 0,
    ONCE_AND_RESET = 1,
    LOOP = 2,
    MIRRORED_ONCE = 3,
    MIRRORED_LOOP = 4,
  }

  public partial class ANK1Section : IAnx1 {
    public const string Signature = "ANK1";
    public DataBlockHeader Header;
    public LoopMode LoopMode;
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

    public ANK1Section(IBinaryReader br, out bool OK) {
        bool OK1;
        this.Header = new DataBlockHeader(br, "ANK1", out OK1);
        if (!OK1) {
          OK = false;
        } else {
          this.LoopMode = (LoopMode) br.ReadByte();
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
          var rotScale = MathF.Pow(2, this.AngleMultiplier - 15) * MathF.PI;
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
          for (var i = 0; i < this.axes.Length; ++i) {
            this.axes[i] = br.ReadNew<AnimComponent>();
          }
        }

      public IJointAnim Values { get; private set; }

      public void SetValues(
          float[] Scales,
          short[] Rotations,
          float[] Translations,
          float RotScale) {
          this.Values =
              new JointAnim(
                  this,
                  Scales,
                  Rotations,
                  Translations,
                  RotScale);
        }

      [BinarySchema]
      public partial class AnimComponent : IBinaryConvertible {
        public AnimIndex S { get; } = new();
        public AnimIndex R { get; } = new();
        public AnimIndex T { get; } = new();
      }

      [BinarySchema]
      public partial class AnimIndex : IBinaryConvertible {
        public ushort Count;
        public ushort Index;
        public ushort TangentMode;
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
            AnimIndex component) {
            dst = new IJointAnimKey[component.Count];
            if (component.Count <= 0)
              throw new Exception("Count <= 0");
            if (component.Count == 1) {
              dst[0] =
                  new Key(
                      0,
                      src[component.Index],
                      0,
                      0);
            } else {
              var tangentMode = component.TangentMode;
              var hasTwoTangents = tangentMode == 1;
              Asserts.True(tangentMode == 0 || tangentMode == 1);

              var stride = hasTwoTangents ? 4 : 3;
              for (var index = 0; index < component.Count; ++index) {
                var i = component.Index + stride * index;

                var time = (int) src[i + 0];
                var value = src[i + 1];

                float incomingTangent, outgoingTangent;
                if (hasTwoTangents) {
                  incomingTangent = src[i + 2];
                  outgoingTangent = src[i + 3];
                } else {
                  incomingTangent = outgoingTangent = src[i + 2];
                }

                dst[index] =
                    new Key(
                        time,
                        value,
                        incomingTangent,
                        outgoingTangent);
              }
            }
          }

        private void SetKeysR_(
            out IJointAnimKey[] dst,
            short[] src,
            float rotScale,
            AnimIndex component) {
            dst =
                new IJointAnimKey[component.Count];
            if (component.Count <= 0)
              throw new Exception("Count <= 0");
            if (component.Count == 1) {
              dst[0] = new Key(
                  0,
                  src[component.Index] * rotScale,
                  0,
                  0);
            } else {
              var tangentMode = component.TangentMode;
              var hasTwoTangents = tangentMode == 1;
              Asserts.True(tangentMode == 0 || tangentMode == 1);

              var stride = hasTwoTangents ? 4 : 3;
              for (var index = 0; index < component.Count; ++index) {
                var i = component.Index + stride * index;

                var time = src[i + 0];
                var value = src[i + 1] * rotScale;

                float incomingTangent, outgoingTangent;
                if (hasTwoTangents) {
                  incomingTangent = src[i + 2] * rotScale;
                  outgoingTangent = src[i + 3] * rotScale;
                } else {
                  incomingTangent = outgoingTangent = src[i + 2] * rotScale;
                }

                dst[index] =
                    new Key(
                        time,
                        value,
                        incomingTangent,
                        outgoingTangent);
              }
            }
          }

        public class Key(
            int frame,
            float value,
            float incomingTangent,
            float outgoingTangent)
            : IJointAnimKey {
          public int Frame { get; } = frame;
          public float Value { get; } = value;
          public float IncomingTangent { get; } = incomingTangent;
          public float OutgoingTangent { get; } = outgoingTangent;
        }
      }
    }
  }
}