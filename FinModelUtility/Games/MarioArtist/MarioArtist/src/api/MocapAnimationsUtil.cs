using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.io;

using fin.animation.keyframes;
using fin.animation.types.quaternion;
using fin.animation.types.vector3;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;

using marioartist.schema.talent_studio;

using schema.binary;

namespace marioartist.api;

using BoneTuple = (IReadOnlyBone bone, Joint joint, int jointIndex);

public static class MocapAnimationsUtil {
  // From decomp, at 0x801978ec
  private static readonly short[] jointIndexByMocapIndexTrue_ = [
      -1, -1, -1, -1,
      -1, -1, -1, -1,
      -1, 0x12, -1, -1,
      -1, 0x1D, -1, 0x1E,
      -1, -1, -1, 0x1F,
      -1, -1, -1, 0x16,
      -1, 0x17, -1, -1,
      -1, 0x18, -1, -1,
      -1, 0x11, -1, -1,
      -1, 0xF, -1, -1,
      -1, 0x1A, -1, 0x1B,
      -1, -1, -1, 0x1C,
      -1, -1, -1, 0x13,
      -1, 0x14, -1, -1,
      -1, 0x15, -1, -1,
      -1,
  ];

  private static readonly JointIndex?[] jointIndexByMocapIndex_ = [
      null, null, null, null,
      null, null, null, null,
      null, JointIndex.UPPER_LEG_0, null, null,
      null, JointIndex.HAND_1, null, (JointIndex) 0x1E,
      null, null, null, (JointIndex) 0x1F,
      null, null, null, JointIndex.FOOT_0,
      null, JointIndex.FOOT_1, null, null,
      null, JointIndex.UPPER_ARM_0, null, null,
      null, JointIndex.HIP, null, null,
      null, JointIndex.NECK, null, null,
      null, JointIndex.FOREARM_0, null, JointIndex.FOREARM_1,
      null, null, null, JointIndex.HAND_0,
      null, null, null, JointIndex.UPPER_LEG_1,
      null, JointIndex.LOWER_LEG_0, null, null,
      null, JointIndex.LOWER_LEG_1, null, null,
      null,
  ];

  // From decomp, at 0x8022eef0
  private static readonly (uint nextSibling, uint firstChild)[]
      nextSiblingAndFirstChild_ = [
          // 0
          (0, 0),
          (0x0200fbd0, 0),
          (0, 0),
          (0x0200fc60, 0),
          (0, 0x0200fca8),
          // 5
          (0, 0x0200fcf0),
          (0, 0x0200fd38),
          (0x0200fd80, 0),
          (0, 0x0200fdc8),
          (0x0200fe10, 0),
          // 10
          (0, 0x0200fe58),
          (0, 0x0200fea0),
          (0, 0),
          (0x0200ff30, 0),
          (0, 0x0200ff78),
          // 15
          (0, 0x0200ffc0),
          (0, 0x02010008),
          (0x02010050, 0),
          (0, 0x02010098),
          (0x020100e0, 0),
          // 20
          (0, 0x02010128),
          (0x0200fee8, 0x02010170),
          (0, 0),
          (0x02010200, 0),
          (0, 0x02010248),
          // 25
          (0x020101b8, 0x02010290),
          (0, 0x020102d8),
          (0x02010320, 0),
          (0, 0x02010368),
          (0, 0x020103b0),
          // 30
          (0, 0),
          (0x02010440, 0),
          (0, 0x02010488),
          (0, 0x020104d0),
          (0, 0x02010518),
          // 35
          (0x02010560, 0),
          (0, 0x020105a8),
          (0x020105f0, 0),
          (0, 0x02010638),
          (0, 0x02010680),
          // 40
          (0, 0),
          (0x02010710, 0),
          (0, 0x02010758),
          (0, 0x020107a0),
          (0, 0x020107e8),
          // 45
          (0x02010830, 0),
          (0, 0x02010878),
          (0x020108c0, 0),
          (0, 0x02010908),
          (0x020106c8, 0x02010950),
          // 50
          (0, 0x02010998),
          (0x020109e0, 0),
          (0, 0x02010a28),
          (0x020103f8, 0x02010a70),
          (0x0200fc18, 0x02010ab8),
          // 55
          (0x02010b00, 0),
          (0x02010b48, 0),
          (0x02010b90, 0),
          (0x02010bd8, 0),
          (0x02010c20, 0),
          // 60
          (0, 0x02010c68)
      ];

  public static void TryToAddAnimations(
      IModel finModel,
      BoneTuple[] finBonesAndJoints,
      IReadOnlyTreeDirectory? animationsDirectory) {
    if (animationsDirectory == null) {
      return;
    }

    //finModel.AnimationManager.AddAnimation();

    var mocapBones = new IReadOnlyBone[0x3d];
    {
      var mocapAnimationIndex = 0;
      PopulateMocapBonesRecursively_(
          60,
          ref mocapAnimationIndex,
          finModel.Skeleton.Root,
          mocapBones);
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

        var lazyBoneTracks = new LazyDictionary<JointIndex, 
            ICombinedQuaternionKeyframes<Keyframe<Quaternion>>>(jointId
              => {
            var boneTracks = finAnimation.GetOrCreateBoneTracks(bones[jointId]);
            return boneTracks.UseCombinedQuaternionKeyframes();
          });

        var lazyMocapBoneTracks = new LazyDictionary<int, 
            (ICombinedVector3Keyframes<Keyframe<Vector3>>,
            ICombinedQuaternionKeyframes<Keyframe<Quaternion>>)>(mocapIndex
            => {
          var mocapBoneTracks = finAnimation.GetOrCreateBoneTracks(mocapBones[mocapIndex]);
          return (mocapBoneTracks.UseCombinedTranslationKeyframes(),
                  mocapBoneTracks.UseCombinedQuaternionKeyframes());
        });

        for (var f = 0; f < frameCount; ++f) {
          var globalMatrixByBone = new Dictionary<IReadOnlyBone, Matrix4x4>();

          var mocapJointAnimationIndex = 0;
          PopulateGlobalMatricesRecursively_(
              60,
              ref mocapJointAnimationIndex,
              (uint) animationFileAddress,
              f,
              Matrix4x4.Identity,
              rotationValues,
              jointMocapRotations,
              lazyMocapBoneTracks,
              globalMatrixByBone,
              bones);

          foreach (var (jointId, finBone) in bones) {
            if (!globalMatrixByBone.TryGetValue(finBone, out var matrix)) {
              continue;
            }

            if (finBone.Parent != null) {
              if (!globalMatrixByBone.TryGetValue(finBone.Parent,
                                                  out var parentMatrix)) {
                parentMatrix = finBone.Parent.Transform.WorldMatrix;
              }

              matrix *= parentMatrix.AssertInvert();
            }

            switch (jointId) {
              case JointIndex.HIP: {
                matrix = Matrix4x4.CreateFromYawPitchRoll(MathF.PI, MathF.PI, 0) *
                         matrix;
                break;
              }
            }

            matrix.AssertDecompose(out _, out var rotation, out _);

            var rotationTracks = lazyBoneTracks[jointId];
            rotationTracks.SetKeyframe(f, rotation);
          }
        }
      }
    }
  }

  private static void PopulateMocapBonesRecursively_(
      int mocapJointOffset,
      ref int mocapJointAnimationIndex,
      IBone parentBone,
      Span<IReadOnlyBone> mocapBones) {
    // From decomp, at 0x80119348
    var index = 0x3d - 1 - mocapJointOffset;

    var currentBone = parentBone.AddChild(0, 0, 0);

    mocapBones[index] = currentBone;
    ++mocapJointAnimationIndex;

    var (nextSiblingSegmentedAddress, firstChildSegmentedAddress)
        = nextSiblingAndFirstChild_[mocapJointOffset];

    if (firstChildSegmentedAddress != 0) {
      var firstChildOffset
          = GetOffsetFromSegmentedAddress_(firstChildSegmentedAddress);
      PopulateMocapBonesRecursively_(
          firstChildOffset,
          ref mocapJointAnimationIndex,
          currentBone,
          mocapBones);
    }

    if (nextSiblingSegmentedAddress != 0) {
      var nextSiblingOffset
          = GetOffsetFromSegmentedAddress_(nextSiblingSegmentedAddress);
      PopulateMocapBonesRecursively_(
          nextSiblingOffset,
          ref mocapJointAnimationIndex,
          parentBone,
          mocapBones);
    }
  }

  private static void PopulateGlobalMatricesRecursively_(
      int mocapJointOffset,
      ref int mocapJointAnimationIndex,
      uint animationFileAddress,
      int f,
      Matrix4x4 matrix,
      short[] rotationValues,
      JointMocapData[] jointMocapRotations,
      ILazyDictionary<int, 
          (ICombinedVector3Keyframes<Keyframe<Vector3>>,
          ICombinedQuaternionKeyframes<Keyframe<Quaternion>>)> lazyMocapBoneTracks,
      IDictionary<IReadOnlyBone, Matrix4x4> globalMatrixByBone,
      IReadOnlyDictionary<JointIndex, IReadOnlyBone> bones) {
    var jointMocapRotation = jointMocapRotations[mocapJointAnimationIndex];

    // From the decomp, 0x8010e9c8
    JointIndex? transformJointId = mocapJointAnimationIndex switch {
        10 or 0xb or 0x15    => JointIndex.HIP,
        0xe                  => JointIndex.UPPER_LEG_1,
        0x10                 => JointIndex.LOWER_LEG_1,
        0x14                 => JointIndex.FOOT_1,
        0x18                 => JointIndex.UPPER_LEG_0,
        0x1a                 => JointIndex.LOWER_LEG_0,
        0x1e                 => JointIndex.FOOT_0,
        0x22 or 0x27 or 0x31 => JointIndex.TORSO,
        0x26                 => JointIndex.NECK,
        0x2a                 => JointIndex.UPPER_ARM_1,
        0x2c                 => JointIndex.FOREARM_1,
        0x30                 => JointIndex.HAND_1,
        0x34                 => JointIndex.UPPER_ARM_0,
        0x36                 => JointIndex.FOREARM_0,
        0x3a                 => JointIndex.HAND_0,
        _                    => null,
    };

    var translation = transformJointId != null
        ? bones[transformJointId.Value].Transform.LocalTranslation :
          Vector3.Zero;

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

    var localMatrix = ConvertRadiansToRotationMatrix_(
        xRotationShort,
        yRotationShort,
        zRotationShort);

    localMatrix.AssertDecompose(out _, out var rotation, out _);

    var (translationKeyframes, rotationKeyframes) = lazyMocapBoneTracks[mocapJointAnimationIndex];
    translationKeyframes.SetKeyframe(f, translation);
    rotationKeyframes.SetKeyframe(f, rotation);

    matrix = localMatrix * matrix;

    var jointId = jointIndexByMocapIndex_[mocapJointAnimationIndex];
    if (jointId != null && (int) jointId <= 29) {
      globalMatrixByBone[bones[jointId.Value]] = matrix;
    }

    ++mocapJointAnimationIndex;

    var (nextSiblingSegmentedAddress, firstChildSegmentedAddress)
        = nextSiblingAndFirstChild_[mocapJointOffset];

    if (firstChildSegmentedAddress != 0) {
      var firstChildOffset
          = GetOffsetFromSegmentedAddress_(firstChildSegmentedAddress);
      PopulateGlobalMatricesRecursively_(
          firstChildOffset,
          ref mocapJointAnimationIndex,
          animationFileAddress,
          f,
          matrix,
          rotationValues,
          jointMocapRotations,
          lazyMocapBoneTracks,
          globalMatrixByBone,
          bones);
    }

    if (nextSiblingSegmentedAddress != 0) {
      var nextSiblingOffset
          = GetOffsetFromSegmentedAddress_(nextSiblingSegmentedAddress);
      PopulateGlobalMatricesRecursively_(
          nextSiblingOffset,
          ref mocapJointAnimationIndex,
          animationFileAddress,
          f,
          matrix,
          rotationValues,
          jointMocapRotations,
          lazyMocapBoneTracks,
          globalMatrixByBone,
          bones);
    }
  }

  public static int Truncate(short value) {
    var step1 = value << 0xc;
    var step2 = step1 >> 0x10;
    var step3 = step2;
    if (step3 < 0) {
      step3 += 0x1000;
    }
    return step3;
  }

  public static float ConvertShortToRadians(short value) {
    var iVar1 = Truncate(value);
    return iVar1 / 4096.0f * 2 * MathF.PI;
  }

  private static Matrix4x4 ConvertRadiansToRotationMatrix_(
      short xValue,
      short yValue,
      short zValue) {
    // From decomp: 0x80117df4

    var xRadians = ConvertShortToRadians(xValue);
    var yRadians = ConvertShortToRadians(yValue);
    var zRadians = ConvertShortToRadians(zValue);

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

    matrix = Matrix4x4.Transpose(matrix);

    return matrix;
  }

  private static int GetOffsetFromSegmentedAddress_(uint segmentedAddress)
    => (int) ((segmentedAddress - 0x0200FB88) / 0x48) - 1;
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