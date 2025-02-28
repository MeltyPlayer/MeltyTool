using System.Collections;

using fin.animation.keyframes;
using fin.data.queues;
using fin.io;
using fin.model;

using schema.binary;


namespace visceral.api;

public class BnkReader {
  public enum MaybeBoneType {
    ROOT = 0x2,
    PARENT = 0x5,
    ANIMATED = 0x16,
  }

  public enum AxisType : byte {
    ROT_X,
    ROT_Y,
    ROT_Z,
    ROT_W,
    POS_X,
    POS_Y,
    POS_Z,
    SCALE_X,
    SCALE_Y,
    SCALE_Z,
  }

  public enum KeyframeType : byte {
    ONLY_KEYFRAME = 0x0,
    CUBIC_SPLINE_COEFFICIENTS_3_TO_4 = 0x1,
    CUBIC_SPLINE_COEFFICIENTS_2_TO_4 = 0x2,
    CUBIC_SPLINE_COEFFICIENTS_1_TO_4 = 0x3,
    FLOATS = 0x4,
    BYTE_GRADIENT = 0x6,
    SHORT_GRADIENT = 0x7,
    SINGLETON_0 = 0xC,
    SINGLETON_1 = 0xD,
  }

  public void ReadBnk(IModel model,
                      IReadOnlyGenericFile bnkFile,
                      IReadOnlyGenericFile? rcbFile,
                      IBone[] bones) {
    using var bnkBr =
        new SchemaBinaryReader(bnkFile.OpenRead(), Endianness.LittleEndian);

    bnkBr.Position = 0x24;
    var headerCount = bnkBr.ReadUInt32();
    var headerOffsetOffset = bnkBr.ReadUInt32();

    bnkBr.Position = headerOffsetOffset;
    var headerOffsets = bnkBr.ReadUInt32s(headerCount);

    foreach (var headerOffset in headerOffsets) {
      var finAnimation = model.AnimationManager.AddAnimation();
      finAnimation.FrameRate = 30;
      finAnimation.UseLoopingInterpolation = false;

      bnkBr.Position = headerOffset;

      if (GeoModelImporter.STRICT_DAT) {
        this.ReadIntoAnimation_(bnkBr, rcbFile, bones, finAnimation);
      } else {
        try {
          this.ReadIntoAnimation_(bnkBr, rcbFile, bones, finAnimation);
        } catch (Exception e) {
          finAnimation.Name = $"* {finAnimation.Name}";
        }
      }
    }
  }

  private void ReadIntoAnimation_(IBinaryReader bnkBr,
                                  IReadOnlyGenericFile? rcbFile,
                                  IBone[] bones,
                                  IModelAnimation finAnimation) {
    {
      var animationNameOffset = bnkBr.ReadUInt32();
      finAnimation.Name =
          bnkBr.SubreadAt(animationNameOffset, () => bnkBr.ReadStringNT());
    }

    {
      var rootOffset = bnkBr.ReadUInt32();
      var maybeBoneOffsetQueue = new FinQueue<uint>(rootOffset);
      while (maybeBoneOffsetQueue.TryDequeue(out var maybeBoneOffset)) {
        bnkBr.Position = maybeBoneOffset;

        var boneType = (MaybeBoneType) bnkBr.ReadByte();
        var childCount = bnkBr.ReadByte();
        var maybeId = bnkBr.ReadByte();
        var unk1 = bnkBr.ReadByte();

        var someMetadataOffset = bnkBr.ReadUInt32();
        var unk2 = bnkBr.ReadUInt32();

        if (childCount > 0) {
          maybeBoneOffsetQueue.Enqueue(bnkBr.ReadUInt32s(childCount));
        }

        if (boneType == MaybeBoneType.ANIMATED) {
          var animationDataOffset = bnkBr.ReadUInt32();
          bnkBr.Position = animationDataOffset;

          var unkHash = bnkBr.ReadUInt32();
          var maybeVersion = bnkBr.ReadUInt32();
          var isLooping = bnkBr.ReadUInt32() != 0;
          var unk4 = bnkBr.ReadUInt16();
          var extraAxisCount = bnkBr.ReadUInt16();
          var unk6 = bnkBr.ReadUInt32();

          var someHashFromRcb = bnkBr.ReadUInt32();
          var standaloneCommandPrefix = bnkBr.ReadUInt32();

          bnkBr.Position += 4;

          var animationType = bnkBr.ReadUInt16();
          var unk7 = bnkBr.ReadUInt16();

          // These are unknown
          bnkBr.Position += 4 * 3;

          using var rcbEr =
              new SchemaBinaryReader(rcbFile.OpenRead(),
                                     Endianness.LittleEndian);
          rcbEr.Position = 0x24;
          var someDefinitionsOffset = rcbEr.ReadUInt32();
          rcbEr.Position = someDefinitionsOffset;
          while (rcbEr.ReadUInt32() != someHashFromRcb) ;
          var bitMaskOffsetOffset = rcbEr.ReadUInt32();
          rcbEr.Position = bitMaskOffsetOffset;
          var bitMaskOffset = rcbEr.ReadUInt32();
          var bitMaskLength = rcbEr.ReadUInt32();

          rcbEr.Position = bitMaskOffset;
          var bitMaskArray =
              new BitArray(
                  rcbEr.ReadBytes(
                      (int) Math.Ceiling(bitMaskLength / 8f)));

          if (animationType == 1) {
            ReadAnimationType1_(finAnimation,
                                bnkBr,
                                bones,
                                bitMaskArray,
                                bitMaskLength,
                                standaloneCommandPrefix,
                                extraAxisCount);
          } else {
            
          }
        }
      }
    }
  }

  private void ReadAnimationType1_(
      IModelAnimation finAnimation,
      IBinaryReader bnkBr,
      IBone[] bones,
      BitArray bitMaskArray,
      uint bitMaskLength,
      uint standaloneCommandPrefix,
      ushort extraAxisCount) {
    var totalFrames = 1;

    for (var b = 0; b < bitMaskLength; ++b) {
      var isActive = bitMaskArray[b];
      if (!isActive) {
        continue;
      }

      var boneTracks = finAnimation.GetOrCreateBoneTracks(bones[b]);
      var rotations = boneTracks.UseSeparateQuaternionKeyframes();
      var translations = boneTracks.UseSeparateTranslationKeyframes();
      var scales = boneTracks.UseSeparateScaleKeyframes();

      void ReadAxis(AxisType axisType) {
        void SetKeyframe(int frame, float value) {
          totalFrames = Math.Max(totalFrames, frame);

          switch (axisType) {
            case AxisType.ROT_X:
            case AxisType.ROT_Y:
            case AxisType.ROT_Z:
            case AxisType.ROT_W: {
              rotations.Axes[axisType - AxisType.ROT_X]
                       .SetKeyframe(frame, value);
              break;
            }
            case AxisType.POS_X:
            case AxisType.POS_Y:
            case AxisType.POS_Z: {
              translations.Axes[axisType - AxisType.POS_X]
                          .SetKeyframe(frame, value);
              break;
            }
            case AxisType.SCALE_X:
            case AxisType.SCALE_Y:
            case AxisType.SCALE_Z: {
              scales.Axes[axisType - AxisType.SCALE_X]
                    .SetKeyframe(frame, value);
              break;
            }
          }
        }

        var command = bnkBr.ReadUInt16();
        var upper = command >> 4;
        var lower = command & 0xF;

        if (upper == standaloneCommandPrefix) {
          var keyframeType = (KeyframeType) lower;
          bnkBr.Position -= 1;

          var frame = 0;
          foreach (var keyframeValue in
                   this.ReadKeyframeValuesOfType_(
                       bnkBr,
                       keyframeType,
                       (int) standaloneCommandPrefix)) {
            SetKeyframe(frame++, keyframeValue);
          }
        } else if (lower == 5) {
          var keyframeCount = upper;

          var frame = 0;
          for (var k = 0; k < keyframeCount; ++k) {
            var lengthAndKeyframeType = bnkBr.ReadUInt16();

            var keyframeLength = lengthAndKeyframeType >> 4;
            var keyframeType =
                (KeyframeType) (lengthAndKeyframeType & 0xF);

            bnkBr.Position -= 1;

            var startingKeyframe = frame;
            foreach (var keyframeValue in
                     this.ReadKeyframeValuesOfType_(
                         bnkBr,
                         keyframeType,
                         keyframeLength)) {
              SetKeyframe(frame++, keyframeValue);
            }

            frame = startingKeyframe + keyframeLength;
            totalFrames = Math.Max(totalFrames, frame);
          }
        } else {
          throw new NotImplementedException();
        }
      }

      if (b > 0) {
        for (var a = 7; a < 7 + extraAxisCount; ++a) {
          ReadAxis((AxisType) a);
        }
      }

      for (var a = 0; a < 7; ++a) {
        ReadAxis((AxisType) a);
      }
    }

    finAnimation.FrameCount = totalFrames;
  }

  private float[] values_ = new float[4];

  private IEnumerable<float> ReadKeyframeValuesOfType_(
      IBinaryReader br,
      KeyframeType keyframeType,
      int keyframeLength) {
    switch (keyframeType) {
      case KeyframeType.SINGLETON_0: {
        br.Position += 1;
        yield return 0;
        break;
      }
      case KeyframeType.SINGLETON_1: {
        br.Position += 1;
        yield return 1;
        break;
      }
      case KeyframeType.ONLY_KEYFRAME:
      case KeyframeType.CUBIC_SPLINE_COEFFICIENTS_3_TO_4:
      case KeyframeType.CUBIC_SPLINE_COEFFICIENTS_2_TO_4:
      case KeyframeType.CUBIC_SPLINE_COEFFICIENTS_1_TO_4: {
        var totalValueCount = 1 + (int) keyframeType;
        this.values_.AsSpan().Fill(0);
        for (var i = 0; i < totalValueCount; ++i) {
          if (i > 0) {
            br.Position -= 1;
          }

          this.values_[4 - totalValueCount + i] = br.ReadSingle();
        }

        if (keyframeType == KeyframeType.ONLY_KEYFRAME) {
          yield return this.values_[3];
        } else {
          // TODO: Support this as a keyframe type, rather than
          // precalculating them here.
          for (var f = 0; f < keyframeLength; ++f) {
            yield return ((this.values_[0] * f + this.values_[1]) * f +
                          this.values_[2]) * f + this.values_[3];
          }
        }

        break;
      }
      case KeyframeType.FLOATS: {
        br.Position += 1;

        for (var i = 0; i < keyframeLength; ++i) {
          yield return br.ReadSingle();
        }

        break;
      }
      case KeyframeType.BYTE_GRADIENT:
      case KeyframeType.SHORT_GRADIENT: {
        var bias = br.ReadSingle();
        br.Position -= 1;

        // The scale starts with the final byte from the bias. This was
        // absolutely the hardest thing to figure out with this format,
        // I struggled with this forever lol.
        var scale = br.ReadSingle();

        var values =
            (keyframeType == KeyframeType.BYTE_GRADIENT
                ? br.ReadBytes(keyframeLength)
                    .Select(u => (ushort) u)
                : br.ReadUInt16s(keyframeLength)
                    .Select(u => u))
            .Select(u => u * scale + bias);

        foreach (var keyframe in values) {
          yield return keyframe;
        }

        break;
      }
      default: throw new NotImplementedException();
    }
  }
}