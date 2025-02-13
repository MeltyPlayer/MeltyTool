using System;
using System.IO;

using CommunityToolkit.HighPerformance;

using fin.io;
using fin.math;
using fin.util.asserts;
using fin.util.hash;

using OggVorbisEncoder;

namespace fin.audio.io.exporters.ogg;

/// <summary>
///   Based on example at:
///   https://github.com/SteveLillis/.NET-Ogg-Vorbis-Encoder/blob/master/OggVorbisEncoder.Example/Encoder.cs
/// </summary>
public class OggAudioExporter : IAudioExporter {
  private const int WRITE_BUFFER_SIZE = 512;

  public void ExportAudio(IAudioBuffer<short> audioBuffer,
                          ISystemFile outputFile) {
    Asserts.SequenceEqual(".ogg", outputFile.FileType.ToLower());

    using var outputData = outputFile.OpenWrite();

    var channelCount = audioBuffer.AudioChannelsType switch {
        AudioChannelsType.MONO      => 1,
        AudioChannelsType.STEREO    => 2,
        _                           => throw new ArgumentOutOfRangeException()
    };

    var hash = new FluentHash();
    var lengthInSamples = audioBuffer.LengthInSamples;
    var floatSamples = new float[channelCount][];
    for (var c = 0; c < channelCount; ++c) {
      var channelSamples = floatSamples[c] = new float[lengthInSamples];

      var channel = channelCount switch {
          1 => AudioChannelType.MONO,
          2 => c switch {
              0 => AudioChannelType.STEREO_LEFT,
              1 => AudioChannelType.STEREO_RIGHT,
              _ => throw new ArgumentOutOfRangeException()
          },
          _ => throw new ArgumentOutOfRangeException()
      };

      for (var i = 0; i < lengthInSamples; ++i) {
        var shortSample = audioBuffer.GetPcm(channel, i);
        var floatSample = (((float) shortSample) / 32768).Clamp(-1, 1);
        channelSamples[i] = floatSample;
      }

      hash.With(channelSamples.AsSpan().AsBytes());
    }

    var oggStream = new OggStream(hash);

    // =========================================================
    // HEADER
    // =========================================================
    // Vorbis streams begin with three headers; the initial header (with
    // most of the codec setup parameters) which is mandated by the Ogg
    // bitstream spec.  The second header holds any comment fields.  The
    // third header holds the bitstream codebook.
    var info = VorbisInfo.InitVariableBitRate(channelCount,
                                              audioBuffer.Frequency,
                                              .5f);
    var infoPacket = HeaderPacketBuilder.BuildInfoPacket(info);
    var commentsPacket
        = HeaderPacketBuilder.BuildCommentsPacket(new Comments());
    var booksPacket = HeaderPacketBuilder.BuildBooksPacket(info);

    oggStream.PacketIn(infoPacket);
    oggStream.PacketIn(commentsPacket);
    oggStream.PacketIn(booksPacket);

    // Flush to force audio data onto its own page per the spec
    FlushPages_(oggStream, outputData, true);

    // =========================================================
    // BODY (Audio Data)
    // =========================================================
    var processingState = ProcessingState.Create(info);

    for (int readIndex = 0;
         readIndex <= lengthInSamples;
         readIndex += WRITE_BUFFER_SIZE) {
      if (readIndex == lengthInSamples) {
        processingState.WriteEndOfStream();
      } else {
        var writeLength
            = Math.Min(lengthInSamples - readIndex, WRITE_BUFFER_SIZE);
        processingState.WriteData(floatSamples, writeLength, readIndex);
      }

      while (!oggStream.Finished &&
             processingState.PacketOut(out OggPacket packet)) {
        oggStream.PacketIn(packet);

        FlushPages_(oggStream, outputData, false);
      }
    }

    FlushPages_(oggStream, outputData, true);
  }

  private static void FlushPages_(OggStream oggStream,
                                  Stream output,
                                  bool force) {
    while (oggStream.PageOut(out OggPage page, force)) {
      output.Write(page.Header, 0, page.Header.Length);
      output.Write(page.Body, 0, page.Body.Length);
    }
  }
}