using fin.data.queues;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.sets;

using xmod.schema.ped;
using xmod.schema.skel;


namespace xmod.api;

public class PedModelImporter : IModelImporter<PedModelFileBundle> {
  public IModel Import(PedModelFileBundle modelFileBundle) {
    var pedFile = modelFileBundle.PedFile;
    var ped = pedFile.ReadNewFromText<Ped>();

    var files = pedFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = files
    };

    var animDirectory = pedFile.AssertGetParent();
    var skelFile = animDirectory.AssertGetExistingFile($"{ped.SkelName}.skel");
    var skel = skelFile.ReadNewFromText<Skel>();

    var boneQueue = new FinTuple2Queue<SkelBone, IBone?>((skel.Root, null));
    while (boneQueue.TryDequeue(out var skelBone, out var parentFinBone)) {
      var offset = skelBone.Offset;
      var finBone = (parentFinBone ?? finModel.Skeleton.Root).AddChild(
              offset.X,
              offset.Y,
              offset.Z);
      finBone.Name = skelBone.Name;

      boneQueue.Enqueue(skelBone.Children.Select(child => (child, finBone)));
    }

    return finModel;
  }
}