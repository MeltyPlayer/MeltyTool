using fin.data.queues;
using fin.io;

using SceneGate.Ekona.Containers.Rom;

using uni.games;

using Yarhl.FileSystem;

namespace uni.platforms.ds {
  internal class DsFileHierarchyExtractor {
    public IFileHierarchy ExtractFromRom(
        IReadOnlySystemFile romFile) {
      if (ExtractorUtil.HasNotBeenExtractedYet(romFile, out var outDir)) {
        var game = NodeFactory.FromFile(romFile.FullPath);
        game.TransformWith<Binary2NitroRom>();

        var nodeQueue = new FinTuple2Queue<string, Node>(("", game));
        while (nodeQueue.TryDequeue(out var path, out var node)) {
          path += node.Name;


          nodeQueue.Enqueue(node.Children.Select(child => (path, child)));
        }
      }

      return FileHierarchy.From(romFile.NameWithoutExtension, outDir);
    }
  }
}