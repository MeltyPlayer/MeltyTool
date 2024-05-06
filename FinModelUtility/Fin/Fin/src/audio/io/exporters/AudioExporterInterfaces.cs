using fin.io;

namespace fin.audio.io.exporters {
  public interface IAudioExporter {
    void ExportAudio(IAudioBuffer<short> audioBuffer, ISystemFile outputFile);
  }
}