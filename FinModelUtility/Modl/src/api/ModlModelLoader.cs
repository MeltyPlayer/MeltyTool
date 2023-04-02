using System.Numerics;

using fin.data.lazy;
using fin.data.queue;
using fin.image;
using fin.io;
using fin.math;
using fin.model;
using fin.model.impl;
using fin.util.enumerables;

using modl.schema.anim;
using modl.schema.anim.bw1;
using modl.schema.anim.bw2;
using modl.schema.modl;
using modl.schema.modl.bw1;
using modl.schema.modl.bw1.node;
using modl.schema.modl.bw2;


namespace modl.api {
  public class ModlModelFileBundle : IBattalionWarsModelFileBundle {
    public required string GameName { get; init; }

    public IFileHierarchyFile MainFile => this.ModlFile;
    public IEnumerable<IGenericFile> Files
      => this.ModlFile.Yield().ConcatIfNonnull(this.AnimFiles);

    public required GameVersion GameVersion { get; init; }
    public required IFileHierarchyFile ModlFile { get; init; }

    public required IList<IFileHierarchyFile>? AnimFiles { get; init; }
  }

  public class ModlModelLoader : IAsyncModelLoader<ModlModelFileBundle> {
    public Task<IModel> LoadModelAsync(ModlModelFileBundle modelFileBundle)
      => LoadModelAsync(modelFileBundle.ModlFile.Impl,
                        modelFileBundle.AnimFiles
                                       ?.Select(file => file.Impl)
                                       .ToArray(),
                        modelFileBundle.GameVersion);

    public async Task<IModel> LoadModelAsync(
        IFile modlFile,
        IList<IFile>? animFiles,
        GameVersion gameVersion) {
      var flipSign = ModlFlags.FLIP_HORIZONTALLY ? -1 : 1;

      using var er = new EndianBinaryReader(modlFile.OpenRead(),
                                            Endianness.BigEndian);
      var bwModel = gameVersion switch {
          GameVersion.BW1 => (IModl) er.ReadNew<Bw1Modl>(),
          GameVersion.BW2 => er.ReadNew<Bw2Modl>(),
      };

      var model = new ModelImpl();
      var finMesh = model.Skin.AddMesh();

      var finBones = new IBone[bwModel.Nodes.Count];
      var finBonesByModlNode = new Dictionary<IBwNode, IBone>();
      var finBonesByIdentifier = new Dictionary<string, IBone>();

      {
        var nodeQueue =
            new FinTuple2Queue<IBone, ushort>((model.Skeleton.Root, 0));

        while (nodeQueue.TryDequeue(out var parentFinBone,
                                    out var modlNodeId)) {
          var modlNode = bwModel.Nodes[modlNodeId];

          var transform = modlNode.Transform;
          var bonePosition = transform.Position;

          var modlRotation = transform.Rotation;
          var rotation = new Quaternion(
              flipSign * modlRotation.X,
              modlRotation.Y,
              modlRotation.Z,
              flipSign * modlRotation.W);
          var eulerRadians = QuaternionUtil.ToEulerRadians(rotation);

          var finBone =
              parentFinBone
                  .AddChild(flipSign * bonePosition.X,
                            bonePosition.Y,
                            bonePosition.Z)
                  .SetLocalRotationRadians(
                      eulerRadians.X,
                      eulerRadians.Y,
                      eulerRadians.Z);

          var identifier = modlNode.GetIdentifier();
          finBone.Name = identifier;
          finBones[modlNodeId] = finBone;
          finBonesByModlNode[modlNode] = finBone;
          finBonesByIdentifier[identifier] = finBone;

          if (bwModel.CnctParentToChildren.TryGetList(
                  modlNodeId,
                  out var modlChildIds)) {
            nodeQueue.Enqueue(
                modlChildIds!.Select(modlChildId => (finBone, modlChildId)));
          }
        }

        foreach (var animFile in animFiles ?? Array.Empty<IFile>()) {
          var anim = gameVersion switch {
              GameVersion.BW1 => (IAnim) animFile.ReadNew<Bw1Anim>(
                  Endianness.BigEndian),
              GameVersion.BW2 => animFile.ReadNew<Bw2Anim>(
                  Endianness.BigEndian)
          };

          var maxFrameCount = -1;
          foreach (var animBone in anim.AnimBones) {
            maxFrameCount = (int) Math.Max(maxFrameCount,
                                           Math.Max(
                                               animBone
                                                   .PositionKeyframeCount,
                                               animBone
                                                   .RotationKeyframeCount));
          }

          var finAnimation = model.AnimationManager.AddAnimation();
          finAnimation.Name = animFile.NameWithoutExtension;
          finAnimation.FrameRate = 30;
          finAnimation.FrameCount = maxFrameCount;

          for (var b = 0; b < anim.AnimBones.Count; ++b) {
            var animBone = anim.AnimBones[b];
            var animBoneFrames = anim.AnimBoneFrames[b];

            var animNodeIdentifier = animBone.GetIdentifier();
            if (!finBonesByIdentifier.TryGetValue(
                    animNodeIdentifier,
                    out var finBone)) {
              // TODO: Gross hack for the vet models, what's the real fix???
              if (animNodeIdentifier == Bw1Node.GetIdentifier(33)) {
                finBone = finBonesByIdentifier[Bw1Node.GetIdentifier(34)];
              } else if (finBonesByIdentifier.TryGetValue(
                             animNodeIdentifier + 'X',
                             out var xBone)) {
                finBone = xBone;
              } else if (finBonesByIdentifier.TryGetValue(
                             "BONE_" + animNodeIdentifier,
                             out var prefixBone)) {
                finBone = prefixBone;
              } else if (animNodeIdentifier == "WF_GRUNT_BACKPAC") {
                // TODO: Is this right?????
                finBone = finBonesByIdentifier["BONE_BCK_MISC"];
              } else {
                ;
              }
            }

            var finBoneTracks = finAnimation.AddBoneTracks(finBone!);

            var fbtPositions = finBoneTracks.Positions;
            for (var f = 0; f < animBone.PositionKeyframeCount; ++f) {
              var (fPX, fPY, fPZ) = animBoneFrames.PositionFrames[f];

              fbtPositions.Set(f, 0, flipSign * fPX);
              fbtPositions.Set(f, 1, fPY);
              fbtPositions.Set(f, 2, fPZ);
            }

            var fbtRotations = finBoneTracks.Rotations;
            for (var f = 0; f < animBone.RotationKeyframeCount; ++f) {
              var (fRX, fRY, fRZ, frW) = animBoneFrames.RotationFrames[f];

              var animationQuaternion =
                  new Quaternion(flipSign * fRX, fRY, fRZ, flipSign * frW);
              var eulerRadians =
                  QuaternionUtil.ToEulerRadians(animationQuaternion);

              fbtRotations.Set(f, eulerRadians);
            }
          }
        }

        var textureDictionary = new LazyDictionary<string, Task<ITexture>>(
            async textureName => {
              var textureFile =
                  modlFile.GetParent()
                          .GetExistingFile($"{textureName}.png");

              var image = await FinImage.FromFileAsync(textureFile);

              var finTexture =
                  model.MaterialManager.CreateTexture(image);
              finTexture.Name = textureName;

              // TODO: Need to handle wrapping
              finTexture.WrapModeU = WrapMode.REPEAT;
              finTexture.WrapModeV = WrapMode.REPEAT;

              return finTexture;
            });

        foreach (var modlNode in bwModel.Nodes) {
          if (modlNode.IsLowLodModel) {
            continue;
          }

          var modlMaterials = modlNode.Materials;
          var finMaterials = new ITextureMaterial[modlMaterials.Count];
          await Task.WhenAll(modlMaterials.Select(async (modlMaterial, i) => {
            var textureName = modlMaterial.Texture1.ToLower();
            if (textureName == "") {
              return;
            }

            var finTexture = await textureDictionary[textureName];
            finMaterials[i] = model.MaterialManager
                                  .AddTextureMaterial(finTexture);
          })).ConfigureAwait(false);

          foreach (var modlMesh in modlNode.Meshes) {
            var finMaterial = finMaterials[modlMesh.MaterialIndex];

            foreach (var triangleStrip in modlMesh.TriangleStrips) {
              var vertices =
                  new IVertex[triangleStrip.VertexAttributeIndicesList.Count];
              for (var i = 0; i < vertices.Length; i++) {
                var vertexAttributeIndices =
                    triangleStrip.VertexAttributeIndicesList[i];

                var position =
                    modlNode.Positions[vertexAttributeIndices.PositionIndex];
                var vertex = vertices[i] = model.Skin.AddVertex(
                    flipSign * position.X * modlNode.Scale,
                    position.Y * modlNode.Scale,
                    position.Z * modlNode.Scale);

                if (vertexAttributeIndices.NormalIndex != null) {
                  var normal =
                      modlNode.Normals[
                          vertexAttributeIndices.NormalIndex.Value];
                  vertex.SetLocalNormal(flipSign * normal.X,
                                        normal.Y,
                                        normal.Z);
                }

                if (vertexAttributeIndices.NodeIndex != null) {
                  var finBone =
                      finBones[vertexAttributeIndices.NodeIndex.Value];
                  vertex.SetBoneWeights(
                      model.Skin
                           .GetOrCreateBoneWeights(
                               PreprojectMode.NONE,
                               new BoneWeight(finBone, null, 1)));
                } else {
                  var finBone = finBonesByModlNode[modlNode];
                  vertex.SetBoneWeights(
                      model.Skin.GetOrCreateBoneWeights(
                          PreprojectMode.BONE,
                          finBone));
                }

                var texCoordIndex0 = vertexAttributeIndices.TexCoordIndices[0];
                var texCoordIndex1 = vertexAttributeIndices.TexCoordIndices[1];
                if (texCoordIndex1 != null) {
                  int texCoordIndex;
                  if (texCoordIndex0 != null) {
                    texCoordIndex =
                        (texCoordIndex0.Value << 8) | texCoordIndex1.Value;
                  } else {
                    texCoordIndex = texCoordIndex1.Value;
                  }

                  var uv = modlNode.UvMaps[0][texCoordIndex];
                  vertex.SetUv(uv.U, uv.V);
                }
              }

              var triangleStripPrimitive = finMesh.AddTriangleStrip(vertices);
              if (finMaterial != null) {
                triangleStripPrimitive.SetMaterial(finMaterial);
              }
            }
          }
        }
      }

      return model;
    }
  }
}