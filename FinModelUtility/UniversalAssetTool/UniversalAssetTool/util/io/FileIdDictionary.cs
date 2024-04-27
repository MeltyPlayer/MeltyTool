using fin.io;

using schema.binary;
using schema.binary.attributes;
using schema.readOnly;

namespace uni.util.io {
  [GenerateReadOnly]
  public partial interface IFileIdDictionary {
    IReadOnlyTreeFile this[uint id] { get; set; }

    [Const]
    void Save(IGenericFile file);
  }

  public partial class FileIdDictionary : IFileIdDictionary {
    private readonly IReadOnlyTreeDirectory baseDirectory_;
    private readonly Dictionary<uint, string> impl_;

    public FileIdDictionary(IReadOnlyTreeDirectory baseDirectory,
                            IReadOnlyGenericFile fileIds) {
      this.baseDirectory_ = baseDirectory;
      this.impl_ = fileIds.ReadNew<FileIds>()
                          .Pairs.ToDictionary(pair => pair.Id,
                                              pair => pair.FilePath);
    }

    public FileIdDictionary(IReadOnlyTreeDirectory baseDirectory) {
      this.baseDirectory_ = baseDirectory;
      this.impl_ = new();
    }

    public IReadOnlyTreeFile this[uint id] {
      get => this.baseDirectory_.AssertGetExistingFile(this.impl_[id]);
      set => this.impl_[id]
          = value.AssertGetPathRelativeTo(this.baseDirectory_);
    }

    public void Save(IGenericFile file) => file.Write(new FileIds {
        Pairs = this.impl_
                    .Select(pair => new FileIdPair
                                { Id = pair.Key, FilePath = pair.Value })
                    .ToArray()
    });

    [BinarySchema]
    private partial class FileIds : IBinaryConvertible {
      [RSequenceUntilEndOfStream]
      public FileIdPair[] Pairs { get; set; }
    }

    [BinarySchema]
    private partial class FileIdPair : IBinaryConvertible {
      public uint Id { get; set; }

      [NullTerminatedString]
      public string FilePath { get; set; }
    }
  }
}