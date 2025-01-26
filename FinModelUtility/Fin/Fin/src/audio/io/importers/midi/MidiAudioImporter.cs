using System;

using fin.util.sets;

using SimpleSynth.Parameters;
using SimpleSynth.Parsing;
using SimpleSynth.Providers;
using SimpleSynth.Synths;

namespace fin.audio.io.importers.midi;

public class MidiAudioImporter : IAudioImporter<MidiAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      MidiAudioFileBundle audioFileBundle) {
    var midiFile = audioFileBundle.MidiFile;
    using var ms = midiFile.OpenRead();
    var midi = new MidiInterpretation(ms, new DefaultNoteSegmentProvider());

    // Create a new synthesizer with default providers.
    var synth = new BasicSynth(midi, new DefaultAdsrEnvelopeProvider(AdsrParameters.Short), new DefaultBalanceProvider());
    var signal = synth.GetSignal();

    var mutableBuffer = audioManager.CreateLoadedAudioBuffer(
        audioFileBundle,
        midiFile.AsFileSet());

    mutableBuffer.Frequency = signal.SamplingRate;

    {
      var samples = signal.Samples;
      var sampleCount = samples.Length;

      var channelCount = 1;
      var floatCount = channelCount * sampleCount;
      var floatPcm = new float[floatCount];

      var channels = new short[channelCount][];
      for (var c = 0; c < channelCount; ++c) {
        channels[c] = new short[sampleCount];
      }

      for (var i = 0; i < sampleCount; ++i) {
        for (var c = 0; c < channelCount; ++c) {
          var floatSample = floatPcm[channelCount * i + c];

          var floatMin = -1f;
          var floatMax = 1f;

          var normalizedFloatSample =
              (MathF.Max(floatMin, Math.Min(floatSample, floatMax)) -
               floatMin) / (floatMax - floatMin);

          float shortMin = short.MinValue;
          float shortMax = short.MaxValue;

          var shortSample = (short) (shortMin +
                                     normalizedFloatSample *
                                     (shortMax - shortMin));

          channels[c][i] = shortSample;
        }
      }

      mutableBuffer.SetPcm(channels);
    }

    return [mutableBuffer];
  }
}