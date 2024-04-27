using fin.data.dictionaries;
using fin.io;

using schema.binary;
using schema.binary.attributes;
using schema.readOnly;

namespace uni.util.io {
  [GenerateReadOnly]
  public partial interface IFileIdsDictionary {
    IEnumerable<IReadOnlyTreeFile> this[uint id] { get; }

    public void AddFile(uint id, IReadOnlyTreeFile file);

    [Const]
    void Save(IGenericFile file);
  }

  public partial class FileIdsDictionary : IFileIdsDictionary {
    private readonly IReadOnlyTreeDirectory baseDirectory_;
    private readonly SetDictionary<uint, string> impl_ = new();

    public FileIdsDictionary(IReadOnlyTreeDirectory baseDirectory,
                             IReadOnlyGenericFile fileIds) {
      this.baseDirectory_ = baseDirectory;
      foreach (var fileIdsPair in fileIds.ReadNew<FileIds>().Pairs) {
        foreach (var filePath in fileIdsPair.FilePaths) {
          this.impl_.Add(fileIdsPair.Id, filePath.FilePath);
        }
      }
    }

    public FileIdsDictionary(IReadOnlyTreeDirectory baseDirectory) {
      this.baseDirectory_ = baseDirectory;
      this.impl_ = new();
    }

    public IEnumerable<IReadOnlyTreeFile> this[uint id]
      => this.impl_[id].Select(this.baseDirectory_.AssertGetExistingFile);

    public void AddFile(uint id, IReadOnlyTreeFile file)
      => this.impl_.Add(id, file.AssertGetPathRelativeTo(this.baseDirectory_));

    public void Save(IGenericFile file) => file.Write(new FileIds {
        Pairs = this.impl_
                    .Select(pair => new FileIdsPair {
                        Id = pair.Key,
                        FilePaths = pair.Value
                                        .Select(v => new NullableString
                                                    { FilePath = v })
                                        .ToArray()
                    })
                    .ToArray()
    });

    [BinarySchema]
    private partial class FileIds : IBinaryConvertible {
      [RSequenceUntilEndOfStream]
      public FileIdsPair[] Pairs { get; set; }
    }

    [BinarySchema]
    private partial class FileIdsPair : IBinaryConvertible {
      public uint Id { get; set; }

      [IntegerFormat(SchemaIntegerType.BYTE)]
      public NullableString[] FilePaths { get; set; }
    }

    private partial class NullableString {
      [NullTerminatedString]
      public string FilePath { get; set; }
    }
  }
}