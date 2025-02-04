using fin.util.sets;

using MeltySynth;

namespace fin.audio.io.importers.midi;

public class MidiAudioImporter : IAudioImporter<MidiAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      MidiAudioFileBundle audioFileBundle) {
    var midiFile = audioFileBundle.MidiFile;
    using var ms = midiFile.OpenRead();
    var midi = new MidiFile(ms);

    using var ss = audioFileBundle.SoundFontFile.OpenRead();
    var soundFont = new SoundFont(ss);

    var synthesizer = new Synthesizer(soundFont, 44100);

    var sequencer = new MidiFileSequencer(synthesizer);
    sequencer.Play(midi, true);

    var sampleCount = (int) (midi.Length.TotalSeconds * synthesizer.SampleRate);

    var leftChannel = new short[sampleCount];
    var rightChannel = new short[sampleCount];
    sequencer.RenderInt16(leftChannel, rightChannel);

    var mutableBuffer = audioManager.CreateLoadedAudioBuffer(
        audioFileBundle,
        midiFile.AsFileSet());

    mutableBuffer.Frequency = synthesizer.SampleRate;
    mutableBuffer.SetStereoPcm(leftChannel, rightChannel);

    return [mutableBuffer];
  }
}