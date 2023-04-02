using fin.io;
using fin.math;
using fin.model;
using fin.model.impl;
using glo.schema;
using System.Numerics;
using fin.color;
using fin.data.lazy;
using fin.image;
using fin.util.asserts;


namespace glo.api {
  public class GloModelFileBundle : IModelFileBundle {
    public GloModelFileBundle(IFileHierarchyFile gloFile,
                              IReadOnlyList<IFileHierarchyDirectory>
                                  textureDirectories) {
      this.GloFile = gloFile;
      this.TextureDirectories = textureDirectories;
    }

    public string GameName => "glover";
    public IFileHierarchyFile MainFile => this.GloFile;

    public IFileHierarchyFile GloFile { get; }
    public IReadOnlyList<IFileHierarchyDirectory> TextureDirectories { get; }
  }

  public class GloModelLoader : IModelLoader<GloModelFileBundle> {
    private readonly string[] hiddenNames_ = new[] { "Box01", "puzzle" };

    private readonly string[] mirrorTextures_ = new[] { "Badg2.bmp" };

    public IModel LoadModel(GloModelFileBundle gloModelFileBundle) {
      var gloFile = gloModelFileBundle.GloFile;
      var textureDirectories = gloModelFileBundle.TextureDirectories;
      var fps = 20;

      var glo = gloFile.Impl.ReadNew<Glo>();

      var textureFilesByName = new Dictionary<string, IFileHierarchyFile>();
      foreach (var textureDirectory in textureDirectories) {
        foreach (var textureFile in textureDirectory.Files) {
          textureFilesByName[textureFile.NameWithoutExtension.ToLower()] =
              textureFile;
        }
      }

      /*new MeshCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "mesh.csv")));
      new FaceCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "face.csv")));
      new VertexCsvWriter().WriteToFile(
          glo, new FinFile(Path.Join(outputDirectory.FullName, "vertex.csv")));*/

      var finModel = new ModelImpl();
      var finSkin = finModel.Skin;

      var finRootBone = finModel.Skeleton.Root;

      var finTextureMap = new LazyDictionary<string, ITexture?>(
          textureFilename => {
            if (!textureFilesByName.TryGetValue(
                    Path.GetFileNameWithoutExtension(textureFilename).ToLower(),
                    out var textureFile)) {
              return null;
            }

            using var rawTextureImage = FinImage.FromFile(textureFile.Impl);
            var textureImageWithAlpha =
                GloModelLoader.AddTransparencyToGloImage_(rawTextureImage);

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
      var withCullingMap =
          new LazyDictionary<string, IMaterial>(textureFilename => {
            var finTexture = finTextureMap[textureFilename];
            if (finTexture == null) {
              return finModel.MaterialManager.AddStandardMaterial();
            }
            return finModel.MaterialManager.AddTextureMaterial(finTexture);
          });
      var withoutCullingMap = new LazyDictionary<string, IMaterial>(
          textureFilename => {
            var finTexture = finTextureMap[textureFilename];
            IMaterial finMaterial = finTexture == null
                                        ? finModel.MaterialManager
                                                  .AddStandardMaterial()
                                        : finModel.MaterialManager
                                                  .AddTextureMaterial(
                                                      finTexture);
            finMaterial.CullingMode = CullingMode.SHOW_BOTH;
            return finMaterial;
          });

      var firstMeshMap = new Dictionary<string, GloMesh>();

      // TODO: Consider separating these out as separate models
      foreach (var gloObject in glo.Objects) {
        var finObjectRootBone = finRootBone.AddRoot(0, 0, 0);
        var meshQueue = new Queue<(GloMesh, IBone)>();
        foreach (var topLevelGloMesh in gloObject.Meshes) {
          meshQueue.Enqueue((topLevelGloMesh, finObjectRootBone));
        }

        List<(IAnimation, int, int)> finAndGloAnimations = new();
        foreach (var animSeg in gloObject.AnimSegs) {
          var startFrame = (int)animSeg.StartFrame;
          var endFrame = (int)animSeg.EndFrame;

          var finAnimation = finModel.AnimationManager.AddAnimation();
          finAnimation.Name = animSeg.Name;
          finAnimation.FrameCount =
              (int)(animSeg.EndFrame - animSeg.StartFrame + 1);

          finAnimation.FrameRate = fps * animSeg.Speed;

          finAndGloAnimations.Add((finAnimation, startFrame, endFrame));
        }

        while (meshQueue.Count > 0) {
          var (gloMesh, parentFinBone) = meshQueue.Dequeue();

          var name = gloMesh.Name;

          GloMesh idealMesh;
          if (!firstMeshMap.TryGetValue(name, out idealMesh)) {
            firstMeshMap[name] = idealMesh = gloMesh;
          }

          var position = gloMesh.MoveKeys[0].Xyz;

          var rotation = gloMesh.RotateKeys[0];
          var scale = gloMesh.ScaleKeys[0].Scale;

          var finBone = parentFinBone
                        .AddChild(position.X, position.Y, position.Z)
                        .SetLocalRotationRadians(
                            rotation.X, rotation.Y, rotation.Z)
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
                time = (int)(moveKey.Time - startFrame);
                isLast = moveKey.Time == endFrame;
              }
              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              finBoneTracks.Positions.Set(time, moveKey.Xyz);

              if (isLast) {
                break;
              }
            }

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
                time = (int)(rotateKey.Time - startFrame);
                isLast = rotateKey.Time == endFrame;
              }
              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              var quaternionKey =
                  new Quaternion(rotateKey.X, rotateKey.Y, rotateKey.Z,
                                 rotateKey.W);
              var xyzRadiansKey = QuaternionUtil.ToEulerRadians(quaternionKey);
              finBoneTracks.Rotations.Set(time, xyzRadiansKey);

              if (isLast) {
                break;
              }
            }

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
                time = (int)(scaleKey.Time - startFrame);
                isLast = scaleKey.Time == endFrame;
              }
              Asserts.True(time >= 0 && time < finAnimation.FrameCount);

              // TODO: Does this also need to be out of order?
              finBoneTracks.Scales.Set(time, scaleKey.Scale);

              if (isLast) {
                break;
              }
            }
          }

          // Anything with these names are debug objects and can be ignored.
          if (this.hiddenNames_.Contains(name)) {
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
                gloFaceColor.Rb, gloFaceColor.Gb, gloFaceColor.Bb,
                gloFaceColor.Ab);

            var enableBackfaceCulling = (gloFace.Flags & 1 << 2) == 0;

            IMaterial? finMaterial;
            if (textureFilename == previousTextureName) {
              finMaterial = previousMaterial;
            } else {
              previousTextureName = textureFilename;
              finMaterial = enableBackfaceCulling
                                ? withCullingMap[textureFilename]
                                : withoutCullingMap[textureFilename];
              previousMaterial = finMaterial;
            }

            // Face flag:
            // 0: potentially some kind of repeat mode??

            var color = (gloFace.Flags & 1 << 6) != 0
                            ? FinColor.FromRgbaBytes(255, 0, 0, 255)
                            : FinColor.FromRgbaBytes(0, 255, 0, 255);

            var finFaceVertices = new IVertex[3];
            for (var v = 0; v < 3; ++v) {
              var gloVertexRef = gloFace.VertexRefs[v];
              var gloVertex = gloVertices[gloVertexRef.Index];

              var finVertex = finSkin
                              .AddVertex(gloVertex.X, gloVertex.Y, gloVertex.Z)
                              .SetUv(gloVertexRef.U, gloVertexRef.V);
              //.SetColor(color);
              finVertex.SetBoneWeights(finSkin.GetOrCreateBoneWeights(
                                           PreprojectMode.BONE, finBone));
              finFaceVertices[v] = finVertex;
            }

            // TODO: Merge triangles together
            var finTriangles = new (IVertex, IVertex, IVertex)[1];
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
                finSkin.GetOrCreateBoneWeights(PreprojectMode.BONE,
                                               finSpriteBone);

            var textureFilename = gloSprite.TextureFilename;

            var gloFaceColor = gloSprite.Color;
            var finFaceColor = FinColor.FromRgbaBytes(
                gloFaceColor.Rb, gloFaceColor.Gb, gloFaceColor.Bb,
                gloFaceColor.Ab);

            IMaterial? finMaterial;
            if (textureFilename == previousTextureName) {
              finMaterial = previousMaterial;
            } else {
              previousTextureName = textureFilename;
              finMaterial = withCullingMap[textureFilename];
              previousMaterial = finMaterial;
            }

            var w = gloSprite.SpriteSize.X / 4;
            var h = gloSprite.SpriteSize.Y / 4;

            var v1 = finSkin
                     .AddVertex(0, -h / 2, -w / 2)
                     .SetUv(0, 0)
                     .SetBoneWeights(finSpriteBoneWeights);
            var v2 = finSkin
                     .AddVertex(0, -h / 2, w / 2)
                     .SetUv(1, 0)
                     .SetBoneWeights(finSpriteBoneWeights);
            var v3 = finSkin
                     .AddVertex(0, h / 2, -w / 2)
                     .SetUv(0, 1)
                     .SetBoneWeights(finSpriteBoneWeights);
            var v4 = finSkin
                     .AddVertex(0, h / 2, w / 2)
                     .SetUv(1, 1)
                     .SetBoneWeights(finSpriteBoneWeights);

            finMesh.AddTriangles((v1, v3, v2), (v4, v2, v3))
                   .SetMaterial(finMaterial!);
          }
        }

        // TODO: Split out animations
      }

      return finModel;
    }

    private static IImage AddTransparencyToGloImage_(IImage rawImage) {
      var width = rawImage.Width;
      var height = rawImage.Height;

      var textureImageWithAlpha = new Rgba32Image(width, height);
      textureImageWithAlpha.Mutate((_, setHandler) => {
        rawImage.Access(getHandler => {
          for (var y = 0; y < height; ++y) {
            for (var x = 0; x < width; ++x) {
              getHandler(x, y, out var r, out var g, out var b, out var a);

              if (r == 255 && g == 0 && b == 255) {
                a = 0;
              }

              setHandler(x, y, r, g, b, a);
            }
          }
        });
      });

      return textureImageWithAlpha;
    }
  }
}