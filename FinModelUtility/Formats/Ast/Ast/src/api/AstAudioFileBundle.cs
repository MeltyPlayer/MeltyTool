using fin.audio.io;
using fin.io;

namespace ast.api;

public class AstAudioFileBundle : IAudioFileBundle {
  public IReadOnlyTreeFile MainFile => this.AstFile;
  public required IReadOnlyTreeFile AstFile { get; init; }
}