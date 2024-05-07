using System.Runtime.CompilerServices;

using fin.math;
using fin.util.asserts;

using schema.binary;

namespace ast.schema {
  /// <summary>
  ///   Cobbled together from various sources:
  ///     - https://github.com/vgmstream/vgmstream/blob/37cc12295c92ec6aa874118fb237bd3821970836/src/meta/ast.c
  /// </summary>
  public partial class Ast {
    private void ReadAdpcm_(IBinaryReader br) {
      var channelCount = 2;
      Asserts.Equal(channelCount, this.StrmHeader.ChannelCount);
      var sampleCount = this.StrmHeader.SampleCount;

      var leftChannel = new short[sampleCount];
      var leftChannelSampleIndex = 0;

      var rightChannel = new short[sampleCount];
      var rightChannelSampleIndex = 0;

      this.ChannelData = [leftChannel, rightChannel];

      Asserts.Equal(2, channelCount);

      // TODO: This doesn't look right???
      br.Position = 0x40;

      var histL1 = 0;
      var histL2 = 0;
      var histR1 = 0;
      var histR2 = 0;

      var blocks =
          new LinkedList<(byte[] left, byte[] right)>();
      while (!br.Eof) {
        var blckHeader = br.ReadNew<BlckHeader>();

        // TODO: Does this need to be split up as left/right channels??
        var blockSize = blckHeader.BlockSizeInBytes;
        var leftChannelAdpcm = br.ReadBytes(blockSize);
        var rightChannelAdpcm = br.ReadBytes(blockSize);

        blocks.AddLast((leftChannelAdpcm, rightChannelAdpcm));
      }

      foreach (var (leftChannelAdpcm, rightChannelAdpcm) in
               blocks) {
        this.decode_ngc_afc(leftChannelAdpcm,
                            leftChannel,
                            sampleCount,
                            ref leftChannelSampleIndex,
                            ref histL1,
                            ref histL2);
        this.decode_ngc_afc(rightChannelAdpcm,
                            rightChannel,
                            sampleCount,
                            ref rightChannelSampleIndex,
                            ref histR1,
                            ref histR2);
      }
    }

    private static (short, short)[] afc_coefs = [
        (0, 0),
        (2048, 0),
        (0, 2048),
        (1024, 1024),
        (4096, -2048),
        (3584, -1536),
        (3072, -1024),
        (4608, -2560),
        (4200, -2248),
        (4800, -2300),
        (5120, -3072),
        (2048, -2048),
        (1024, -1024),
        (-1024, 1024),
        (-1024, 0),
        (-2048, 0)
    ];

    private void decode_ngc_afc(ReadOnlySpan<byte> adpcmData,
                                Span<short> channelData,
                                uint sampleCount,
                                ref int sampleIndex,
                                ref int hist1,
                                ref int hist2) {
      var bytesPerFrame = 0x09;
      var samplesPerFrame = 16; // (bytesPerFrame - 1) * 2

      var frameCount = Math.Min(adpcmData.Length / bytesPerFrame,
                                sampleCount / samplesPerFrame);

      for (var frameIndex = 0; frameIndex < frameCount; ++frameIndex) {
        /* parse frame header */
        var frameOffset = frameIndex * bytesPerFrame;
        var frame = adpcmData.Slice(frameOffset, bytesPerFrame);

        var scale = 1 << ((frame[0] >> 4) & 0xf);
        var index = (frame[0] & 0xf);
        var coef1 = afc_coefs[index].Item1;
        var coef2 = afc_coefs[index].Item2;

        /* decode nibbles */
        for (var sI = 0; sI < samplesPerFrame; sI++, sampleIndex++) {
          if (sampleIndex >= sampleCount) {
            return;
          }
          var nibbles = frame[0x01 + sI / 2];

          var isLeftChannel = sampleIndex.GetBit(0);
          var sample = isLeftChannel
              ? /* high nibble first */
              GetLowNibbleSigned_(nibbles)
              : GetHighNibbleSigned_(nibbles);
          sample = ((sample * scale) << 11);
          sample = (sample + coef1 * hist1 + coef2 * hist2) >> 11;

          channelData[sampleIndex] = Clamp.Int16(sample);

          hist2 = hist1;
          hist1 = sample;
        }
      }
    }

    private static readonly int[] nibbleToInt_ = [
        0, 1, 2, 3, 4, 5, 6, 7, -8, -7, -6, -5, -4, -3, -2, -1
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetHighNibbleSigned_(byte n) => nibbleToInt_[n >> 4];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetLowNibbleSigned_(byte n) => nibbleToInt_[n & 0xf];
  }
}