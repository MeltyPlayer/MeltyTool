using fin.io;

using schema.binary;

namespace visceral.api;

public class Tg4hFileIdDictionary : IReadOnlyFileIdDictionary {
  private readonly IFileIdDictionary impl_;

  public Tg4hFileIdDictionary(IReadOnlyTreeDirectory baseDirectory) {
      this.BaseDirectory = baseDirectory;
      this.impl_ = new FileIdDictionary(baseDirectory);
      this.PopulateFromBaseDirectory_(baseDirectory);
    }

  public Tg4hFileIdDictionary(IReadOnlyTreeDirectory baseDirectory,
                              ISystemFile fileIdFile) {
      this.BaseDirectory = baseDirectory;
      if (fileIdFile.Exists) {
        this.impl_ = new FileIdDictionary(baseDirectory, fileIdFile);
      } else {
        this.impl_ = new FileIdDictionary(baseDirectory);
        this.PopulateFromBaseDirectory_(baseDirectory);
        this.Save(fileIdFile);
      }
    }

  private void PopulateFromBaseDirectory_(
      IReadOnlyTreeDirectory baseDirectory) {
      foreach (var tg4hFile in baseDirectory.GetFilesWithFileType(".tg4h",
                 true)) {
        using var br = tg4hFile.OpenReadAsBinary(Endianness.LittleEndian);
        br.Position = 0x14;
        var id = br.ReadUInt32();
        this.impl_[id] = tg4hFile;
      }
    }

  public IReadOnlyTreeDirectory BaseDirectory { get; }
  public IReadOnlyTreeFile this[uint id] => this.impl_[id];
  public void Save(IGenericFile fileIdFile) => this.impl_.Save(fileIdFile);
}