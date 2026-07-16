using CommunityToolkit.Diagnostics;

using f3dzex2.io;

using fin.io;
using fin.model;
using fin.util.asserts;

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

      foreach (var animationFileAddress in animationOffsets) {
        br.Position = animationFileAddress;

        var unkFloats = br.ReadSingles(8);
        var frameCount = br.ReadUInt32();

        var expectedAnimationSegmentedAddress = (0x04 << 24) | animationFileAddress;
        br.AssertInt32(expectedAnimationSegmentedAddress);

        var animationSegmentedAddress = br.ReadUInt32();
        var unkCounts = br.ReadUInt32s(14);

        br.AssertUInt16((ushort) frameCount);
        // TODO: Not sure what this is?
        br.AssertUInt16(0x3d);

        var weirdScalesSegmentedAddress = br.ReadUInt32();
        var weirdIndices0SegmentedAddress = br.ReadUInt32();
        var unkShorts0SegmentedAddress = br.ReadUInt32();
        var weirdIndices1SegmentedAddress = br.ReadUInt32();
        var unkShorts1SegmentedAddress = br.ReadUInt32();
        var weirdIndices2SegmentedAddress = br.ReadUInt32();

        var weirdScaleSize = unkShorts0SegmentedAddress - weirdScalesSegmentedAddress;
        var unkShorts0Size = unkShorts1SegmentedAddress - unkShorts0SegmentedAddress;
        var unkShorts1Size = weirdIndices0SegmentedAddress - unkShorts1SegmentedAddress;
        var weirdIndices0Size = weirdIndices1SegmentedAddress - weirdIndices0SegmentedAddress;
        var weirdIndices1Size = weirdIndices2SegmentedAddress - weirdIndices1SegmentedAddress;
        var weirdIndices2Size = expectedAnimationSegmentedAddress - weirdIndices2SegmentedAddress;

        var actualWeirdScaleCount = weirdScaleSize / 4;
        var actualUnkShort0Count = unkShorts0Size / 2;
        var actualUnkShort1Count = unkShorts1Size / 2;
        var actualWeirdIndex0Count = weirdIndices0Size / 4;
        var actualWeirdIndex1Count = weirdIndices1Size / 4;
        var actualWeirdIndex2Count = weirdIndices2Size / 4;
            
        Guard.IsEqualTo(actualWeirdScaleCount, 19);
        Guard.IsEqualTo(actualWeirdIndex0Count, 6 * 0x3d);
        Guard.IsEqualTo(actualWeirdIndex1Count, 6 * 0x3d);
        Guard.IsEqualTo(actualWeirdIndex2Count, 6 * 0x3d);

        IoUtils.SplitSegmentedAddress(
            weirdScalesSegmentedAddress,
            out _,
            out var weirdScalesFileAddress);

        br.Position = weirdScalesFileAddress;

        var weirdScales = br.ReadSingles(actualWeirdScaleCount);

        var unkShorts0 = br.ReadUInt16s(actualUnkShort0Count);
        var unkShorts1 = br.ReadUInt16s(actualUnkShort1Count);

        var weirdIndices0 = br.ReadUInt32s(actualWeirdIndex0Count);
        var weirdIndices1 = br.ReadUInt32s(actualWeirdIndex1Count);
        var weirdIndices2 = br.ReadUInt32s(actualWeirdIndex2Count);
      }
    }
  }
}