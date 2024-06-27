using System.Collections.Generic;
using System.Linq;

using f3dzex2.io;

using schema.binary;

using UoT.api;

namespace UoT.model {
  public interface ILimb2 : IBinaryDeserializable {
    short X { get; }
    short Y { get; }
    short Z { get; }

    sbyte FirstChildIndex { get; }
    sbyte NextSiblingIndex { get; }

    uint DisplayListSegmentedAddress { get; }
  }

  [BinarySchema]
  public partial class NonLinkLimb : ILimb2 {
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public sbyte FirstChildIndex { get; set; }
    public sbyte NextSiblingIndex { get; set; }

    public uint DisplayListSegmentedAddress { get; set; }
  }

  [BinarySchema]
  public partial class LinkLimb : ILimb2 {
    public short X { get; set; }
    public short Y { get; set; }
    public short Z { get; set; }

    public sbyte FirstChildIndex { get; set; }
    public sbyte NextSiblingIndex { get; set; }

    public uint DisplayListSegmentedAddress { get; set; }
    public uint LowLodDisplayListSegmentedAddress { get; set; }
  }

  public class LimbHierarchyReader2 {
    /// <summary>
    ///   Parses a limb hierarchy according to the following spec:
    ///   https://wiki.cloudmodding.com/oot/Animation_Format#Hierarchy
    /// </summary>
    public IList<ILimb2>? GetHierarchies(
        IReadOnlyN64Memory n64Memory,
        bool isLink) {
      using var entryEr = n64Memory.OpenSegment((uint) OotSegmentIndex.ZOBJECT);

      var limbSize = isLink ? 16 : 12;

      // We have no idea where the limbs will be defined in the current
      // segment, so we'll try each offset until we find one that's valid.
      for (long entryI = 0; entryI <= entryEr.Length - 8; entryI += 4) {
        entryEr.Position = entryI;


        // If the limb index address is invalid, we can safely assume this
        // offset is invalid.
        var possibleLimbIndexSegmentedAddress = entryEr.ReadUInt32();
        if (!n64Memory.IsValidSegmentedAddress(
                possibleLimbIndexSegmentedAddress)) {
          continue;
        }

        // If the limb count is 0, we can safely assume this offset is invalid.
        var possibleLimbCount = entryEr.ReadByte();
        if (possibleLimbCount == 0) {
          continue;
        }


        // Opens what *might* be the correct segmented address.
        using var limbIndexEr =
            n64Memory.OpenAtSegmentedAddress(possibleLimbIndexSegmentedAddress);

        // If the limb index section goes past the length of the segment, we
        // can safely assume this offset is invalid.
        if (limbIndexEr.Position + 4 * possibleLimbCount >=
            limbIndexEr.Length) {
          continue;
        }

        var originalLimbIndexErPosition = limbIndexEr.Position;


        // We now verify that each of the limbs is valid.
        bool areLimbsValid = true;
        bool somethingVisible = false;
        for (var limbI = 0; limbI < possibleLimbCount; ++limbI) {
          var possibleLimbSegmentedAddress = limbIndexEr.ReadUInt32();
          if (!n64Memory.IsValidSegmentedAddress(
                  possibleLimbSegmentedAddress)) {
            areLimbsValid = false;
            break;
          }

          using var limbEr =
              n64Memory.OpenAtSegmentedAddress(possibleLimbSegmentedAddress);

          if (limbEr.Position + limbSize >= limbEr.Length) {
            areLimbsValid = false;
            break;
          }

          var possibleLimb = isLink
              ? (ILimb2) limbEr.ReadNew<LinkLimb>()
              : limbEr.ReadNew<NonLinkLimb>();
          var firstChild = possibleLimb.FirstChildIndex;
          var nextSibling = possibleLimb.NextSiblingIndex;

          if (firstChild == limbI || nextSibling == limbI) {
            areLimbsValid = false;
            break;
          }

          var displayListAddress = possibleLimb.DisplayListSegmentedAddress;
          IoUtils.SplitSegmentedAddress(displayListAddress,
                                        out var displayListBank,
                                        out _);

          var isDisplayListVisible = displayListBank != 0;
          somethingVisible = somethingVisible || isDisplayListVisible;

          if (isDisplayListVisible &
              !n64Memory.IsValidSegmentedAddress(displayListAddress)) {
            areLimbsValid = false;
            break;
          }
        }

        if (!areLimbsValid || !somethingVisible) {
          continue;
        }


        limbIndexEr.Position = originalLimbIndexErPosition;
        return limbIndexEr
               .ReadUInt32s(possibleLimbCount)
               .Select(limbSegmentedAddress => {
                 var limbEr =
                     n64Memory.OpenAtSegmentedAddress(limbSegmentedAddress);

                 return isLink
                     ? (ILimb2) limbEr.ReadNew<LinkLimb>()
                     : limbEr.ReadNew<NonLinkLimb>();
               })
               .ToArray();
      }

      return null;
    }
  }
}