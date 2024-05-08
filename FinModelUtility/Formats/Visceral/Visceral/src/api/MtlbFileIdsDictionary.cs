using fin.io;

using schema.binary;

namespace visceral.api {
  public class MtlbFileIdsDictionary : IReadOnlyFileIdsDictionary {
    private readonly IFileIdsDictionary impl_;

    public MtlbFileIdsDictionary(IReadOnlyTreeDirectory baseDirectory) {
      this.BaseDirectory = baseDirectory;
      this.impl_ = new FileIdsDictionary(baseDirectory);
      this.PopulateFromBaseDirectory_(baseDirectory);
    }

    public MtlbFileIdsDictionary(IReadOnlyTreeDirectory baseDirectory,
                                 ISystemFile fileIdsFile) {
      this.BaseDirectory = baseDirectory;
      if (fileIdsFile.Exists) {
        this.impl_ = new FileIdsDictionary(baseDirectory, fileIdsFile);
      } else {
        this.impl_ = new FileIdsDictionary(baseDirectory);
        this.PopulateFromBaseDirectory_(baseDirectory);
        this.Save(fileIdsFile);
      }
    }

    private void PopulateFromBaseDirectory_(
        IReadOnlyTreeDirectory baseDirectory) {
      foreach (var mtlbFile in baseDirectory.GetFilesWithFileType(
                   ".mtlb",
                   true)) {
        using var br = mtlbFile.OpenReadAsBinary(Endianness.LittleEndian);
        br.Position = 8;
        var id = br.ReadUInt32();
        this.impl_.AddFile(id, mtlbFile);
      }
    }

    public IReadOnlyTreeDirectory BaseDirectory { get; }
    public IEnumerable<IReadOnlyTreeFile> this[uint id] => this.impl_[id];
    public void Save(IGenericFile fileIdsFile) => this.impl_.Save(fileIdsFile);
  }
}