using fin.util.asserts;

using schema.binary;

namespace ast.schema;

public partial class Ast {
  private void ReadPcm16_(IBinaryReader br) {
    var channelCount = 2;
    Asserts.Equal(channelCount, this.StrmHeader.ChannelCount);
    var sampleCount = this.StrmHeader.SampleCount;

    var channelData = new short[channelCount][];
    for (var i = 0; i < channelCount; ++i) {
      channelData[i] = new short[sampleCount];
    }
    this.ChannelData = channelData;

    for (var i = 0; i < channelCount; i += 2) {
      // TODO: This doesn't look right???
      br.Position = 0x40;

      for (var currentSample = 0; currentSample < sampleCount;) {
        if (br.Eof) {
          break;
        }

        var blckHeader = br.ReadNew<BlckHeader>();

        var blockSizeInBytes = blckHeader.BlockSizeInBytes;
        var blockSizeInShorts = blockSizeInBytes / 2;

        for (var chan = 0; chan < channelCount; chan++) {
          if (chan / 2 == i / 2) {
            var shorts =
                br.ReadInt16s(Math.Min(blockSizeInShorts,
                                       (br.Length - br.Position) / 2));

            if (shorts.Length != blockSizeInShorts) {
              if (currentSample + blockSizeInBytes / 2 >= sampleCount &&
                  chan + 1 == channelCount) {
                /* it seems that the last block is cut off? */
                /*printf("last block missing %#x bytes, filling with zero\n",
                        bytes_short);
                memset(dumpbuf + bytes_read, 0, bytes_short);*/
              }
            }

            for (var s = 0; s < shorts.Length; ++s) {
              if (currentSample + s < sampleCount) {
                channelData[chan][currentSample + s] = shorts[s];
              }
            }
          } else {
            // Skips samples
            br.Position += blockSizeInBytes;
          }
        }

        currentSample += (int)blockSizeInShorts;
      }
    }
  }
}