namespace fin.audio.io.importers;

public interface IAudioImporter<in TAudioFileBundle>
    where TAudioFileBundle : IAudioFileBundle {
  ILoadedAudioBuffer<short> ImportAudio(
      IAudioManager<short> audioManager,
      TAudioFileBundle audioFileBundle);
}