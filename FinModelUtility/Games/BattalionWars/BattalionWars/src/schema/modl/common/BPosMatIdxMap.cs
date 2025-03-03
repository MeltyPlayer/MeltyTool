using schema.binary;

namespace modl.schema.modl.common;

public abstract class BPosMatIdxMap(int tenCount) : IBinaryDeserializable {
  private readonly int[] impl_ = new int[tenCount * 10];

  public void Read(IBinaryReader br) {
    // This may look simple, but it was an ABSOLUTE nightmare to reverse engineer, lol.
    var currentPosMatIdx = 0;
    var currentOffset = 0;
    for (var i = 0; i < tenCount; ++i) {
      var posMatIdxOffsetFlags = br.ReadUInt32();

      // Loops over each bit in the offset.
      for (var b = 0; b < 32; ++b) {
        var currentBit = ((posMatIdxOffsetFlags >> b) & 1) == 1;

        // If bit is true, then we increment the current posMatIdx.
        if (currentBit) {
          this.impl_[currentPosMatIdx] = currentPosMatIdx + currentOffset;
          currentPosMatIdx++;
        }
        // Otherwise, if bit is false, then we increment the current offset.
        else {
          currentOffset++;
        }
      }
    }
  }

  public int this[int index] => this.impl_[index];
}