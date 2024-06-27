using fin.audio.io;
using fin.io;

namespace ast.api;

public class AstAudioFileBundle : IAudioFileBundle {
  public required string GameName { get; init; }

  public IReadOnlyTreeFile MainFile => this.AstFile;

  public required IReadOnlyTreeFile AstFile { get; init; }
}