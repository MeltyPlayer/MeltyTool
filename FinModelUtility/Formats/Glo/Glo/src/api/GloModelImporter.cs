using System.Numerics;

using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.image.formats;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.util.asserts;
using fin.util.image;

using glo.schema;

using SixLabors.ImageSharp.PixelFormats;

namespace glo.api {
  public class GloModelImporter : IModelImporter<GloModelFileBundle> {
    private readonly string[] hiddenNames_ = ["Box01", "puzzle"];

    private readonly string[] mirrorTextures_ = ["Badg2.bmp"];

    public IModel Import(GloModelFileBundle gloModelFileBundle) {
      var gloFile = gloModelFileBundle.GloFile;
      var textureDirectories = gloModelFileBundle.TextureDirectories;
      var fps = 20;

      var glo = gloFile.ReadNew<Glo>();

      var textureFilesByName
          = new CaseInvariantStringDictionary<IReadOnlyTreeFile>();
      foreach (var textureDirectory in textureDirectories) {
        foreach (var textureFile in textureDirectory.GetExistingFiles()) {
          if (FinImage.IsSupportedFileType(textureFile)) {
            textureFilesByName[textureFile.NameWithoutExtension] =
                textureFile;
          }
        }
      }

      /*new MeshCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "mesh.csv")));
      new FaceCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "face.csv")));
      new VertexCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "vertex.csv")));*/

      var finModel = new ModelImpl<Normal1Color1UvVertexImpl>(
          (index, position) => new Normal1Color1UvVertexImpl(index, position));
      var finSkin = finModel.Skin;

      var finRootBone = finModel.Skeleton.Root;

      var finTextureMap = new LazyCaseInvariantStringDictionary<ITexture?>(
          textureFilename => {
            if (!textureFilesByName.TryGetValue(
                    Path.GetFileNameWithoutExtension(textureFilename),
                    out var textureFile)) {
              return null;
            }

            using var rawTextureImage = FinImage.FromFile(textureFile);
            var textureImageWithAlpha =
                GloModelImporter.AddTransparencyToGloImage_(rawTextureImage);

            var finTexture = finModel.MaterialManager.CreateTexture(
                textureImageWithAlpha);
            finTexture.Name = textureFilename;

            if (this.mirrorTextures_.Contains(textureFilename)) {
              finTexture.WrapModeU = WrapMode.MIRROR_REPEAT;
              finTexture.WrapModeV = WrapMode.MIRROR_REPEAT;
            } else {
              finTexture.WrapModeU = WrapMode.REPEAT;
              finTexture.WrapModeV = WrapMode.REPEAT;
            }

            return finTexture;
          });
      var finMaterialMap
          = new LazyDictionary<(IReadOnlyTexture? finTexture,
              TransparencyType meshTransparencyType, bool withCulling),
              IMaterial>(
              tuple => {
                var (finTexture, meshTransparencyType, withCulling) = tuple;

                IMaterial finMaterial;
                if (finTexture == null) {
                  finMaterial = finModel.MaterialManager.AddStandardMaterial();
                } else {
                  finMaterial
                      = finModel.MaterialManager.AddTextureMaterial(finTexture);
                }

                if (withCulling) {
                  finMaterial.CullingMode = CullingMode.SHOW_BOTH;
                }

                finMaterial.TransparencyType = meshTransparencyType.Merge(
                    finTexture?.TransparencyType ?? TransparencyType.OPAQUE);

                return finMaterial;
              });

      var firstMeshMap = new CaseInvariantStringDictionary<GloMesh>();

      // TODO: Consider separating these out as separate models
      foreach (var gloObject in glo.Objects) {
        var finObjectRootBone = finRootBone.AddRoot(0, 0, 0);
        var meshQueue = new FinTuple2Queue<GloMesh, IBone>(
            gloObject.Meshes.Select(topLevelGloMesh
                                        => (topLevelGloMesh,
                                            finObjectRootBone)));

        List<(IModelAnimation, int, int)> finAndGloAnimations = [];
        foreach (var animSeg in gloObject.AnimSegs) {
          var startFrame = (int) animSeg.StartFrame;
          var endFrame = (int) animSeg.EndFrame;

          var finAnimation = finModel.AnimationManager.AddAnimation();
          finAnimation.Name = animSeg.Name;
          finAnimation.FrameCount =
              (int) (animSeg.EndFrame - animSeg.StartFrame + 1);

          finAnimation.FrameRate = fps * animSeg.Speed;

          finAndGloAnimations.Add((finAnimation, startFrame, endFrame));
        }

        while (meshQueue.TryDequeue(out var gloMesh, out var parentFinBone)) {
          var name = gloMesh.Name;

          GloMesh idealMesh;
          if (!firstMeshMap.TryGetValue(name, out idealMesh)) {
            firstMeshMap[name] = idealMesh = gloMesh;
          }

          var meshTransparencyType
              = TransparencyTypeUtil.GetTransparencyType(
                  idealMesh.MeshTranslucency);

          var position = gloMesh.MoveKeys[0].Xyz;

          var rotation = gloMesh.RotateKeys[0];
          var scale = gloMesh.ScaleKeys[0].Scale;

          var finBone = parentFinBone
                        .AddChild(position.X, position.Y, position.Z)
                        .SetLocalRotationRadians(
                            rotation.X,
                            rotation.Y,
                            rotation.Z)
                        // This is weird, but seems to be right for levels.
                        .SetLocalScale(scale.Y, scale.X, scale.Z);
          finBone.Name = name + "_bone";
          finBone.IgnoreParentScale = true;

          var child = gloMesh.Pointers.Child;
          if (child != null) {
            meshQueue.Enqueue((child, finBone));
          }

          var next = gloMesh.Pointers.Next;
          if (next != null) {
            meshQueue.Enqueue((next, parentFinBone));
          }

          foreach (var (finAnimation, startFrame, endFrame) in
                   finAndGloAnimations) {
            var finBoneTracks = finAnimation.AddBoneTracks(finBone);

            var positions =
                finBoneTracks.UseCombinedPositionAxesTrack(
                    gloMesh.MoveKeys.Length);
            long prevTime = -1;
            foreach (var moveKey in gloMesh.MoveKeys) {
              Asserts.True(moveKey.Time > prevTime);
              prevTime = moveKey.Time;

              var isLast = false;
              int time;
              if (moveKey.Time < startFrame) {
                time = 0;
              } else if (moveKey.Time > endFrame) {
                time = endFrame - startFrame;
                isLast = true;
              } else {
                time = (int) (moveKey.Time - startFrame);
                isLast = moveKey.Time == endFrame;
              }

              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              positions.SetKeyframe(time,
                                    new Position(moveKey.Xyz.X,
                                                 moveKey.Xyz.Y,
                                                 moveKey.Xyz.Z));

              if (isLast) {
                break;
              }
            }

            var rotations =
                finBoneTracks.UseQuaternionRotationTrack(
                    gloMesh.RotateKeys.Length);
            prevTime = -1;
            foreach (var rotateKey in gloMesh.RotateKeys) {
              Asserts.True(rotateKey.Time > prevTime);
              prevTime = rotateKey.Time;

              var isLast = false;
              int time;
              if (rotateKey.Time < startFrame) {
                time = 0;
              } else if (rotateKey.Time > endFrame) {
                time = endFrame - startFrame;
                isLast = true;
              } else {
                time = (int) (rotateKey.Time - startFrame);
                isLast = rotateKey.Time == endFrame;
              }

              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              var quaternionKey =
                  new Quaternion(rotateKey.X,
                                 rotateKey.Y,
                                 rotateKey.Z,
                                 rotateKey.W);
              rotations.SetKeyframe(time, quaternionKey);

              if (isLast) {
                break;
              }
            }

            var scales = finBoneTracks.UseScaleTrack(gloMesh.ScaleKeys.Length);
            prevTime = -1;
            foreach (var scaleKey in gloMesh.ScaleKeys) {
              Asserts.True(scaleKey.Time > prevTime);
              prevTime = scaleKey.Time;

              var isLast = false;
              int time;
              if (scaleKey.Time < startFrame) {
                time = 0;
              } else if (scaleKey.Time > endFrame) {
                time = endFrame - startFrame;
                isLast = true;
              } else {
                time = (int) (scaleKey.Time - startFrame);
                isLast = scaleKey.Time == endFrame;
              }

              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              // TODO: Does this also need to be out of order?
              scales.Set(time, scaleKey.Scale);

              if (isLast) {
                break;
              }
            }
          }

          // Anything with these names are debug objects and can be ignored.
          if (this.hiddenNames_.Contains(name)) {
            if (idealMesh.Sprites.Length > 0) {
              ;
            }

            continue;
          }

          var finMesh = finSkin.AddMesh();
          finMesh.Name = name;

          var gloVertices = idealMesh.Vertices;

          string previousTextureName = null;
          IMaterial? previousMaterial = null;

          foreach (var gloFace in idealMesh.Faces) {
            // TODO: What can we do if texture filename is empty?
            var textureFilename = gloFace.TextureFilename;

            var gloFaceColor = gloFace.Color;
            var finFaceColor = FinColor.FromRgbaBytes(
                gloFaceColor.Rb,
                gloFaceColor.Gb,
                gloFaceColor.Bb,
                gloFaceColor.Ab);

            var enableBackfaceCulling = (gloFace.Flags & 1 << 2) == 0;

            finFaceColor
                = FinColor.FromRgbaFloats(1, 1, 1, gloMesh.MeshTranslucency);

            IMaterial? finMaterial;
            if (textureFilename == previousTextureName) {
              finMaterial = previousMaterial;
            } else {
              previousTextureName = textureFilename;
              finMaterial = finMaterialMap[
                  (finTextureMap[textureFilename],
                   meshTransparencyType,
                   enableBackfaceCulling)];
              previousMaterial = finMaterial;
            }

            // Face flag:
            // 0: potentially some kind of repeat mode??

            var color = (gloFace.Flags & 1 << 6) != 0
                ? FinColor.FromRgbaBytes(255, 0, 0, 255)
                : FinColor.FromRgbaBytes(0, 255, 0, 255);

            var finFaceVertices = new IReadOnlyVertex[3];
            for (var v = 0; v < 3; ++v) {
              var gloVertexRef = gloFace.VertexRefs[v];
              var gloVertex = gloVertices[gloVertexRef.Index];

              var finVertex = finSkin.AddVertex(gloVertex);
              finVertex.SetUv(gloVertexRef.U, gloVertexRef.V);
              finVertex.SetColor(finFaceColor);
              finVertex.SetBoneWeights(finSkin.GetOrCreateBoneWeights(
                                           VertexSpace.RELATIVE_TO_BONE,
                                           finBone));
              finFaceVertices[v] = finVertex;
            }

            // TODO: Merge triangles together
            var finTriangles =
                new (IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)[1];
            finTriangles[0] = (finFaceVertices[0], finFaceVertices[2],
                               finFaceVertices[1]);
            finMesh.AddTriangles(finTriangles).SetMaterial(finMaterial!);
          }

          foreach (var gloSprite in idealMesh.Sprites) {
            var gloSpritePosition = gloSprite.SpritePosition;

            var finSpriteBone = finBone.AddChild(
                gloSpritePosition.X,
                gloSpritePosition.Y,
                gloSpritePosition.Z);
            finSpriteBone.AlwaysFaceTowardsCamera(Quaternion.Identity);
            var finSpriteBoneWeights =
                finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                               finSpriteBone);

            var textureFilename = gloSprite.TextureFilename;

            var gloFaceColor = gloSprite.Color;
            var finFaceColor = FinColor.FromRgbaBytes(
                gloFaceColor.Rb,
                gloFaceColor.Gb,
                gloFaceColor.Bb,
                gloFaceColor.Ab);

            finFaceColor
                = FinColor.FromRgbaFloats(1, 1, 1, gloMesh.MeshTranslucency);

            IMaterial? finMaterial;
            if (textureFilename == previousTextureName) {
              finMaterial = previousMaterial;
            } else {
              previousTextureName = textureFilename;
              finMaterial = finMaterialMap[
                  (finTextureMap[textureFilename],
                   meshTransparencyType,
                   true)];
              previousMaterial = finMaterial;
            }

            var w = gloSprite.SpriteSize.X / 4;
            var h = gloSprite.SpriteSize.Y / 4;

            var v1 = finSkin.AddVertex(0, -h / 2, -w / 2);
            v1.SetUv(0, 0);
            v1.SetColor(finFaceColor);
            v1.SetBoneWeights(finSpriteBoneWeights);

            var v2 = finSkin.AddVertex(0, -h / 2, w / 2);
            v2.SetUv(1, 0);
            v1.SetColor(finFaceColor);
            v2.SetBoneWeights(finSpriteBoneWeights);

            var v3 = finSkin.AddVertex(0, h / 2, -w / 2);
            v3.SetUv(0, 1);
            v1.SetColor(finFaceColor);
            v3.SetBoneWeights(finSpriteBoneWeights);

            var v4 = finSkin.AddVertex(0, h / 2, w / 2);
            v4.SetUv(1, 1);
            v1.SetColor(finFaceColor);
            v4.SetBoneWeights(finSpriteBoneWeights);

            finMesh.AddTriangles((v1, v3, v2), (v4, v2, v3))
                   .SetMaterial(finMaterial!);
          }
        }

        // TODO: Split out animations
      }

      return finModel;
    }

    private static unsafe IImage AddTransparencyToGloImage_(IImage rawImage) {
      var width = rawImage.Width;
      var height = rawImage.Height;

      var textureImageWithAlpha =
          new Rgba32Image(rawImage.PixelFormat, width, height);
      using var alphaLock = textureImageWithAlpha.UnsafeLock();
      var alphaScan0 = alphaLock.pixelScan0;

      rawImage.Access(getHandler => {
                        for (var y = 0; y < height; ++y) {
                          for (var x = 0; x < width; ++x) {
                            getHandler(x,
                                       y,
                                       out var r,
                                       out var g,
                                       out var b,
                                       out var a);

                            if (r == 255 && g == 0 && b == 255) {
                              a = 0;
                            }

                            alphaScan0[y * width + x] = new Rgba32(r, g, b, a);
                          }
                        }
                      });

      return textureImageWithAlpha;
    }
  }
}