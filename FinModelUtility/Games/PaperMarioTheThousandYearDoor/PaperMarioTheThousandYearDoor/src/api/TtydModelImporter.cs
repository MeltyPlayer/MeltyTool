using fin.data.lazy;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;

using schema.binary;

using ttyd.schema.model;
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

      var ttydSceneGraphs = ttydModel.SceneGraphs;
      var ttydSceneGraphObjectTransforms = ttydModel.SceneGraphObjectTransforms;

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
      var sceneGraphQueue = new FinTuple2Queue<int, IBone>(
          (ttydSceneGraphs.Length - 1, finModel.Skeleton.Root));
      while (sceneGraphQueue.TryDequeue(
                 out var ttydSceneGraphIndex,
                 out var parentFinBone)) {
        var ttydSceneGraph = ttydSceneGraphs[ttydSceneGraphIndex];

        var matrix = this.GetSceneGraphObjectTransformMatrix_(
            ttydSceneGraphObjectTransforms.AsSpan(
                ttydSceneGraph.TransformBaseIndex,
                24));
        matrix.Decompose(out var position, out var quaternion, out var scale);
        var rotation = QuaternionUtil.ToEulerRadians(quaternion);

        var finBone
            = parentFinBone
              .AddChild(position.X, position.Y, position.Z)
              .SetLocalRotationRadians(rotation.X, rotation.Y, rotation.Z)
              .SetLocalScale(scale.X, scale.Y, scale.Z);
        finBone.Name = ttydSceneGraph.Name;

        if (ttydSceneGraph.SceneGraphObjectIndex != -1) {
          var ttydSceneGraphObject
              = ttydModel.SceneGraphObjects[
                  ttydSceneGraph.SceneGraphObjectIndex];
          var finMesh
              = finGroupVisibilityMeshesAndDefaultVisibility[
                  ttydSceneGraph.VisibilityGroupIndex].Item1;

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

                finVertices[i] = finVertex;
              }

              var finPrimitive = finMesh.AddTriangleFan(finVertices);
              if (finMaterial != null) {
                finPrimitive.SetMaterial(finMaterial);
              }
            }
          }
        }

        if (ttydSceneGraph.NextGroupIndex != -1) {
          sceneGraphQueue.Enqueue(
              (ttydSceneGraph.NextGroupIndex, parentFinBone));
        }

        if (ttydSceneGraph.ChildGroupIndex != -1) {
          sceneGraphQueue.Enqueue((ttydSceneGraph.ChildGroupIndex, finBone));
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

        // TODO: is this right?
        var length = ttydAnimationData.BaseInfos.SingleOrDefault()?.End ??
                     ttydAnimationData.Keyframes.Max(k => k.Time);
        finAnimation.FrameCount = (int) length;
        finAnimation.FrameRate = 60;

        foreach (var ttydKeyframe in ttydAnimationData.Keyframes) {
          // TODO: Usually ints, but some fractions... how to handle these?
          var keyframe = (int) ttydKeyframe.Time;

          var visibilityGroupDeltaCount = ttydKeyframe.VisibilityGroupDeltaCount;
          if (visibilityGroupDeltaCount > 0) {
            var visibilityGroupDeltas
                = ttydAnimationData.VisibilityGroupDeltas.AsSpan(
                    (int) ttydKeyframe.VisibilityGroupDeltaBaseIndex,
                    (int) visibilityGroupDeltaCount);

            foreach (var visibilityGroupDelta in visibilityGroupDeltas) {
              var finMeshTracks
                  = allFinMeshTracks[visibilityGroupDelta.VisibilityGroupId];
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

    private IReadOnlyFinMatrix4x4 GetSceneGraphObjectTransformMatrix_(
        ReadOnlySpan<float> sceneGraphObjectTransforms) {
      // Translate by v1
      var matrix = FinMatrix4x4Util.FromTranslation(
          sceneGraphObjectTransforms[0],
          sceneGraphObjectTransforms[1],
          sceneGraphObjectTransforms[2]);

      // Translate by v7
      matrix.MultiplyInPlace(
          FinMatrix4x4Util.FromTranslation(
              sceneGraphObjectTransforms[18],
              sceneGraphObjectTransforms[19],
              sceneGraphObjectTransforms[20]));

      // Scale by v2
      matrix.MultiplyInPlace(
          FinMatrix4x4Util.FromScale(
              sceneGraphObjectTransforms[3],
              sceneGraphObjectTransforms[4],
              sceneGraphObjectTransforms[5]));

      // Translate by -v8
      matrix.MultiplyInPlace(
          FinMatrix4x4Util
              .FromTranslation(
                  -sceneGraphObjectTransforms[21],
                  -sceneGraphObjectTransforms[22],
                  -sceneGraphObjectTransforms[23]));

      // Rotate by v4
      matrix.MultiplyInPlace(
          FinMatrix4x4Util.FromRotation(
              QuaternionUtil.CreateZyx(
                  float.DegreesToRadians(sceneGraphObjectTransforms[9]),
                  float.DegreesToRadians(sceneGraphObjectTransforms[10]),
                  float.DegreesToRadians(sceneGraphObjectTransforms[11]))));

      // Translate by v5
      matrix.MultiplyInPlace(
          FinMatrix4x4Util.FromTranslation(
              sceneGraphObjectTransforms[12],
              sceneGraphObjectTransforms[13],
              sceneGraphObjectTransforms[14]));

      // Rotate by 2 * v3
      matrix.MultiplyInPlace(
          FinMatrix4x4Util.FromRotation(
              QuaternionUtil.CreateZyx(
                  float.DegreesToRadians(2 * sceneGraphObjectTransforms[6]),
                  float.DegreesToRadians(2 * sceneGraphObjectTransforms[7]),
                  float.DegreesToRadians(2 * sceneGraphObjectTransforms[8]))));

      // Translate by -v6
      matrix.MultiplyInPlace(
          FinMatrix4x4Util
              .FromTranslation(
                  -sceneGraphObjectTransforms[15],
                  -sceneGraphObjectTransforms[16],
                  -sceneGraphObjectTransforms[17]));

      return matrix;
    }
  }
}