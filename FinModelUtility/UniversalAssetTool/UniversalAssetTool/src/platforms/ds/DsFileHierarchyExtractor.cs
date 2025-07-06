using fin.common;
using fin.data.queues;
using fin.io;

using SceneGate.Ekona.Containers.Rom;

using uni.games;

using Yarhl.FileSystem;

namespace uni.platforms.ds;

internal class DsFileHierarchyExtractor {
  public bool TryToExtractFromGame(
      string gameName,
      out IFileHierarchy fileHierarchy) {
    if (!this.TryToFindRom_(gameName, out var romFile)) {
      fileHierarchy = default;
      return false;
    }

    fileHierarchy = this.ExtractFromRom(romFile);
    return true;
  }

  private bool TryToFindRom_(string gameName, out ISystemFile romFile)
    => DirectoryConstants.ROMS_DIRECTORY
                         .TryToGetExistingFileWithFileType(
                             gameName,
                             out romFile,
                             ".nds");

  public IFileHierarchy ExtractFromRom(
      IReadOnlySystemFile romFile) {
      if (ExtractorUtil.HasNotBeenExtractedYet(romFile, out var outDir)) {
        var game = NodeFactory.FromFile(romFile.FullPath);
        game.TransformWith<Binary2NitroRom>();

        var extractedDirectory
            = ExtractorUtil.GetOrCreateExtractedDirectory(romFile);

        var nodeQueue
            = new FinTuple2Queue<ISystemDirectory, Node>(
                game.Children.Select(n => (extractedDirectory, n)));
        while (nodeQueue.TryDequeue(out var parentDirectory, out var node)) {
          if (node.IsContainer) {
            var directory = parentDirectory.GetOrCreateSubdir(node.Name);
            nodeQueue.Enqueue(node.Children.Select(n => (directory, n)));
            continue;
          }

          var stream = node.Stream;
          if (stream != null) {
            var path = Path.Join(parentDirectory.FullPath, node.Name);
            var file = new FinFile(path);
            using var fs = file.OpenWrite();
            stream.CopyTo(fs);
            fs.Flush();
          }
        }
      }

      return ExtractorUtil.GetFileHierarchy(romFile.NameWithoutExtension, outDir);
    }
}