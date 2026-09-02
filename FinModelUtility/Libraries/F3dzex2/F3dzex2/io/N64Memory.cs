using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CommunityToolkit.Diagnostics;

using fin.compression;
using fin.data.dictionaries;
using fin.io;
using fin.util.asserts;
using fin.util.hex;
using fin.util.linq;

using schema.binary;

namespace f3dzex2.io;

public abstract class BN64Memory(Endianness endianness = Endianness.BigEndian)
    : ISeparateN64Memory {
  private readonly ListDictionary<uint, ISegmentChunk> segments_ = new();

  public Endianness Endianness { get; } = endianness;

  public SchemaBinaryReader OpenAtSegmentedAddress(uint segmentedAddress) {
    var possibilities
        = this.OpenPossibilitiesAtSegmentedAddress(segmentedAddress);

    if (!possibilities.TryGetSingle(out var single)) {
      Asserts.Fail(
          $"Expected to have a single possibility for {segmentedAddress.ToHexString()}");
    }

    return single;
  }

  public IEnumerable<SchemaBinaryReader> OpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress) {
    if (!this.TryToOpenPossibilitiesAtSegmentedAddress(
            segmentedAddress,
            out var possibilities)) {
      Asserts.Fail(
          $"Expected 0x{segmentedAddress.ToHex()} to be a valid segmented address.");
    }

    return possibilities;
  }

  public bool TryToOpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress,
      out IEnumerable<SchemaBinaryReader> possibilities) {
    if (!this.TryToGetSegmentsAtSegmentedAddress_(
            segmentedAddress,
            out var offset,
            out var validSegments)) {
      possibilities = null;
      return false;
    }

    possibilities =
        validSegments.Select(segment => this.OpenSegment(segment, offset));
    return true;
  }

  public SchemaBinaryReader OpenSegment(uint segmentIndex)
    => this.OpenPossibilitiesForSegment(segmentIndex).Single();

  public abstract SchemaBinaryReader OpenSegment(ISegmentChunk segmentChunk,
                                                 uint? offset = null);

  public IEnumerable<SchemaBinaryReader> OpenPossibilitiesForSegment(
      uint segmentIndex)
    => this
       .segments_[segmentIndex]
       .Select(segment => this.OpenSegment(segment));


  public ISegmentChunk GetSegment(uint segmentIndex)
    => this.segments_[segmentIndex].Single();

  public bool IsValidSegment(uint segmentIndex)
    => this.segments_.HasList(segmentIndex);

  public bool IsValidSegmentedAddress(uint segmentedAddress) {
    IoUtils.SplitSegmentedAddress(segmentedAddress,
                                  out var segmentIndex,
                                  out var offset);
    if (!this.segments_.TryGetList(segmentIndex, out var segments)) {
      return false;
    }

    return segments.Any(segment => offset - segment.OffsetInSegment < segment.Length);
  }

  public bool IsSegmentCompressed(uint segmentIndex)
    => this.segments_[segmentIndex].Single() is SliceSegmentChunk {
        Decompressor: not null
    };

  public void AddSegment(uint segmentIndex, uint offset, byte[] bytes)
    => this.AddSegment(segmentIndex,
                       new BytesSegmentChunk {
                           OffsetInSegment = offset,
                           Bytes = bytes,
                       });

  public void AddSegment(uint segmentIndex, ISegmentChunk segmentChunk)
    => this.segments_.Add(segmentIndex, segmentChunk);

  public void SetSegment(uint segmentIndex, uint offset, byte[] bytes)
    => this.SetSegment(segmentIndex,
                       new BytesSegmentChunk {
                           OffsetInSegment = offset,
                           Bytes = bytes,
                       });

  public void SetSegment(uint segmentIndex, ISegmentChunk segmentChunk) {
    this.segments_.ClearList(segmentIndex);
    this.segments_.Add(segmentIndex, segmentChunk);
  }

  private bool TryToGetSegmentsAtSegmentedAddress_(
      uint segmentedAddress,
      out uint offset,
      out IEnumerable<ISegmentChunk> validSegments) {
    IoUtils.SplitSegmentedAddress(segmentedAddress,
                                  out var segmentIndex,
                                  out offset);
    var offsetInSegment = offset;

    if (!this.segments_.TryGetList(segmentIndex, out var segments)) {
      validSegments = null;
      return false;
    }

    validSegments =
        segments!.Where(segment => offsetInSegment - segment.OffsetInSegment <
                                   segment.Length);
    return segments!.Any();
  }
}

public sealed class SeparateN64Memory(
    Endianness endianness = Endianness.BigEndian)
    : BN64Memory(endianness) {
  public override SchemaBinaryReader OpenSegment(ISegmentChunk segmentChunk,
                                                 uint? offset = null) {
    switch (segmentChunk) {
      case BytesSegmentChunk bytesSegment: {
        var br = new SchemaBinaryReader(bytesSegment.Bytes, this.Endianness);
        if (offset != null) {
          br.Position = offset.Value - segmentChunk.OffsetInSegment;
        }
        return br;
      }
      default: throw new NotImplementedException();
    }
  }
}

public sealed class SlicedN64Memory(
    byte[] data,
    Endianness endianness = Endianness.BigEndian)
    : BN64Memory(endianness), ISlicedN64Memory {
  public SlicedN64Memory(
      IReadOnlyGenericFile file,
      Endianness endianness = Endianness.BigEndian) :
      this(file.ReadAllBytes(), endianness) { }

  public void AddSegment(uint segmentIndex,
                         uint offset,
                         uint length,
                         IArrayToArrayDecompressor? decompressor = null)
    => this.AddSegment(segmentIndex,
                       new SliceSegmentChunk {
                           OffsetInRom = offset,
                           Length = length,
                           Decompressor = decompressor,
                       });

  public void SetSegment(uint segmentIndex,
                         uint offset,
                         uint length,
                         IArrayToArrayDecompressor? decompressor = null)
    => this.SetSegment(segmentIndex,
                       new SliceSegmentChunk {
                           OffsetInRom = offset,
                           Length = length,
                           Decompressor = decompressor,
                       });

  public override SchemaBinaryReader OpenSegment(ISegmentChunk segmentChunk,
                                                 uint? offset = null) {
    switch (segmentChunk) {
      case BytesSegmentChunk bytesSegment: {
        var br = new SchemaBinaryReader(bytesSegment.Bytes, this.Endianness);
        if (offset != null) {
          br.Position = offset.Value - segmentChunk.OffsetInSegment;
        }

        return br;
      }
      case SliceSegmentChunk sliceSegment: {
        var br = new SchemaBinaryReader(
            new MemoryStream(data,
                             (int) sliceSegment.OffsetInRom,
                             (int) segmentChunk.Length),
            this.Endianness);
        if (offset != null) {
          br.Position = offset.Value - segmentChunk.OffsetInSegment;
        }

        return br;
      }
      default: throw new NotImplementedException();
    }
  }
}