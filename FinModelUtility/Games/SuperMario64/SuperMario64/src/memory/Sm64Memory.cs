using f3dzex2.io;

using fin.compression;
using fin.util.asserts;
using fin.util.enumerables;

using schema.binary;

using sm64.schema;

namespace sm64.memory {
  public interface IReadOnlySm64Memory : IReadOnlyN64Memory {
    byte? AreaId { get; }
  }

  public interface ISm64Memory : IN64Memory, IReadOnlySm64Memory {
    new byte? AreaId { get; set; }
  }

  public class Sm64Memory : ISm64Memory {
    public byte? AreaId { get; set; }

    public Endianness Endianness => Endianness.BigEndian;

    public IEnumerable<SchemaBinaryReader> OpenPossibilitiesAtSegmentedAddress(
        uint address)
      => this.OpenAtSegmentedAddress(address).Yield();

    public bool TryToOpenPossibilitiesAtSegmentedAddress(uint segmentedAddress,
      out IEnumerable<SchemaBinaryReader> possibilities) {
      possibilities = this.OpenPossibilitiesAtSegmentedAddress(segmentedAddress);
      return true;
    }

    public SchemaBinaryReader OpenSegment(Segment segment, uint? offset = null) {
      throw new NotImplementedException();
    }

    public IEnumerable<SchemaBinaryReader> OpenPossibilitiesForSegment(
        uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsValidSegment(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsValidSegmentedAddress(uint segmentedAddress) {
      throw new NotImplementedException();
    }

    public SchemaBinaryReader OpenAtSegmentedAddress(uint segmentedAddress) {
      IoUtils.SplitSegmentedAddress(segmentedAddress,
                                    out var segment,
                                    out var offset);
      var br = new SchemaBinaryReader(
          Asserts.CastNonnull(ROM.Instance.getSegment(segment, this.AreaId)),
          SchemaConstants.SM64_ENDIANNESS);
      br.Position = offset;
      return br;
    }

    public SchemaBinaryReader OpenSegment(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public bool IsSegmentCompressed(uint segmentIndex) {
      throw new NotImplementedException();
    }

    public Segment GetSegment(uint segmentAddress)
      => new() { Offset = 0, Length = 0 };

    public void AddSegment(uint segmentIndex,
                           uint offset,
                           uint length,
                           IArrayToArrayDecompressor? decompressor = null) {
      throw new NotImplementedException();
    }

    public void AddSegment(uint segmentIndex, Segment segment) {
      throw new NotImplementedException();
    }

    public void SetSegment(uint segmentIndex,
                           uint offset,
                           uint length,
                           IArrayToArrayDecompressor? decompressor = null) {
      throw new NotImplementedException();
    }

    public void SetSegment(uint segmentIndex, Segment segment) {
      throw new NotImplementedException();
    }
  }
}