using System;

namespace fin.io;

public interface IUiFile {
  ReadOnlySpan<char> RawName { get; }
  string? HumanReadableName => null;
}