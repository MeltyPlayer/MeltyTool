using fin.io;

using schema.binary;

namespace visceral.api {
  public class MtlbFileIdsDictionary : IReadOnlyFileIdsDictionary {
    private readonly IReadOnlyFileIdsDictionary impl_;

    public MtlbFileIdsDictionary(IReadOnlyTreeDirectory baseDirectory,
                                 ISystemFile fileIdsFile) {
      this.BaseDirectory = baseDirectory;
      if (fileIdsFile.Exists) {
        this.impl_ = new FileIdsDictionary(baseDirectory, fileIdsFile);
      } else {
        var impl = new FileIdsDictionary(baseDirectory);
        this.impl_ = impl;
        foreach (var mtlbFile in baseDirectory.GetFilesWithFileType(
                     ".mtlb",
                     true)) {
          using var br = mtlbFile.OpenReadAsBinary(Endianness.LittleEndian);
          br.Position = 8;
          var id = br.ReadUInt32();
          impl.AddFile(id, mtlbFile);
        }

        this.Save(fileIdsFile);
      }
    }

    public IReadOnlyTreeDirectory BaseDirectory { get; }

    public IEnumerable<IReadOnlyTreeFile> this[uint id] => this.impl_[id];

    public void Save(IGenericFile fileIdsFile) => this.impl_.Save(fileIdsFile);
  }
}