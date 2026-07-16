using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.io;

using fin.animation.keyframes;
using fin.io;
using fin.math.matrix.four;
using fin.model;

using marioartist.schema.talent_studio;

using schema.binary;

namespace marioartist.api;

using BoneTuple = (IReadOnlyBone bone, Joint joint, int jointIndex);

public static class MocapAnimationsUtil {
  public static void TryToAddAnimations(
      IModel finModel,
      BoneTuple[] finBonesAndJoints,
      IReadOnlyTreeDirectory? animationsDirectory) {
    if (animationsDirectory == null) {
      return;
    }

    var mocapAnimationFiles = animationsDirectory
                              .AssertGetExistingSubdir("mocap")
                              .GetExistingFiles();

    foreach (var mocapAnimationFile in mocapAnimationFiles) {
      int[] animationOffsets = mocapAnimationFile.NameWithoutExtension switch {
          "00A1F3B8" => [0x12e44, 0x15ce4, 0x187c4, 0x1b3bc],
          _          => [],
      };

      using var br = mocapAnimationFile.OpenReadAsBinary(Endianness.BigEndian);

      var bones = new Dictionary<JointIndex, IReadOnlyBone>();
      foreach (var (bone, _, index) in finBonesAndJoints) {
        bones[(JointIndex) index] = bone;
      }

      foreach (var animationFileAddress in animationOffsets) {
        br.Position = animationFileAddress;

        var unkFloats = br.ReadSingles(8);
        var frameCount = br.ReadUInt32();

        var expectedAnimationSegmentedAddress
            = (0x04 << 24) | animationFileAddress;
        br.AssertInt32(expectedAnimationSegmentedAddress);

        var unkCounts = br.ReadUInt32s(14);

        br.AssertUInt16((ushort) frameCount);

        var jointCount = 0x3d;
        br.AssertUInt16((ushort) jointCount);

        var weirdScalesSegmentedAddress = br.ReadUInt32();
        var weirdIndices0SegmentedAddress = br.ReadUInt32();
        var unkShorts0SegmentedAddress = br.ReadUInt32();
        var weirdIndices1SegmentedAddress = br.ReadUInt32();
        var unkShorts1SegmentedAddress = br.ReadUInt32();
        var weirdIndices2SegmentedAddress = br.ReadUInt32();

        var weirdScaleSize
            = unkShorts0SegmentedAddress - weirdScalesSegmentedAddress;
        var unkShorts0Size
            = unkShorts1SegmentedAddress - unkShorts0SegmentedAddress;
        var unkShorts1Size
            = weirdIndices0SegmentedAddress - unkShorts1SegmentedAddress;
        var weirdIndices0Size = weirdIndices1SegmentedAddress -
                                weirdIndices0SegmentedAddress;
        var weirdIndices1Size = weirdIndices2SegmentedAddress -
                                weirdIndices1SegmentedAddress;
        var weirdIndices2Size = expectedAnimationSegmentedAddress -
                                weirdIndices2SegmentedAddress;

        var actualScaleValueCount = weirdScaleSize / 4;
        var actualRotationValueCount = unkShorts0Size / 2;
        var actualUnkShort1Count = unkShorts1Size / 2;
        var actualJointMocapScaleCount = weirdIndices0Size / 24;
        var actualJointMocapRotationCount = weirdIndices1Size / 24;
        var actualJointMocapData2Count = weirdIndices2Size / 24;

        Guard.IsEqualTo(actualScaleValueCount, 19);
        Guard.IsEqualTo(actualJointMocapScaleCount, jointCount);
        Guard.IsEqualTo(actualJointMocapRotationCount, jointCount);
        Guard.IsEqualTo(actualJointMocapData2Count, jointCount);

        IoUtils.SplitSegmentedAddress(
            weirdScalesSegmentedAddress,
            out _,
            out var weirdScalesFileAddress);

        br.Position = weirdScalesFileAddress;

        var scaleValues = br.ReadSingles(actualScaleValueCount);
        var rotationValues = br.ReadInt16s(actualRotationValueCount);
        var values2 = br.ReadInt16s(actualUnkShort1Count);

        var jointMocapScales
            = br.ReadNews<JointMocapData>((int) actualJointMocapScaleCount);
        var jointMocapRotations
            = br.ReadNews<JointMocapData>((int) actualJointMocapRotationCount);
        var jointMocapData2
            = br.ReadNews<JointMocapData>((int) actualJointMocapData2Count);

        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = $"{mocapAnimationFile.NameWithoutExtension}_{animationFileAddress.ToHexString()}";
        finAnimation.FrameCount = (int) frameCount;
        finAnimation.FrameRate = 30;

        for (var jointI = 0; jointI < jointCount; ++jointI) {
          // From the decomp, 0x8010e9c8
          IReadOnlyBone? finBone = jointI switch {
              0 => finModel.Skeleton.Root,
              6 => bones[JointIndex.BODY_ROOT],

              // 7 and 8, and 10?
              8 => bones[JointIndex.HIP],

              // 9 is just hip, without affecting children?

              // 11 and 12?
              11  => bones[JointIndex.UPPER_LEG_1],
              // 13 is just self without affecting children

              14  => bones[JointIndex.LOWER_LEG_1],
              // 15 is just self without affecting children

              // 16 and 17 and 18?
              16  => bones[JointIndex.FOOT_1],

              19  => bones[JointIndex.UPPER_LEG_0],

              _    => null,
          };

          if (finBone == null) {
            continue;
          }

          var boneTracks = finAnimation.GetOrCreateBoneTracks(finBone);

          var rotations = boneTracks.UseCombinedQuaternionKeyframes();

          var jointMocapRotation = jointMocapRotations[jointI];

          // TODO: Optimize this so each channel is only as long as needed
          for (var f = 0; f < frameCount; ++f) {
            // Based on decomp, at 0x80118fa4
            var xRotationShort = f < jointMocapRotation.XFrameCount
                ? rotationValues[jointMocapRotation.XOffset + f]
                : rotationValues[jointMocapRotation.XOffset +
                                 jointMocapRotation.XFrameCount -
                                 1];
            var yRotationShort = f < jointMocapRotation.YFrameCount
                ? rotationValues[jointMocapRotation.YOffset + f]
                : rotationValues[jointMocapRotation.YOffset +
                                 jointMocapRotation.YFrameCount -
                                 1];
            var zRotationShort = f < jointMocapRotation.ZFrameCount
                ? rotationValues[jointMocapRotation.ZOffset + f]
                : rotationValues[jointMocapRotation.ZOffset +
                                 jointMocapRotation.ZFrameCount -
                                 1];

            var axis0 = ConvertShortToRadians_(xRotationShort);
            var axis1 = ConvertShortToRadians_(yRotationShort);
            var axis2 = ConvertShortToRadians_(zRotationShort);

            rotations.SetKeyframe(
                f,
                ConvertRadiansToQuaternion_(
                    axis0,
                    axis1,
                    axis2,
                    finBone.Transform.LocalRotation ?? Quaternion.Identity));
          }
        }
      }
    }
  }

  private static float ConvertShortToRadians_(short value) {
    var step1 = (value << 0xc);
    var iVar1 = (((int) value) << 0xc) >> 0x10;
    if (iVar1 < 0) {
      iVar1 = iVar1 + 0x1000;
    }

    return 2 * iVar1 / 4096.0f * MathF.PI;
  }

  private static Quaternion ConvertRadiansToQuaternion_(
      float xRadians,
      float yRadians,
      float zRadians,
      Quaternion rootPose) {
    // From decomp: 0x80117df4

    var (fVar3, fVar4) = MathF.SinCos(xRadians);
    var (fVar5, fVar6) = MathF.SinCos(yRadians);
    var (fVar7, fVar8) = MathF.SinCos(zRadians);

    var matrix = new Matrix4x4 {
        [2, 0] = -fVar5,
        [0, 0] = fVar6 * fVar8,
        [1, 0] = fVar6 * fVar7,
        [0, 1] = fVar3 * fVar5 * fVar8 - fVar4 * fVar7,
        [1, 1] = fVar3 * fVar5 * fVar7 + fVar4 * fVar8,
        [2, 1] = fVar3 * fVar6,
        [0, 2] = fVar4 * fVar5 * fVar8 + fVar3 * fVar7,
        [2, 2] = fVar4 * fVar6,
        [1, 2] = fVar4 * fVar5 * fVar7 - fVar3 * fVar8,
        [3, 3] = 1f
    };

    matrix.AssertDecompose(out _, out var rotation, out _);

    return rotation;
  }
}

[BinarySchema]
public partial struct JointMocapData : IBinaryConvertible {
  public uint XFrameCount { get; set; }
  public uint XOffset { get; set; }

  public uint YFrameCount { get; set; }
  public uint YOffset { get; set; }

  public uint ZFrameCount { get; set; }
  public uint ZOffset { get; set; }
}