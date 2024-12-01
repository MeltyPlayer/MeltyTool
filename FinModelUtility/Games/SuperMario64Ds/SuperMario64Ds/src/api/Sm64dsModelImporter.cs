using System.Collections.Specialized;

using fin.compression;
using fin.data.dictionaries;
using fin.data.queues;
using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.util.dictionaries;
using fin.util.sets;

using schema.binary;

using sm64ds.schema;

namespace sm64ds.api;

public class Sm64dsModelImporter : IModelImporter<Sm64dsModelFileBundle> {
  public IModel Import(Sm64dsModelFileBundle fileBundle) {
    var bmdFile = fileBundle.BmdFile;

    var files = bmdFile.AsFileSet();
    var model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    var ms = new MemoryStream(
        new Lz77Decompressor().Decompress(bmdFile.OpenReadAsBinary()));
    var br = new SchemaBinaryReader(ms);
    var bmd = br.ReadNew<Bmd>();

    {
      var bones = bmd.Bones.OrderBy(b => b.Id).ToArray();
      var rootBones = new List<Bone>();
      var boneToChildMap = new SetDictionary<Bone, Bone>();
      var nextSiblingMap = new Dictionary<Bone, Bone>();
      foreach (var bone in bones) {
        var offsetToParentBone = bone.OffsetToParentBone;
        if (offsetToParentBone != 0) {
          boneToChildMap.Add(bones[bone.Id + offsetToParentBone], bone);
        } else {
          rootBones.Add(bone);
        }

        var offsetToNextSibling = bone.OffsetToNextSiblingBone;
        if (offsetToNextSibling != 0) {
          nextSiblingMap[bone] = bones[bone.Id + offsetToNextSibling];
        }
      }

      var previousSiblingMap = nextSiblingMap.SwapKeysAndValues();

      var boneQueue = new FinTuple2Queue<Bone, IBone>(
          rootBones.Select(b => (b, model.Skeleton.Root)));
      while (boneQueue.TryDequeue(out var bone, out var parentFinBone)) {
        var finBone = parentFinBone.AddChild(bone.Translation);
        finBone.Name = bone.Name;

        var localTransform = finBone.LocalTransform;
        localTransform.SetRotationDegrees(bone.Rotation);
        localTransform.SetScale(bone.Scale);

        if (boneToChildMap.TryGetSet(bone, out var unorderedChildren)) {
          var firstChild
              = unorderedChildren!.Single(
                  b => !previousSiblingMap.ContainsKey(b));
          boneQueue.Enqueue(nextSiblingMap.Chain(firstChild)
                                          .Select(b => (b, finBone)));
        }
      }
    }

    return model;
  }
}