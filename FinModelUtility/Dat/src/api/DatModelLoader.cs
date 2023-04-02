using dat.schema;

using fin.io;
using fin.model;
using fin.model.impl;


namespace dat.api {
  public class DatModelFileBundle : IModelFileBundle {
    public required string GameName { get; init; }

    public IFileHierarchyFile MainFile => this.DatFile;
    public required IFileHierarchyFile DatFile { get; init; }
  }

  public class DatModelLoader : IModelLoader<DatModelFileBundle> {
    public IModel LoadModel(DatModelFileBundle modelFileBundle) {
      var dat = modelFileBundle.DatFile.Impl.ReadNew<Dat>(Endianness.BigEndian);

      var finModel = new ModelImpl();

      var boneQueue = new Queue<(IBone finParentBone, JObj datBone)>();
      foreach (var datRootBone in dat.RootJObjs) {
        boneQueue.Enqueue((finModel.Skeleton.Root, datRootBone));
      }

      while (boneQueue.Count > 0) {
        var (finParentBone, datBone) = boneQueue.Dequeue();

        var datBoneData = datBone.Data;

        var finBone =
            finParentBone.AddChild(datBoneData.Position.X,
                                   datBoneData.Position.Y,
                                   datBoneData.Position.Z)
                         .SetLocalRotationRadians(
                             datBoneData.RotationRadians.X,
                             datBoneData.RotationRadians.Y,
                             datBoneData.RotationRadians.Z)
                         .SetLocalScale(
                             datBoneData.Scale.X,
                             datBoneData.Scale.Y,
                             datBoneData.Scale.Z);

        foreach (var datChildBone in datBone.Children) {
          boneQueue.Enqueue((finBone, datChildBone));
        }
      }

      return finModel;
    }
  }
}