using System;
using System.Collections.Generic;

namespace fin.io.tree;

public abstract class BStubIoObject(string fullPath)
    : IReadOnlyTreeIoObject<BStubIoObject, StubDirectory, StubFile, string> {
  private readonly StubDirectory? parent_;

  public string FullPath => fullPath;
  public ReadOnlySpan<char> Name => FinIoStatic.GetName(this.FullPath);

  public StubDirectory AssertGetParent() {
    throw new NotImplementedException();
  }

  public bool TryGetParent(out StubDirectory parent) {
    throw new NotImplementedException();
  }

  public IEnumerable<StubDirectory> GetAncestry() {
    throw new NotImplementedException();
  }
}

public class StubFile(string fullPath)
    : BStubIoObject(fullPath),
      IReadOnlyTreeFile<BStubIoObject, StubDirectory, StubFile, string> {
  public string FullNameWithoutExtension
    => FinFileStatic.GetNameWithoutExtension(this.FullPath).ToString();

  public ReadOnlySpan<char> NameWithoutExtension
    => FinFileStatic.GetNameWithoutExtension(this.Name);
}

public class StubDirectory(string fullPath)
    : BStubIoObject(fullPath),
      IReadOnlyTreeDirectory<BStubIoObject, StubDirectory, StubFile, string> { }