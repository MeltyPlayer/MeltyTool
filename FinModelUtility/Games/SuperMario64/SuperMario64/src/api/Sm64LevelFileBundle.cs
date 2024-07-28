using fin.io;

namespace sm64.api {
  public class Sm64LevelFileBundle(
      IReadOnlyTreeDirectory directory,
      IReadOnlyTreeFile sm64Rom,
      LevelId levelId)
      : BSm64FileBundle, IUiFile {
    public override IReadOnlyTreeFile? MainFile => null;
    public IReadOnlyTreeDirectory Directory { get; } = directory;

    public IReadOnlyTreeFile Sm64Rom { get; } = sm64Rom;
    public LevelId LevelId { get; } = levelId;
    string IUiFile.HumanReadableName => $"{this.LevelId}".ToLower();
    public string TrueFullPath => this.Sm64Rom.FullPath;
  }
}