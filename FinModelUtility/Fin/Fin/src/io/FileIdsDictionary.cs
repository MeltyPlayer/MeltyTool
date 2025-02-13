using System.Collections.Generic;
using System.Linq;

using fin.data.dictionaries;

using schema.binary;
using schema.binary.attributes;
using schema.readOnly;

namespace fin.io;

[GenerateReadOnly]
public partial interface IFileIdsDictionary {
  new IEnumerable<IReadOnlyTreeFile> this[uint id] { get; }
  bool TryToLookUpFiles(uint id, out IEnumerable<IReadOnlyTreeFile> files);

  public void AddFile(uint id, IReadOnlyTreeFile file);

  [Const]
  new void Save(IGenericFile fileIdsFile);
}

public partial class FileIdsDictionary : IFileIdsDictionary {
  private readonly IReadOnlyTreeDirectory baseDirectory_;
  private readonly SetDictionary<uint, string> impl_ = new();

  public FileIdsDictionary(IReadOnlyTreeDirectory baseDirectory,
                           IReadOnlyGenericFile fileIdsFile) {
    this.baseDirectory_ = baseDirectory;
    foreach (var fileIdsPair in fileIdsFile.ReadNew<FileIds>().Pairs) {
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
    => this.impl_[id].Select(n => this.baseDirectory_.AssertGetExistingFile(n));

  public bool TryToLookUpFiles(uint id,
                               out IEnumerable<IReadOnlyTreeFile> files) {
    if (this.impl_.TryGetSet(id, out var set)) {
      files = set!.Select(n => this.baseDirectory_.AssertGetExistingFile(n));
      return true;
    }

    files = default!;
    return false;
  }

  public void AddFile(uint id, IReadOnlyTreeFile file)
    => this.impl_.Add(id, file.AssertGetPathRelativeTo(this.baseDirectory_));

  public void Save(IGenericFile fileIdsFile)
    => fileIdsFile.Write(
        new FileIds {
            Pairs = this.impl_
                        .Select(pair => new FileIdsPair {
                            Id = pair.Key,
                            FilePaths = pair.Value
                                            .Select(v => new SizedString
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

    [SequenceLengthSource(SchemaIntegerType.BYTE)]
    public SizedString[] FilePaths { get; set; }
  }

  [BinarySchema]
  private partial class SizedString : IBinaryConvertible {
    [StringLengthSource(SchemaIntegerType.UINT16)]
    public string FilePath { get; set; }
  }
}