using System.Numerics;

using fin.animation.keyframes;
using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math.rotations;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;
using fin.util.strings;

using schema.text.reader;

using tlpe.scb;

namespace tlpe.api;

public sealed record ScbModelFileBundle(
    IReadOnlyTreeFile ScbFile,
    IReadOnlyTreeFile BallsFile,
    IReadOnlyTreeDirectory TexturesDir)
    : IModelFileBundle<ScbModelFileBundle, ScbModelImporter> {
  public IReadOnlyTreeFile MainFile => this.ScbFile;
}

public sealed class ScbModelImporter : IModelImporter<ScbModelImporter, ScbModelFileBundle> {
  public IModel Import(ScbModelFileBundle fileBundle) {
    var scb = fileBundle.ScbFile.ReadNew<Scb>();

    var files = fileBundle.ScbFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files
    };

    var allBallAttributes = ReadAllBallAttributes_(fileBundle.BallsFile);
    var ballAttributesByGeometry
        = allBallAttributes
            .ToListDictionary(a => a.Geometry?.ToLower() ?? "ball.scb");

    var material1TextureFiles = new List<IReadOnlyTreeFile>();
    if (ballAttributesByGeometry.TryGetList(
            fileBundle.ScbFile.Name.ToString().ToLower(),
            out var matchingBallAttributes)) {
      foreach (var ballAttributes in matchingBallAttributes) {
        if (ballAttributes.Texture != null) {
          var textureFile = fileBundle.TexturesDir.AssertGetExistingFile(
              ballAttributes.Texture);
          material1TextureFiles.Add(textureFile);
        }
      }
    }

    var materialById = new Dictionary<uint, IReadOnlyMaterial>();

    foreach (var scbSection in scb.Sections) {
      if (scbSection is MaterialSection materialSection) {
        var id = materialSection.Id;

        var textureFiles = new List<IReadOnlyTreeFile>();
        var textureName = materialSection.TextureName;
        if (textureName.Length > 0) {
          var textureFile = fileBundle.ScbFile.AssertGetParent()
                                      .AssertGetExistingFile(textureName);
          textureFiles.Add(textureFile);
        }

        if (id == 1) {
          textureFiles.AddRange(material1TextureFiles);
        }

        var textures = textureFiles
                       .Select(textureFile => {
                         var image = FinImage.FromFile(textureFile);
                         var finTexture
                             = finModel.MaterialManager.CreateTexture(image);
                         finTexture.Name
                             = textureFile.NameWithoutExtension.ToString();
                         finTexture.WrapModeU
                             = finTexture.WrapModeV = WrapMode.REPEAT;
                         return finTexture;
                       })
                       .ToArray();

        var finMaterial = finModel.MaterialManager.AddStandardMaterial();
        finMaterial.Name = materialSection.Name;
        finMaterial.DiffuseTexture = textures.FirstOrDefault();

        materialById[id] = finMaterial;
      }
    }

    var finSkin = finModel.Skin;

    IBone? currentBone = null;
    IReadOnlyBoneWeights? currentBoneWeights = null;

    var finBoneByName = new CaseInvariantStringDictionary<IBone>();

    var framerate = 30f;
    var lazyAnimationById = new LazyDictionary<uint, IModelAnimation>(i => {
      var finAnimation = finModel.AnimationManager.AddAnimation();
      finAnimation.Name = $"animation {i}";
      finAnimation.FrameRate = framerate;
      return finAnimation;
    });

    foreach (var scbSection in scb.Sections) {
      switch (scbSection) {
        case JointSection joint: {
          if (!finBoneByName.TryGetValue(joint.ParentName,
                                         out var parentBone)) {
            parentBone = finModel.Skeleton.Root;
          }

          currentBone = parentBone.AddChild(AdjustVector3_(joint.Translation));
          currentBone.Transform.SetRotation(
              ConvertRotationToQuaternion_(joint.Rotation));
          currentBone.Transform.SetScale(AdjustVector3_(joint.Scale));
          currentBone.Name = joint.Name;

          finBoneByName[currentBone.Name] = currentBone;

          currentBoneWeights
              = finModel.Skin.GetOrCreateBoneWeights(
                  VertexSpace.RELATIVE_TO_BONE,
                  currentBone);

          break;
        }
        case AnimationSection animationSection: {
          var finAnimation = lazyAnimationById[animationSection.Id];

          var finBoneTracks = finAnimation.GetOrCreateBoneTracks(currentBone!);
          var rotationTrack = finBoneTracks.UseCombinedQuaternionKeyframes(
              animationSection.Keyframes.Length);
          foreach (var keyframe in animationSection.Keyframes) {
            var frame = keyframe.Frame / framerate;

            finAnimation.FrameCount = (int) Math.Max(finAnimation.FrameCount, frame);

            var rotation = currentBone.Transform.LocalRotation.Value *
                           ConvertRotationToQuaternion_(keyframe.EulerRadians);
            rotationTrack.SetKeyframe(frame, rotation);
          }

          break;
        }
        case MeshSection meshSection: {
          var finMesh = finSkin.AddMesh();
          finMesh.Name = currentBone?.Name;


          var scbVertices = meshSection.Vertices;
          var finVertices = new IReadOnlyVertex[scbVertices.Length];
          for (var i = 0; i < scbVertices.Length; ++i) {
            var scbVertex = scbVertices[i];

            var finVertex
                = finSkin.AddVertex(AdjustVector3_(scbVertex.Position));
            finVertex.SetLocalNormal(AdjustVector3_(scbVertex.Normal));
            finVertex.SetUv(0, AdjustVector2_(scbVertex.Uv0));
            finVertex.SetUv(1, AdjustVector2_(scbVertex.Uv1));

            if (currentBoneWeights != null) {
              finVertex.SetBoneWeights(currentBoneWeights);
            }

            finVertices[scbVertices.Length - 1 - i] = finVertex;
          }

          var trianglesByMaterial
              = meshSection.Faces
                           .ToListDictionary(
                               f => f.MaterialId,
                               f => {
                                 var v0 = finVertices[f.Vertex0];
                                 var v1 = finVertices[f.Vertex1];
                                 var v2 = finVertices[f.Vertex2];
                                 return (v0, v1, v2);
                               });

          foreach (var materialId in trianglesByMaterial.Keys) {
            var finPrimitive = finMesh.AddTriangles(
                (IReadOnlyList<(IReadOnlyVertex, IReadOnlyVertex,
                    IReadOnlyVertex)>) trianglesByMaterial[materialId]);
            finPrimitive.SetMaterial(materialById[materialId]);
            finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
          }

          break;
        }
      }
    }

    return finModel;
  }

  private class BallAttributes {
    public string Id { get; set; }
    public string? Geometry { get; set; }
    public string? Texture { get; set; }
    public bool MapFromFile { get; set; }
    public bool NoEnvMap { get; set; }
  }

  private static Vector2 AdjustVector2_(Vector2 input)
    => input with { Y = 1 - input.Y };

  private static Vector3 AdjustVector3_(Vector3 input) => input;

  private static Quaternion ConvertRotationToQuaternion_(Vector3 input)
    => input.CreateZyxRadians();

  private static IList<BallAttributes> ReadAllBallAttributes_(
      IReadOnlyTreeFile file) {
    using var tr = new SchemaTextReader(file.OpenRead());

    var allAttributes = new List<BallAttributes>();

    BallAttributes currentAttributes = default!;
    while (!tr.Eof) {
      var line = tr.ReadLine().Trim();

      if (line.StartsWith('[')) {
        var id = line[1..].SubstringUpTo(']');
        currentAttributes = new BallAttributes { Id = id };
        allAttributes.Add(currentAttributes);
      } else {
        var equalsParts = line.Split('=');
        if (equalsParts.Length > 1) {
          var before = equalsParts[0].Trim();
          var after = equalsParts[1].Trim();

          switch (before) {
            case "Geometry": {
              currentAttributes.Geometry = after;
              break;
            }
            case "MapFromFile": {
              currentAttributes.MapFromFile = after == "1";
              break;
            }
            case "NoEnvMap": {
              currentAttributes.NoEnvMap = after == "1";
              break;
            }
            case "Texture": {
              currentAttributes.Texture = after;
              break;
            }
          }
        }
      }
    }

    return allAttributes;
  }
}