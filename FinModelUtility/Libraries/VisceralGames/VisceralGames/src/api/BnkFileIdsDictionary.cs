using fin.io;

using schema.binary;

namespace visceral.api;

public class BnkFileIdsDictionary : IReadOnlyFileIdsDictionary {
  private readonly IFileIdsDictionary impl_;

  public BnkFileIdsDictionary(IReadOnlyTreeDirectory baseDirectory) {
    this.BaseDirectory = baseDirectory;
    this.impl_ = new FileIdsDictionary(baseDirectory);
    this.PopulateFromBaseDirectory_(baseDirectory);
  }

  public BnkFileIdsDictionary(IReadOnlyTreeDirectory baseDirectory,
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
                 ".bnk.WIN",
                 true)) {
      using var br = mtlbFile.OpenReadAsBinary(Endianness.LittleEndian);
      br.Position = 12;
      var id = br.ReadUInt32();
      this.impl_.AddFile(id, mtlbFile);
    }
  }

  public IReadOnlyTreeDirectory BaseDirectory { get; }
  public IEnumerable<IReadOnlyTreeFile> this[uint id] => this.impl_[id];

  public bool TryToLookUpBnks(uint id, out IEnumerable<IReadOnlyTreeFile> bnks)
    => this.impl_.TryToLookUpFiles(id, out bnks);

  public void Save(IGenericFile fileIdsFile) => this.impl_.Save(fileIdsFile);
}