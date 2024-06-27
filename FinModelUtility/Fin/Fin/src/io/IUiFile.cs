namespace fin.io;

public interface IUiFile {
  string RawName { get; }
  string? HumanReadableName => null;
}