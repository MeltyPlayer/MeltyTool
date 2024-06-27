namespace uni.platforms.threeDs.tools.gar {
  public interface IGarFileType {
    string TypeName { get; }
    IGarSubfile[] Files { get; }
  }

  public interface IGarSubfile {
    string FileName { get; }
    string? FullPath { get; }

    int Position { get; }
    int Length { get; }
  }
}
