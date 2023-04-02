using fin.data;
using fin.data.queue;
using fin.io;
using fin.math;
using fin.model;
using fin.model.impl;

using level5.schema;

using System.Numerics;

using fin.data.lazy;
using fin.util.enumerables;


namespace level5.api {
  public class XcModelFileBundle : IModelFileBundle {
    public string? BetterName { get; set; }
    public required string GameName { get; init; }
    public IFileHierarchyFile MainFile => this.ModelXcFile;
    public IEnumerable<IGenericFile> Files
      => this.ModelXcFile.Yield()
             .ConcatIfNonnull(this.AnimationXcFiles);

    public IFileHierarchyFile ModelXcFile { get; set; }
    public IList<IFileHierarchyFile>? AnimationXcFiles { get; set; }
  }

  public class XcModelLoader : IModelLoader<XcModelFileBundle> {
    public IModel LoadModel(XcModelFileBundle modelFileBundle) {
      var endianness = Endianness.LittleEndian;

      var modelXcFile = modelFileBundle.ModelXcFile;
      var modelXc = modelXcFile.Impl.ReadNew<Xc>(endianness);
      var modelResourceFile =
          new Resource(modelXc.FilesByExtension[".bin"].Single().Data);

      var model = new ModelImpl();

      var finBoneByIndex = new Dictionary<uint, IBone>();
      var finBoneByName = new Dictionary<string, IBone>();
      {
        if (modelXc.FilesByExtension.TryGetList(".mbn", out var mbnFiles)) {
          var mbns = mbnFiles!.Select(mbnFile => {
                                using var er =
                                    new EndianBinaryReader(
                                        new MemoryStream(mbnFile.Data),
                                        endianness);
                                return er.ReadNew<Mbn>();
                              })
                              .ToArray();
          var mbnNodeList =
              mbns.Where(mbn => mbn.Id != mbn.ParentId)
                  .DistinctBy(mbn => mbn.Id)
                  .Select(mbn => new Node<Mbn> { Value = mbn })
                  .ToArray();
          var mbnByIndex =
              mbnNodeList.ToDictionary(node => node.Value.Id);

          foreach (var mbnNode in mbnNodeList) {
            var mbn = mbnNode.Value;
            if (mbn.ParentId == 0) {
              continue;
            }

            mbnNode.Parent = mbnByIndex[mbn.ParentId];
          }

          var rootMbnNodes =
              mbnNodeList.Where(node => node.Value.ParentId == 0);

          var mbnQueue =
              new FinTuple2Queue<Node<Mbn>, IBone>(
                  rootMbnNodes.Select(
                      node => (node, model.Skeleton.Root)));
          while (mbnQueue.TryDequeue(out var mbnNode, out var parentBone)) {
            var mbn = mbnNode.Value;

            var position = mbn.Position;

            var bone = parentBone.AddChild(position.X, position.Y, position.Z);
            bone.Name = modelResourceFile.GetResourceName(mbn.Id);

            var mat3 = mbn.RotationMatrix3;
            var matrix = new OpenTK.Matrix3(mat3[0],
                                            mat3[1],
                                            mat3[2],
                                            mat3[3],
                                            mat3[4],
                                            mat3[5],
                                            mat3[6],
                                            mat3[7],
                                            mat3[8]);
            var openTkQuaternion = matrix.ExtractRotation();
            var quaternion = new Quaternion(openTkQuaternion.X,
                                            openTkQuaternion.Y,
                                            openTkQuaternion.Z,
                                            openTkQuaternion.W);
            var eulerRadians = QuaternionUtil.ToEulerRadians(quaternion);
            bone.SetLocalRotationRadians(eulerRadians.X,
                                         eulerRadians.Y,
                                         eulerRadians.Z);

            var scale = mbn.Scale;
            bone.SetLocalScale(scale.X, scale.Y, scale.Z);

            finBoneByIndex[mbn.Id] = bone;
            finBoneByName[bone.Name] = bone;

            mbnQueue.Enqueue(
                mbnNode.Children.Select(childNode => (childNode, bone)));
          }
        }
      }

      var lazyTextures = new LazyDictionary<string, ITexture>(textureName => {
        var textureIndex = modelResourceFile.TextureNames.IndexOf(textureName);
        var xiFile = modelXc.FilesByExtension[".xi"][textureIndex];

        var xi = new Xi();
        xi.Open(xiFile.Data);

        var image = xi.ToBitmap();
        var texture = model.MaterialManager.CreateTexture(image);
        texture.Name = textureName;

        return texture;
      });

      var lazyMaterials = new LazyDictionary<string, IMaterial>(
          materialName => {
            var binMaterial =
                modelResourceFile.Materials.Single(
                    mat => mat.Name == materialName);
            var finTexture = lazyTextures[binMaterial.TexName];

            var finMaterial =
                model.MaterialManager.AddTextureMaterial(finTexture);
            finMaterial.Name = binMaterial.Name;

            // TODO: Figure out how to fix culling issues
            finMaterial.CullingMode = CullingMode.SHOW_BOTH;

            return finMaterial;
          });

      // Adds vertices
      Dictionary<uint, (Prm prm, IMesh mesh)> prmsByAnimationReferenceHash =
          new();
      {
        if (modelXc.FilesByExtension.TryGetList(".prm", out var prmFiles)) {
          foreach (var prmFile in prmFiles) {
            var prm = new Prm(prmFile.Data);

            var finMaterial = lazyMaterials[prm.MaterialName];

            var mesh = model.Skin.AddMesh();
            mesh.Name = prm.Name;

            var prmAnimationReferenceHashes = prm.AnimationReferenceHashes;
            foreach (var hash in prmAnimationReferenceHashes) {
              prmsByAnimationReferenceHash[hash] = (prm, mesh);
            }

            var finVertices = new List<IVertex>();
            foreach (var prmVertex in prm.Vertices) {
              var position = prmVertex.Pos;
              var finVertex =
                  model.Skin.AddVertex(position.X, position.Y, position.Z);

              var uv = prmVertex.Uv0;
              finVertex.SetUv(uv.X, uv.Y);

              var normal = prmVertex.Nrm;
              finVertex.SetLocalNormal(normal.X, normal.Y, normal.Z);

              var prmBones = prmVertex.Bones;
              if (prmBones != null) {
                var boneWeightList = new List<BoneWeight>();
                for (var b = 0; b < 4; ++b) {
                  var boneId = prmVertex.Bones[b];
                  var weight = prmVertex.Weights[b];

                  var finBone = finBoneByIndex[boneId];
                  boneWeightList.Add(new BoneWeight(finBone, null, weight));
                }

                var boneWeights =
                    model.Skin.GetOrCreateBoneWeights(
                        PreprojectMode.BONE,
                        boneWeightList.ToArray());
                finVertex.SetBoneWeights(boneWeights);
              }

              finVertices.Add(finVertex);
            }

            var finTriangleVertices = prm.Triangles
                                         .Select(vertexIndex =>
                                                     finVertices[
                                                         (int) vertexIndex])
                                         .ToArray();
            var triangles = mesh.AddTriangles(finTriangleVertices);
            triangles.SetMaterial(finMaterial);
          }
        }
      }

      // Adds animations
      {
        foreach (var animationXcFile in modelFileBundle.AnimationXcFiles) {
          var animationXc = animationXcFile?.Impl?.ReadNew<Xc>(endianness);
          var animationResourceFile =
              new Resource(animationXc.FilesByExtension[".bin"].Single().Data);

          if (animationXc.FilesByExtension.TryGetList(
                  ".mtn2",
                  out var mtn2Files)) {
            foreach (var mtn2File in mtn2Files) {
              var mtn2 = new Mtn2();
              mtn2.Open(mtn2File.Data);

              var anim = mtn2.Anim;

              var finAnimation = model.AnimationManager.AddAnimation();
              finAnimation.Name = anim.Name;
              finAnimation.FrameRate = 30;
              finAnimation.FrameCount = anim.FrameCount;

              foreach (var (animationReferenceHash, framesAndValues) in mtn2
                           .Somethings) {
                if (!prmsByAnimationReferenceHash.TryGetValue(
                        animationReferenceHash,
                        out var prmAndMesh)) {
                  continue;
                }

                var (prm, mesh) = prmAndMesh;
                var meshTracks = finAnimation.AddMeshTracks(mesh);
                var displayStates = meshTracks.DisplayStates;

                foreach (var frameAndValue in framesAndValues) {
                  var (frame, value) = frameAndValue;

                  // TODO: Various values may be used to encode texture info?
                  var bits = Convert.ToString(value, 2).PadLeft(16, '0');

                  // TODO: This is just a guess, but still doesn't look right.
                  var displayState = (MeshDisplayState?) null;
                  if (value == 257) {
                    displayState = MeshDisplayState.VISIBLE;
                  } else if (value == 0) {
                    displayState = MeshDisplayState.HIDDEN;
                  }

                  if (displayState != null) {
                    displayStates.Set(frame, displayState.Value);
                  }
                }
              }

              foreach (var transformNode in anim.TransformNodes) {
                if (!finBoneByIndex.TryGetValue(transformNode.Hash,
                                                out var finBone)) {
                  continue;
                }

                var finBoneTracks = finAnimation.AddBoneTracks(finBone);
                foreach (var mtnTrack in transformNode.Tracks.Values) {
                  foreach (var mtnKey in mtnTrack.Keys.Keys) {
                    var frame = (int) mtnKey.Frame;
                    var value = mtnKey.Value;

                    var inTan = mtnKey.InTan;
                    var outTan = mtnKey.OutTan;

                    switch (mtnTrack.Type) {
                      case AnimationTrackFormat.RotateX: {
                        finBoneTracks.Rotations.Set(
                            frame,
                            0,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.RotateY: {
                        finBoneTracks.Rotations.Set(
                            frame,
                            1,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.RotateZ: {
                        finBoneTracks.Rotations.Set(
                            frame,
                            2,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.ScaleX: {
                        finBoneTracks.Scales.Set(
                            frame,
                            0,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.ScaleY: {
                        finBoneTracks.Scales.Set(
                            frame,
                            1,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.ScaleZ: {
                        finBoneTracks.Scales.Set(
                            frame,
                            2,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.TranslateX: {
                        finBoneTracks.Positions.Set(
                            frame,
                            0,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.TranslateY: {
                        finBoneTracks.Positions.Set(
                            frame,
                            1,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      case AnimationTrackFormat.TranslateZ: {
                        finBoneTracks.Positions.Set(
                            frame,
                            2,
                            value,
                            inTan,
                            outTan);
                        break;
                      }
                      default:
                        throw new NotSupportedException();
                    }
                  }
                }
              }
            }
          }
        }
      }

      return model;
    }
  }
}