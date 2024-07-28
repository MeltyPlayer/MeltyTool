using System;
using System.Collections.Generic;

using CommunityToolkit.HighPerformance;

using f3dzex2.io;

using fin.schema;

using UoT.memory;

#pragma warning disable CS8603


namespace UoT.model {
  public class AnimationReader2 {
    /// <summary>
    ///   Parses a set of animations according to the spec at:
    ///   https://wiki.cloudmodding.com/oot/Animation_Format#Normal_Animations
    /// </summary>
    // TODO: Some jank still slips through, is there a proper list of these
    // addresses somewhere in the file?
    public IList<IAnimation>? GetCommonAnimations(
        IN64Memory n64Memory,
        IReadOnlyList<IZFile> animationFiles,
        int limbCount) {
      uint trackCount = (uint) (limbCount * 3);
      var animations = new List<IAnimation>();

      foreach (var animationFile in animationFiles) {
        using var entryEr = n64Memory.OpenSegment(animationFile.Segment);

        // Guesstimating the index by looking for an spot where the header's angle
        // address and track address have the same bank as the param at the top.
        for (var i = 0; i < entryEr.Length - 16; ++i) {
          entryEr.Position = i;

          var frameCount = entryEr.ReadUInt16();
          var pad0 = entryEr.ReadUInt16();
          var rotationValuesAddress = entryEr.ReadUInt32();
          var rotationIndicesAddress = entryEr.ReadUInt32();
          var limit = entryEr.ReadUInt16();
          var pad1 = entryEr.ReadUInt16();


          IoUtils.SplitSegmentedAddress(rotationValuesAddress, out var rotationValueSegment, out var rotationValueOffset);
          IoUtils.SplitSegmentedAddress(rotationIndicesAddress, out var rotationIndicesSegment, out var rotationIndicesOffset);

          if (pad0 != 0 || pad1 != 0) {
            continue;
          }

          if (rotationValueSegment == 6 && rotationIndicesSegment == 6) {
            ;
          }
          
          // Verifies the frame count is positive.
          if (frameCount == 0) {
            continue;
          }

          if (frameCount < 100) {
           ;
          }

          if (!n64Memory.IsValidSegmentedAddress(rotationValuesAddress)) {
            continue;
          }

          // Verifies the rotation indices address has a valid bank.
          if (!n64Memory.IsValidSegmentedAddress(rotationIndicesAddress)) {
            continue;
          }

          // Obtains the specified banks.
          using var rotationValuesEr =
              n64Memory.OpenAtSegmentedAddress(rotationValuesAddress);
          using var rotationIndicesEr =
              n64Memory.OpenAtSegmentedAddress(rotationIndicesAddress);
          var originalRotationIndicesOffset = rotationIndicesEr.Position;

          // Angle count should be greater than 0.
          var angleCount =
              (rotationIndicesEr.Position - rotationValuesEr.Position) / 2L;
          if (angleCount <= 0) {
            continue;
          }

          // All values of "tTrack" should be within the bounds of .Angles.
          var validTTracks = true;
          for (var i1 = 0; i1 < 3 + (trackCount + 1); i1++) {
            var tTrack = rotationIndicesEr.ReadUInt16();
            if (tTrack < limit) {
              if (tTrack >= angleCount) {
                validTTracks = false;
                goto badTTracks;
              }
            } else if ((uint) (tTrack + frameCount) > angleCount) {
              validTTracks = false;
              goto badTTracks;
            }
          }

          badTTracks:
          if (!validTTracks) {
            continue;
          }

          var animation = new NormalAnimation {
              FrameCount = frameCount,
              TrackOffset = (uint) originalRotationIndicesOffset,
              AngleCount = (uint) angleCount
          };

          animation.Angles = rotationValuesEr.ReadUInt16s(animation.AngleCount);

          // Translation is at the start.
          rotationIndicesEr.Position = originalRotationIndicesOffset;
          var xList =
              ReadFrames_(
                  rotationIndicesEr.ReadUInt16(),
                  limit,
                  animation);
          var yList =
              ReadFrames_(
                  rotationIndicesEr.ReadUInt16(),
                  limit,
                  animation);
          var zList =
              ReadFrames_(
                  rotationIndicesEr.ReadUInt16(),
                  limit,
                  animation);

          animation.Positions = new Vec3s[animation.FrameCount];
          for (var pi = 0; pi < animation.FrameCount; ++pi) {
            animation.Positions[pi] = new Vec3s {
                X = this.ConvertUShortToShort_(xList[Math.Min(pi, xList.Length - 1)]),
                Y = this.ConvertUShortToShort_(yList[Math.Min(pi, yList.Length - 1)]),
                Z = this.ConvertUShortToShort_(zList[Math.Min(pi, zList.Length - 1)]),
            };
          }

          animation.Tracks = new NormalAnimationTrack[trackCount];

          for (var i1 = 0; i1 < trackCount; ++i1) {
            var track = animation.Tracks[i1] = new NormalAnimationTrack();
            track.Frames =
                ReadFrames_(rotationIndicesEr.ReadUInt16(), limit, animation);
          }

          animations.Add(animation);
        }
      }

      return animations.Count > 0 ? animations : null;
    }

    private short ConvertUShortToShort_(ushort value) {
      Span<ushort> ptr = stackalloc ushort[1];
      ptr[0] = value;
      return ptr.Cast<ushort, short>()[0];
    }

    private static ushort[] ReadFrames_(
        ushort tTrack,
        ushort limit,
        NormalAnimation animation) {
      ushort[] frames;

      // Constant
      if (tTrack < limit) {
        frames = new ushort[1];
        frames[0] = animation.Angles[tTrack];
      } else {
        // Keyframes
        frames = new ushort[animation.FrameCount];
        for (var i2 = 0; i2 < animation.FrameCount; ++i2) {
          try {
            frames[i2] = animation.Angles[tTrack + i2];
          } catch {
            return null;
          }
        }
      }

      return frames;
    }

    /// <summary>
    ///   Parses a set of animations according to the spec at:
    ///   https://wiki.cloudmodding.com/oot/Animation_Format#C_code
    /// </summary>
    [Unknown]
    public IList<IAnimation>? GetLinkAnimations(
        IN64Memory n64Memory,
        IZFile headerFile,
        int limbCount) {
      var animations = new List<IAnimation>();

      using var headerEr = n64Memory.OpenSegment(headerFile.Segment);

      var trackCount = (uint) (limbCount * 3);
      var frameSize = 2 * (3 + trackCount) + 2;
      for (uint i = 0x2310; i <= 0x34F8; i += 4) {
        headerEr.Position = i;

        var frameCount = headerEr.ReadUInt16();
        var pad0 = headerEr.ReadUInt16();
        var animationAddress = headerEr.ReadUInt32();

        if (pad0 != 0) {
          continue;
        }

        // Verifies the frame count is positive.
        if (frameCount == 0) {
          continue;
        }

        if (!n64Memory.IsValidSegmentedAddress(animationAddress)) {
          continue;
        }

        // Everything looks good with this animation location!

        // Starts parsing animation from this spot.
        var tracks = new LinkAnimetionTrack[(int) (trackCount - 1L + 1)];
        var positions = new Vec3s[frameCount];
        var facialStates = new FacialState[frameCount];

        for (int t = 0, loopTo = (int) (trackCount - 1L);
             t <= loopTo;
             t++) {
          tracks[t] = new LinkAnimetionTrack(1, new ushort[frameCount]);
        }

        using var animationEr = n64Memory.OpenAtSegmentedAddress(animationAddress);
        var originalAnimationOffset = animationEr.Position;
        for (int f = 0; f < frameCount; f++) {
          var frameOffset = animationEr.Position = (uint) (originalAnimationOffset + f * frameSize);

          positions[f] = new Vec3s {
              X = animationEr.ReadInt16(),
              Y = animationEr.ReadInt16(),
              Z = animationEr.ReadInt16(),
          };
          for (int t = 0, loopTo2 = (int) (trackCount - 1L);
               t <= loopTo2;
               t++) {
            animationEr.Position = (uint) (frameOffset + 2 * (3 + t));
            tracks[t].Frames[f] = animationEr.ReadUInt16();
          }

          animationEr.Position =
              (int) (frameOffset + 2 * (3 + trackCount));
          var unk = animationEr.ReadByte();
          var facialState = animationEr.ReadByte();
          var mouthState = IoUtil.ShiftR(facialState, 4, 4);
          var eyeState = IoUtil.ShiftR(facialState, 0, 4);

          facialStates[f] = new FacialState((EyeState) eyeState,
                                            (MouthState) mouthState);
        }

        var animation =
            new LinkAnimetion(frameCount, tracks, positions, facialStates);
        animations.Add(animation);
      }

      return animations.Count > 0 ? animations : null;
    }
  }
}