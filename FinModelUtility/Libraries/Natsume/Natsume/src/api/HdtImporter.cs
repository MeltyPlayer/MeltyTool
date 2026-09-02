using CommunityToolkit.Diagnostics;

using fin.archives;
using fin.io;

namespace natsume.api;

public sealed record HdtFileBundle(
    ISystemFile HdtFile,
    ISystemFile BinFile,
    string[] FilePaths)
    : ISimpleCleanableArchiveFileBundle {
  public IReadOnlyTreeFile MainFile => this.BinFile;

  public void CleanUp() {
    this.HdtFile.Delete();
    this.BinFile.Delete();
  }
}

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Suphanat1722/hm-btn-mod-tool/blob/main/src/hm_btn_tool/archive.py
/// </summary>
public sealed class HdtImporter : BSimpleArchiveImporter<HdtFileBundle> {
  protected override void BuildHierarchyAndGetFileStream(
      HdtFileBundle bundle,
      ISet<IReadOnlyGenericFile> fileSet,
      ISimpleArchiveDirectory builderRoot,
      out Stream baseStream,
      out Stream readStream) {
    baseStream = readStream = bundle.BinFile.OpenRead();

    var hdtBr = bundle.HdtFile.OpenReadAsBinary();

    var filePaths = bundle.FilePaths;
    var fileCount = filePaths.Length;
    
    var offsetCount = fileCount + 1;
    Guard.IsEqualTo(hdtBr.Length, offsetCount * 4);
    var offsets = hdtBr.ReadUInt32s(offsetCount);

    for (var i = 0; i < fileCount; ++i) {
      var filePath = filePaths[i];

      var offset = offsets[i];
      var nextOffset = offsets[i + 1];

      builderRoot.AddFile(filePath, offset, nextOffset - offset);
    }
  }
}