

/*namespace fin.testing {
  public class FakeDirectory : IDirectory {
    private IDirectory? parent_;
    private List<IDirectory> subdirs_ = new();

    private FakeDirectory(IDirectory? parent, string name) {
      this.parent_ = parent;
      this.Name = name;
      this.FullName = (parent?.FullName ?? "") + "/" + name;
    }

    public static FakeDirectory CreateRoot(string name)
      => new FakeDirectory(null, name);


    public FakeDirectory AddSubdir(string name) {
      var subdir = new FakeDirectory(this, name);
      this.subdirs_.Add(subdir);
      return subdir;
    }

    public IFile AddFile(string file)
    => 


    public string Name { get; }
    public string FullName { get; }

    public IDirectory? GetParent() => this.parent_;

    public bool Exists => true;
    public bool Create() => throw new NotSupportedException();

    public IEnumerable<IDirectory> GetExistingSubdirs() => this.subdirs_;
    public IDirectory GetExistingSubdir(string relativePath, bool create = false) {
      throw new NotImplementedException();
    }

    public IEnumerable<IFile> GetExistingFiles() {
      throw new NotImplementedException();
    }

    public IEnumerable<IFile> SearchForFiles(string searchPattern) {
      throw new NotImplementedException();
    }

    public IFile TryToGetFile(string relativePath) {
      throw new NotImplementedException();
    }

    private class FakeFile : IFile {
      public FakeFile(IDirectory parent, string parent) {

      }

      public string Name { get; }
      public string FullName { get; }
      public IDirectory? GetParent() {
        throw new NotImplementedException();
      }

      public bool Exists { get; }
      public string Extension { get; }
      public IFile CloneWithExtension(string newExtension) {
        throw new NotImplementedException();
      }

      public StreamReader ReadAsText() {
        throw new NotImplementedException();
      }

      public byte[] SkimAllBytes() {
        throw new NotImplementedException();
      }
    }
  }

  public class FakeFile : IFile {}
}*/