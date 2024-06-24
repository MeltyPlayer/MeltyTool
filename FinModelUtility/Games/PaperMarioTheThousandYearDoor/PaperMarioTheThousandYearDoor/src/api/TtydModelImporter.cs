using System.Numerics;

using fin.data.dictionaries;
using fin.data.indexable;
using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using schema.binary;

using ttyd.schema.model;
using ttyd.schema.model.blocks;
using ttyd.schema.tpl;

namespace ttyd.api {
  public class TtydModelFileBundle : IModelFileBundle {
    public string GameName => "paper_mario_the_thousand_year_door";

    public required IReadOnlyTreeFile ModelFile { get; init; }
    public IReadOnlyTreeFile MainFile => this.ModelFile;
  }

  public class TtydModelImporter : IModelImporter<TtydModelFileBundle> {
    public IModel Import(TtydModelFileBundle fileBundle) {
      var modelFile = fileBundle.ModelFile;
      var ttydModel = modelFile.ReadNew<Model>(Endianness.BigEndian);

      var textureFile = modelFile.AssertGetParent()
                                 .AssertGetExistingFile(
                                     $"{ttydModel.Header.TextureFileName}-");
      var tpl = textureFile.ReadNew<Tpl>(Endianness.BigEndian);

      var ttydGroups = ttydModel.Groups;
      var ttydGroupTransforms = ttydModel.GroupTransforms;
      var ttydGroupToParent = new Dictionary<Group, Group>();
      var ttydGroupToChildren = new SetDictionary<Group, (Group, IReadOnlyBone)>();

      var finModel = new ModelImpl {
          FileBundle = fileBundle,
          Files = new HashSet<IReadOnlyGenericFile>([modelFile, textureFile])
      };

      // Sets up materials
      var finTextureMap = new LazyDictionary<int, ITexture?>(
          texMapIndex => {
            if (texMapIndex == -1) {
              return null;
            }

            var ttydTextureIndex
                = ttydModel.TextureMaps[texMapIndex].TextureIndex;
            var ttydTexture = ttydModel.Textures[ttydTextureIndex];

            var tplTextureIndex = ttydTexture.TplTextureIndex;
            var tplTexture = tpl.Textures[tplTextureIndex];

            var finTexture
                = finModel.MaterialManager.CreateTexture(tplTexture.Image);
            finTexture.Name = ttydTexture.Name;

            return finTexture;
          });
      var finMaterialMap = new LazyDictionary<int, IMaterial?>(
          texMapIndex => {
            var finTexture = finTextureMap[texMapIndex];
            if (finTexture == null) {
              return null;
            }

            var finMaterial
                = finModel.MaterialManager.AddTextureMaterial(finTexture);
            finMaterial.CullingMode = CullingMode.SHOW_BOTH;

            return finMaterial;
          });

      // Sets up meshes for each group visibility
      var finGroupVisibilityMeshesAndDefaultVisibility
          = ttydModel.GroupVisibilities
                     .Select(visible => (finModel.Skin.AddMesh(), visible))
                     .ToArray();

      // Adds bones/meshes
      var groupsAndBones = new (Group, IReadOnlyBone)[ttydGroups.Length];

      var groupAndBoneQueue = new FinTuple3Queue<int, Group?, IBone>(
          (ttydGroups.Length - 1, null, finModel.Skeleton.Root));
      while (groupAndBoneQueue.TryDequeue(
                 out var ttydGroupIndex,
                 out var ttydParentGroup,
                 out var parentFinBone)) {
        var ttydGroup = ttydGroups[ttydGroupIndex];

        var matrix = TtydGroupTransformUtils.GetTransformMatrix(
            ttydGroup,
            ttydGroupToParent,
            ttydGroupTransforms);
        
        var finBone = parentFinBone.AddChild(matrix);
        finBone.Name = ttydGroup.Name;
        groupsAndBones[ttydGroupIndex] = (ttydGroup, finBone);

        if (ttydParentGroup != null) {
          ttydGroupToParent[ttydGroup] = ttydParentGroup;
          ttydGroupToChildren.Add(ttydParentGroup, (ttydGroup, finBone));
        }

        var boneWeights = finModel.Skin.GetOrCreateBoneWeights(
            VertexSpace.RELATIVE_TO_BONE,
            finBone);

        if (ttydGroup.SceneGraphObjectIndex != -1) {
          var ttydSceneGraphObject
              = ttydModel.SceneGraphObjects[
                  ttydGroup.SceneGraphObjectIndex];
          var finMesh
              = finGroupVisibilityMeshesAndDefaultVisibility[
                  ttydGroup.VisibilityGroupIndex].Item1;

          var objectPositions = ttydModel.Vertices.AsSpan(
              ttydSceneGraphObject.VertexPositionBaseIndex);
          var objectNormals = ttydModel.Normals.AsSpan(
              ttydSceneGraphObject.VertexNormalBaseIndex);
          var objectColors = ttydModel.Colors.AsSpan(
              ttydSceneGraphObject.VertexColorBaseIndex);
          var objectTexCoords = ttydModel.TexCoords.AsSpan(
              ttydSceneGraphObject.VertexTexCoordBaseIndex);

          var ttydMeshes = ttydModel.Meshes.AsSpan(
              ttydSceneGraphObject.MeshBaseIndex,
              ttydSceneGraphObject.MeshCount);
          foreach (var ttydMesh in ttydMeshes) {
            if (ttydMesh.PolygonBaseIndex == -1) {
              continue;
            }

            var finMaterial = finMaterialMap[ttydMesh.SamplerIndex];

            var ttydPolygons
                = ttydModel.Polygons.AsSpan(ttydMesh.PolygonBaseIndex,
                                            ttydMesh.PolygonCount);
            foreach (var ttydPolygon in ttydPolygons) {
              var finVertices = new IVertex[ttydPolygon.VertexCount];
              for (var i = 0; i < ttydPolygon.VertexCount; i++) {
                var vertexPosition = objectPositions[
                    ttydModel.VertexIndices[
                        ttydMesh.VertexPositionBaseIndex +
                        ttydPolygon.VertexBaseIndex +
                        i]];
                var vertexNormal = objectNormals[
                    ttydModel.NormalIndices[
                        ttydMesh.VertexNormalBaseIndex +
                        ttydPolygon.VertexBaseIndex +
                        i]];
                var vertexColor = objectColors[
                    ttydModel.ColorIndices[
                        ttydMesh.VertexColorBaseIndex +
                        ttydPolygon.VertexBaseIndex +
                        i]];
                var vertexTexCoord = objectTexCoords[
                    ttydModel.TexCoordIndices[
                        ttydMesh.VertexTexCoordBaseIndex +
                        ttydPolygon.VertexBaseIndex +
                        i]];

                var finVertex = finModel.Skin.AddVertex(vertexPosition);
                finVertex.SetLocalNormal(vertexNormal);
                finVertex.SetColor(vertexColor);
                finVertex.SetUv(vertexTexCoord);
                finVertex.SetBoneWeights(boneWeights);

                finVertices[i] = finVertex;
              }

              var finPrimitive = finMesh.AddTriangleFan(finVertices);
              if (finMaterial != null) {
                finPrimitive.SetMaterial(finMaterial);
              }
            }
          }
        }

        if (ttydGroup.NextGroupIndex != -1) {
          groupAndBoneQueue.Enqueue(
              (ttydGroup.NextGroupIndex, ttydParentGroup, parentFinBone));
        }

        if (ttydGroup.ChildGroupIndex != -1) {
          groupAndBoneQueue.Enqueue((ttydGroup.ChildGroupIndex, ttydGroup,
                                     finBone));
        }
      }

      // Sets up animations
      foreach (var ttydAnimation in ttydModel.Animations) {
        var ttydAnimationData = ttydAnimation.Data;
        if (ttydAnimationData == null) {
          continue;
        }

        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = ttydAnimation.Name;

        // TODO: is this right?
        var length = ttydAnimationData.BaseInfos.SingleOrDefault()?.End ??
                     ttydAnimationData.Keyframes.Max(k => k.Time);
        finAnimation.FrameCount = (int) length;
        finAnimation.FrameRate = 60;

        var finBoneTracksByBone
            = new IndexableDictionary<IReadOnlyBone, (
                ICombinedPositionAxesTrack3d, IQuaternionRotationTrack3d,
                IScale3dTrack)>();
        foreach (var (_, finBone) in groupsAndBones) {
          var finBoneTracks = finAnimation.AddBoneTracks(finBone);

          var positionsTrack = finBoneTracks.UseCombinedPositionAxesTrack();
          var rotationsTrack
              = finBoneTracks.UseQuaternionRotationTrack();
          var scalesTrack = finBoneTracks.UseScaleTrack();

          finBoneTracksByBone[finBone]
              = (positionsTrack, rotationsTrack, scalesTrack);
        }

        var allFinMeshTracks
            = finGroupVisibilityMeshesAndDefaultVisibility
              .Select(tuple => {
                        var (finMesh, visible) = tuple;

                        var finMeshTracks
                            = finAnimation.AddMeshTracks(
                                finMesh);
                        finMeshTracks.DisplayStates
                                     .SetKeyframe(
                                         0,
                                         visible
                                             ? MeshDisplayState.VISIBLE
                                             : MeshDisplayState.HIDDEN);

                        return finMeshTracks;
                      })
              .ToArray();

        var animatedGroupTransformValues
            = ttydModel.GroupTransforms.ToArray();

        foreach (var ttydKeyframe in ttydAnimationData.Keyframes) {
          // TODO: Usually ints, but some fractions... how to handle these?
          var keyframe = (int) ttydKeyframe.Time;

          // Sets up transform animations
          // TODO: Hopefully this works????
          var groupTransformDataDeltaCount
              = ttydKeyframe.GroupTransformDataDeltaCount;
          if (groupTransformDataDeltaCount > 0) {
            var groupTransformDataDeltas =
                ttydAnimationData.GroupTransformDataDeltas.AsSpan(
                    (int) ttydKeyframe.GroupTransformDataDeltaBaseIndex,
                    (int) groupTransformDataDeltaCount);

            var affectedSetThisFrame = new HashSet<(Group, IReadOnlyBone)>();
            var groupTransformIndexAccumulator = 0;

            if (keyframe == 0) {
              affectedSetThisFrame.Add(groupsAndBones);
            }

            foreach (var groupTransformDataDelta in groupTransformDataDeltas) {
              groupTransformIndexAccumulator
                  += groupTransformDataDelta.IndexDelta;

              // TODO: Handle tangents
              // TODO: Pull this out into a separate class

              var deltaValue = groupTransformDataDelta.ValueDelta / 16f;
              animatedGroupTransformValues[groupTransformIndexAccumulator]
                  += deltaValue;

              // TODO: This is the stupidest way to do this, do this better
              foreach (var (ttydGroup, finBone) in groupsAndBones) {
                var transformBaseIndex = ttydGroup.TransformBaseIndex;
                var transformCount = 24;

                if (transformBaseIndex >= groupTransformIndexAccumulator &&
                    groupTransformIndexAccumulator <
                    transformBaseIndex + transformCount) {
                  affectedSetThisFrame.Add((ttydGroup, finBone));

                  if (ttydGroupToChildren.TryGetSet(ttydGroup, out var ttydChildren)) {
                    foreach (var ttydChild in ttydChildren) {
                      affectedSetThisFrame.Add(ttydChild);
                    }
                  }
                }
              }
            }

            foreach (var (ttydGroup, finBone) in affectedSetThisFrame) {
              var matrix = TtydGroupTransformUtils.GetTransformMatrix(
                  ttydGroup,
                  ttydGroupToParent,
                  animatedGroupTransformValues);
              Matrix4x4.Decompose(matrix,
                                  out var scale,
                                  out var quaternion,
                                  out var position);

              var (positionsTrack, rotationsTrack, scalesTrack)
                  = finBoneTracksByBone[finBone];

              positionsTrack.SetKeyframe(
                  keyframe,
                  new Position(position.X, position.Y, position.Z));
              rotationsTrack.SetKeyframe(keyframe, quaternion);
              scalesTrack.Set(keyframe, scale);
            }
          }

          // Sets up visibility animations
          var visibilityIndexAccumulator = 0;

          var visibilityGroupDeltaCount
              = ttydKeyframe.VisibilityGroupDeltaCount;
          if (visibilityGroupDeltaCount > 0) {
            var visibilityGroupDeltas
                = ttydAnimationData.VisibilityGroupDeltas.AsSpan(
                    (int) ttydKeyframe.VisibilityGroupDeltaBaseIndex,
                    (int) visibilityGroupDeltaCount);

            foreach (var visibilityGroupDelta in visibilityGroupDeltas) {
              visibilityIndexAccumulator
                  += visibilityGroupDelta.VisibilityGroupId;

              var finMeshTracks
                  = allFinMeshTracks[visibilityIndexAccumulator];
              finMeshTracks.DisplayStates.SetKeyframe(
                  keyframe,
                  visibilityGroupDelta.Visible
                      ? MeshDisplayState.VISIBLE
                      : MeshDisplayState.HIDDEN);
            }
          }
        }
      }

      return finModel;
    }
  }
}