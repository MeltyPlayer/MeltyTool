using fin.io;

namespace fin.audio.io.importers.midi;

public class MidiAudioFileBundle(
    IReadOnlyTreeFile midiFile,
    IReadOnlyTreeFile soundFontFile) : IAudioFileBundle {
  public string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.MidiFile;

  public IReadOnlyTreeFile MidiFile { get; } = midiFile;
  public IReadOnlyTreeFile SoundFontFile { get; } = soundFontFile;
}