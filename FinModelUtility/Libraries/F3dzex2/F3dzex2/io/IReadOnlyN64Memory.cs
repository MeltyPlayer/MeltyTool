using System.Collections.Generic;

using fin.compression;
using fin.util.types;

using schema.binary;

namespace f3dzex2.io;

public interface IReadOnlyN64Memory {
  Endianness Endianness { get; }

  SchemaBinaryReader OpenAtSegmentedAddress(uint segmentedAddress);

  IEnumerable<SchemaBinaryReader> OpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress);

  bool TryToOpenPossibilitiesAtSegmentedAddress(
      uint segmentedAddress,
      out IEnumerable<SchemaBinaryReader> possibilities);

  SchemaBinaryReader OpenSegment(uint segmentIndex);

  SchemaBinaryReader OpenSegment(ISegmentChunk segmentChunk,
                                 uint? offset = null);

  IEnumerable<SchemaBinaryReader> OpenPossibilitiesForSegment(
      uint segmentIndex);

  ISegmentChunk GetSegment(uint segmentIndex);
  bool IsValidSegment(uint segmentIndex);
  bool IsValidSegmentedAddress(uint segmentedAddress);
  bool IsSegmentCompressed(uint segmentIndex);
}

public interface IN64Memory : IReadOnlyN64Memory {
  void AddSegment(uint segmentIndex, ISegmentChunk segmentChunk);
  void SetSegment(uint segmentIndex, ISegmentChunk segmentChunk);
}

public interface ISeparateN64Memory : IN64Memory {
  void AddSegment(uint segmentIndex, uint offset, byte[] bytes);
  void SetSegment(uint segmentIndex, uint offset, byte[] bytes);
}

public interface ISlicedN64Memory : ISeparateN64Memory {
  void AddSegment(uint segmentIndex,
                  uint offset,
                  uint length,
                  IArrayToArrayDecompressor? decompressor = null);

  void SetSegment(uint segmentIndex,
                  uint offset,
                  uint length,
                  IArrayToArrayDecompressor? decompressor = null);
}

[UnionCandidate]
public interface ISegmentChunk {
  uint OffsetInSegment { get; }
  uint Length { get; }
}

public class SliceSegmentChunk : ISegmentChunk {
  public required uint OffsetInRom { get; init; }
  public uint OffsetInSegment { get; init; }
  public required uint Length { get; init; }
  public IArrayToArrayDecompressor? Decompressor { get; init; }
}

public class BytesSegmentChunk : ISegmentChunk {
  public required uint OffsetInSegment { get; init; }
  public uint Length => (uint) this.Bytes.Length;
  public required byte[] Bytes { get; init; }
}