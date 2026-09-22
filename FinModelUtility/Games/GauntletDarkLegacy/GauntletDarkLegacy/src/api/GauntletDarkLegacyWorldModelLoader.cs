using System.Numerics;

using fin.data.queues;
using fin.image;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;

using gdl.schema.anim;
using gdl.schema.objects;
using gdl.schema.worlds;

using schema.binary;


namespace gdl.api;

public sealed class GauntletDarkLegacyWorldModelFileBundle
    : IModelFileBundle<GauntletDarkLegacyWorldModelFileBundle,
        GauntletDarkLegacyWorldModelImporter> {
  public required IReadOnlyTreeFile WorldsFile { get; init; }
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public IReadOnlyTreeFile MainFile => this.ObjectsFile;

  public bool UseSubMeshes { get; init; }
}

public sealed class GauntletDarkLegacyWorldModelImporter
    : IModelImporter<GauntletDarkLegacyWorldModelImporter,
        GauntletDarkLegacyWorldModelFileBundle> {
  public IModel Import(GauntletDarkLegacyWorldModelFileBundle fileBundle) {
    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();

    return ImportImpl(
        fileBundle,
        new HashSet<IReadOnlyGenericFile>([
            fileBundle.WorldsFile,
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ]),
        fileBundle.WorldsFile.ReadNew<Worlds>(),
        objects,
        anim,
        fileBundle.TexturesFile,
        new Dictionary<int, IReadOnlyImage>());
  }

  public static IModel ImportImpl(
      GauntletDarkLegacyWorldModelFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files,
      Worlds worlds,
      Objects objects,
      Anim anim,
      IReadOnlyGenericFile texturesFile,
      Dictionary<int, IReadOnlyImage> textureImageCache) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var textureBr = texturesFile.OpenReadAsBinary(Endianness.BigEndian);
    var lazyFinMaterials =
        new LazyGdlMaterials(finModel, textureImageCache, textureBr, objects);

    var objectByName = objects.ObjectDefinitions.Zip(objects.RootObjects)
                              .ToDictionary(k => k.First.Name);

    var worldObjectQueue
        = new FinTuple2Queue<short, Vector3>((0, Vector3.Zero));
    while (worldObjectQueue.TryDequeue(out var worldObjectIndex,
                                       out var parentTranslation)) {
      var gdlWorldObject = worlds.WorldObjects[worldObjectIndex];

      var position = gdlWorldObject.Position with {
          X = -gdlWorldObject.Position.X
      };

      var translation = parentTranslation + position;

      if (gdlWorldObject.ChildIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.ChildIndex, translation));
      }
      if (gdlWorldObject.NextIndex != -1) {
        worldObjectQueue.Enqueue((gdlWorldObject.NextIndex, parentTranslation));
      }

      if (!objectByName.TryGetValue(gdlWorldObject.Name, out var tuple)) {
        continue;
      }

      var (definition, obj) = tuple;
      var mbFlags = gdlWorldObject.MbFlags;

      IBoneWeights? boneWeights = null;
      Matrix4x4 translationMatrix = Matrix4x4.Identity;
      if (mbFlags.CheckFlag(MbFlags.YAW_ONLY_BILLBOARD) ||
          mbFlags.CheckFlag(MbFlags.YAW_AND_PITCH_BILLBOARD)) {
        var finBone = finModel.Skeleton.Root.AddChild(translation);
        GauntletDarkLegacyModelImporter.SetFaceTowardsCamera(
            finBone,
            mbFlags);

        boneWeights = finModel.Skin.GetOrCreateBoneWeights(
            VertexSpace.RELATIVE_TO_BONE,
            finBone);
      } else {
        translationMatrix = Matrix4x4.CreateTranslation(translation);
      }

      MeshUtil.AddObjectMesh(finModel,
                             boneWeights,
                             translationMatrix,
                             definition,
                             obj,
                             lazyFinMaterials,
                             mbFlags,
                             fileBundle.UseSubMeshes,
                             out _);
    }

    return finModel;
  }
}