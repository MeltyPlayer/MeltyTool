using fin.compression;

namespace f3dzex2.io;

public static class N64MemoryExtensions {
  public static Segment? GetSegmentOrNull(
      this IReadOnlyN64Memory n64Memory,
      uint index)
    => n64Memory.IsValidSegment(index)
        ? n64Memory.GetSegment(index)
        : null;
}