using System;
using System.Collections.Generic;
using System.Linq;

using Assimp;
using Assimp.Unmanaged;

using fin.exporter.gltf;
using fin.io;
using fin.log;
using fin.model;
using fin.model.io.exporters;
using fin.util.asserts;

using WrapMode = fin.model.WrapMode;

namespace fin.exporter.assimp {
  using FinBlendMode = fin.model.BlendMode;

  public class AssimpDirectExporter : IModelExporter {
    // You can bet your ass I'm gonna prefix everything with ass.

    public void Export(ITreeFile outputFile, IModel model) {
      var outputPath = outputFile.FullPath;
      var outputExtension = outputFile.FileType;

      using var ctx = new AssimpContext();

      string exportFormatId;
      {
        var supportedExportFormats = ctx.GetSupportedExportFormats();
        var exportFormatIds =
            supportedExportFormats
                .Where(exportFormat
                           => outputExtension ==
                              $".{exportFormat.FileExtension}")
                .Select(exportFormat => exportFormat.FormatId);
        Asserts.True(exportFormatIds.Any(),
                     $"'{outputExtension}' is not a supported export format!");

        exportFormatId = exportFormatIds.First();
      }

      var assScene = new Scene();
      assScene.RootNode = new Node("ROOT");

      var inputFile = outputFile.CloneWithExtension(".glb");
      var inputPath = inputFile.FullName;

      new GltfExporter().Export(inputFile, model);
      var sc = ctx.ImportFile(inputPath);
      ;

      // [ ]: Get skeleton working
      // [ ]: Get mesh working
      // [ ]: Get UVs working
      // [ ]: Get materials working
      // [ ]: Get multi-bone-binding working

      new AssimpSkeletonBuilder().BuildAndBindSkeleton(assScene, model);
      new AssimpMeshBuilder().BuildAndBindMesh(assScene, model);

      /*var postProcessSteps = PostProcessSteps.FindInvalidData |
                             PostProcessSteps.ValidateDataStructure |
                             PostProcessSteps.GenerateBoundingBoxes |
                             PostProcessSteps.JoinIdenticalVertices |
                             PostProcessSteps.OptimizeMeshes |
                             PostProcessSteps.PreTransformVertices;*/

      LogStream.IsVerboseLoggingEnabled = true;
      var logStream = new FinLogStream();
      logStream.Attach();

      bool wasSuccessful = false;
      try {
        wasSuccessful =
            ctx.ExportFile(assScene,
                           outputPath,
                           exportFormatId);
      } catch (Exception e) {}
      var error = AssimpLibrary.Instance.GetErrorString();
      Asserts.True(wasSuccessful, "Failed to export model: " + error);

      return;

/*      // Importing the pre-generated GLTF file does most of the hard work off
      // the bat: generating the mesh with properly weighted bones.

      // Bone orientation is already correct, you just need to enable
      // "Automatic Bone Orientation" if importing in Blender.

      // Fix animations.
      var finAnimations = model.AnimationManager.Animations;
      var assAnimations = sc.Animations;
      for (var a = 0; a < assAnimations.Count; ++a) {
        var assAnimation = assAnimations[a];
        var finAnimation = finAnimations[a];

        // Animations are SUPER slow, we need to speed them way up!
        {
          // Not entirely sure why this is right...
          var animationSpeedup = 2 / assAnimation.DurationInTicks;

          // TODO: Include tangents from the animation file.
          foreach (var channel in assAnimation.NodeAnimationChannels) {
            this.ScaleKeyTimes_(channel.PositionKeys, animationSpeedup);
            this.ScaleKeyTimes_(channel.ScalingKeys, animationSpeedup);
            this.ScaleKeyTimes_(channel.RotationKeys, animationSpeedup);
          }
        }

        var assFps = assAnimation.TicksPerSecond;
        var finFps = finAnimation.Fps;

        assAnimation.TicksPerSecond = finFps;
        assAnimation.DurationInTicks *= finFps / assFps;

        // TODO: Include animation looping behavior here.
      }

      // Export materials.
      {
        /*var finTextures = new HashSet<string>();
        foreach (var finMaterial in model.MaterialManager.All) {
          foreach (var finTexture in finMaterial.Textures) {
            finTextures.Add(finTexture);
          }
        }

        foreach (var finTexture in finTextures) {
          var imageData = finTexture.ImageData;

          var imageBytes = new MemoryStream();
          imageData.Save(imageBytes, ImageFormat.Png);

          var assTexture =
              new EmbeddedTexture("png",
                                  imageBytes.ToArray(),
                                  finTexture.Name);
          assTexture.Filename = finTexture.Name + ".png";

          finTextureToAssTexture[finTexture] = assTexture;
          sc.Textures.Add(assTexture);
        }

        // TODO: Need to update the UVs...

        // Fix the UVs.
        var finVertices = model.Skin.Vertices;
        var assMeshes = sc.Meshes;

        var animations = model.AnimationManager.Animations;
        var firstAnimation =
            (animations?.Count ?? 0) > 0 ? animations[0] : null;

        var boneTransformManager = new BoneTransformManager();
        /*boneTransformManager.CalculateMatrices(
            model.Skeleton.Root,
            firstAnimation != null ? (firstAnimation, 0) : null);
        boneTransformManager.CalculateMatrices(model.Skeleton.Root, null);

        var finVerticesByMaterial =
            model.Skin.Primitives
                 .GroupBy(primitive => primitive.Material)
                 .Select(grouping => {
                   var positionsAndNormals = grouping
                       .SelectMany(primitive => primitive.Vertices)
                       .ToHashSet()
                       .Select(finVertex => {
                         IPosition position =
                             new ModelImpl.PositionImpl();
                         INormal normal = new ModelImpl.NormalImpl();
                         boneTransformManager.ProjectVertex(
                             finVertex,
                             position,
                             normal);
                         return (position, normal);
                       });

                   //var uniquePositions = positionsAndNormals.

                   return positionsAndNormals;
                 })
                 .ToList();

        var worldFinVerticesAndNormals =
            finVertices.Select(finVertex => {
                         IPosition position =
                             new ModelImpl.PositionImpl();
                         INormal normal = new ModelImpl.NormalImpl();
                         boneTransformManager.ProjectVertex(
                             finVertex,
                             position,
                             normal);
                         return (position, normal);
                       })
                       .ToArray();

        foreach (var assMesh in assMeshes) {
          var assLocations = assMesh.Vertices;
          var assNormals = assMesh.Normals;

          var assUvs = assMesh.TextureCoordinateChannels;

          var oldAssUvs = new List<Vector3D>[8];
          for (var t = 0; t < 8; ++t) {
            oldAssUvs[t] = assUvs[t].Select(x => x).ToList();
            assUvs[t].Clear();
          }


          var hadUv = new bool[8];
          for (var i = 0; i < assLocations.Count; ++i) {
            var assLocation = assLocations[i];
            var assNormal = assNormals[i];

            // TODO: How to find this more efficiently???
            var minIndex = -1;
            for (var v = 0; v < worldFinVerticesAndNormals.Length; ++v) {
              var (position, normal) = worldFinVerticesAndNormals[v];
              var inNormal = finVertices[v].LocalNormal;

              var tolerance = .005;

              if (i == 131) {
                var finUv = finVertices[v].GetUv(0);

                if (Math.Abs(finUv.U - .9375) < tolerance ||
                    Math.Abs((1 - finUv.U) - .9375) < tolerance) {
                  var u0 = finUv.U;
                  var v0 = 1 - finUv.V;
                  ;
                }
              }

              if (Math.Abs(position.X - assLocation.X) < tolerance &&
                  Math.Abs(position.Y - assLocation.Y) < tolerance &&
                  Math.Abs(position.Z - assLocation.Z) < tolerance) {
                if (Math.Abs(normal.X - assNormal.X) < tolerance &&
                    Math.Abs(normal.Y - assNormal.Y) < tolerance &&
                    Math.Abs(normal.Z - assNormal.Z) < tolerance) {
                  minIndex = v;
                  break;
                } else {
                  ;
                }
              }
            }

            var minLocation = worldFinVerticesAndNormals[minIndex];
            var minVertex = finVertices[minIndex];

            for (var t = 0; t < 8; ++t) {
              var uv = minVertex.GetUv(t);

              if (uv != null) {
                hadUv[t] = true;
                assUvs[t].Add(new Vector3D(uv.U, 1 - uv.V, 0));
              } else {
                assUvs[t].Add(default);
              }
            }
          }

          for (var t = 0; t < 8; ++t) {
            if (!hadUv[t]) {
              assUvs[t].Clear();
            }
          }

          var lhs = oldAssUvs[0];
          var rhs = assUvs[0];
          for (var i = 0; i < lhs.Count; ++i) {
            var oldAssUv = lhs[i];
            var assUv = rhs[i];

            if (Math.Abs(oldAssUv.X - assUv.X) > .01 ||
                Math.Abs(oldAssUv.Y - assUv.Y) > .01) {
              var assWorld = assLocations[i];
              ;
            }
          }

          ;
        }

        // Re-add the textures.

        sc.Textures.Clear();
        sc.Materials.Clear();

        foreach (var finMaterial in model.MaterialManager.All) {
          var assMaterial = new Material();

          assMaterial.Shaders.ShaderLanguageType = "HLSL";
          assMaterial.Shaders.FragmentShader =
              @"
struct a2v { 
  float4 Position : POSITION;
  float4 Color    : COLOR0;
};

struct v2p {
  float4 Position : POSITION;
  float4 Color    : COLOR0; 
};

void main(in a2v IN, out v2p OUT, uniform float4x4 ModelViewMatrix) {
  OUT.Position = mul(IN.Position, ModelViewMatrix);
  OUT.Color    = float4(255, 255, 0, 0);
}
";

          assMaterial.Name = finMaterial.Name;
          // TODO: Set shader

          if (finMaterial is ILayerMaterial layerMaterial) {
            var addLayers =
                layerMaterial
                    .Layers
                    .Where(layer => layer.BlendMode == FinBlendMode.ADD)
                    .ToArray();
            var multiplyLayers =
                layerMaterial
                    .Layers
                    .Where(layer => layer.BlendMode == FinBlendMode.MULTIPLY)
                    .ToArray();

            if (addLayers.Length == 0) {
              throw new NotSupportedException("Expected to find an add layer!");
            }
            if (addLayers.Length > 1) {
              ;
            }
            if (addLayers.Length > 2) {
              throw new NotSupportedException("Too many add layers for GLTF!");
            }

            for (var i = 0; i < addLayers.Length; ++i) {
              var layer = addLayers[i];

              // TODO: Support flat color layers by generating a 1x1 clamped texture of that color.
              if (layer.ColorSource is ITexture finTexture) {
                var assTextureSlot = new TextureSlot();
                assTextureSlot.FilePath = finTexture.Name + ".png";

                // TODO: FBX doesn't support mirror. Blegh
                assTextureSlot.WrapModeU =
                    this.ConvertWrapMode_(finTexture.WrapModeU);
                assTextureSlot.WrapModeV =
                    this.ConvertWrapMode_(finTexture.WrapModeV);

                if (i == 0) {
                  assTextureSlot.TextureType = TextureType.Diffuse;
                } else {
                  assTextureSlot.TextureType = TextureType.Emissive;
                }

                // TODO: Set blend mode
                //assTextureSlot.Operation =

                assTextureSlot.UVIndex = layer.TexCoordIndex;

                // TODO: Set texture coord type

                assMaterial.AddMaterialTexture(assTextureSlot);
              }
            }

            /*foreach (var layer in layerMaterial.Layers) {
              // TODO: Support flat color layers by generating a 1x1 clamped texture of that color.

              if (layer.ColorSource is ITexture finTexture) {
                var assTextureSlot = new TextureSlot();
                assTextureSlot.FilePath = finTexture.Name + ".png";
                assTextureSlot.TextureType = TextureType.Diffuse;

                // TODO: FBX doesn't support mirror. Blegh
                assTextureSlot.WrapModeU =
                    this.ConvertWrapMode_(finTexture.WrapModeU);
                assTextureSlot.WrapModeV =
                    this.ConvertWrapMode_(finTexture.WrapModeV);

                // TODO: Set blend mode
                //assTextureSlot.Operation =

                // TODO: Set texture coord type

                assMaterial.AddMaterialTexture(assTextureSlot);
              }
            }*/
    }

    /*{
      var lastFinTexture = finMaterial.Textures.Last();

      var assTextureSlot = new TextureSlot();
      assTextureSlot.FilePath = lastFinTexture.Name + ".png";
      assTextureSlot.TextureType = TextureType.Diffuse;

      // TODO: FBX doesn't support mirror. Blegh
      assTextureSlot.WrapModeU = this.ConvertWrapMode_(lastFinTexture.WrapModeU);
      assTextureSlot.WrapModeV = this.ConvertWrapMode_(lastFinTexture.WrapModeV);

      // TODO: Set blend mode
      // TODO: Set texture coord type

      assMaterial.TextureDiffuse = assTextureSlot;
    }*/

    /*sc.Materials.Add(assMaterial);
  }

  // Meshes should already have material indices set.
}

var success = ctx.ExportFile(sc, outputPath, exportFormatId);
Asserts.True(success, "Failed to export model.");

/*var scene = new Scene();

var animations = model.AnimationManager.Animations;
var firstAnimation = (animations?.Count ?? 0) > 0 ? animations[0] : null;

if (firstAnimation != null) {
  this.ApplyFirstFrameToSkeleton_(model.Skeleton, firstAnimation);
}

var boneTransformManager = new BoneTransformManager();
boneTransformManager.CalculateMatrices(model.Skeleton.Root,
                                       firstAnimation != null
                                           ? (firstAnimation, 0)
                                           : null);



var mesh = new Mesh();
var meshNode = scene.RootNode.CreateChildNode("mesh", mesh);

var bones = new List<Bone>();

var finToFbxBones = new Dictionary<IBone, Bone>();
var skd = new SkinDeformer("skeleton");
{
  var skeleton = model.Skeleton;

  var boneQueue = new Queue<(Node, IBone)>();

  var fbxSkeletonRoot =
      scene.RootNode.CreateChildNode("skeletonNode", new Skeleton("sk"));
  boneQueue.Enqueue((fbxSkeletonRoot, skeleton.Root));

  while (boneQueue.Count > 0) {
    var (fbxBoneNode, finBone) = boneQueue.Dequeue();

    var transform = fbxBoneNode.Transform;

    var position = finBone.LocalPosition;
    transform.Translation =
        new Vector3(position.X, position.Y, position.Z);

    var rotation = finBone.LocalRotation;
    if (rotation != null) {
      transform.Rotation = Quaternion.FromEulerAngle(
          new Vector3(rotation.XRadians,
                      rotation.YRadians,
                      rotation.ZRadians));
    }

    var fbxBone = new Bone(finBone.Name + "Bone") {
        Node = fbxBoneNode
    };

    fbxBone.BoneTransform = fbxBone.Transform.Inverse();

    bones.Add(fbxBone);
    finToFbxBones[finBone] = fbxBone;

    foreach (var child in finBone.Children) {
      var sk = new Skeleton(child.Name + "Sk") {Type = SkeletonType.Bone};

      var childNode =
          fbxBoneNode.CreateChildNode(child.Name + "Node", sk);
      boneQueue.Enqueue((childNode, child));
    }
  }
}

{
  IPosition outPosition = new ModelImpl.PositionImpl();
  INormal outNormal = new ModelImpl.NormalImpl();

  var vertices = model.Skin.Vertices;

  var positions = new Vector4[vertices.Count];
  var normals = new Vector4[vertices.Count];
  foreach (var vertex in vertices) {
    boneTransformManager.ProjectVertex(vertex,
                                       outPosition,
                                       outNormal);

    var vertexIndex = vertex.Index;

    var hasMultipleWeights = (vertex.Weights?.Count ?? 0) > 1;

    var position = outPosition;
    //outNormal = vertex.LocalNormal;

    positions[vertexIndex] =
        new Vector4(position.X,
                    position.Y,
                    position.Z,
                    position.W);
    normals[vertexIndex] =
        new Vector4(outNormal.X, outNormal.Y, outNormal.Z, outNormal.W);

    if (vertex.Weights != null) {
      foreach (var weight in vertex.Weights) {
        var fbxBone = finToFbxBones[weight.Bone];
        fbxBone.SetWeight(vertexIndex, weight.Weight);

        if (hasMultipleWeights && false) {
          var boneMatrix = weight.SkinToBone;
          var boneTransform = fbxBone.BoneTransform;

          /*boneTransform.m00 = boneMatrix[0, 0];
          boneTransform.m01 = boneMatrix[0, 1];
          boneTransform.m02 = boneMatrix[0, 2];
          boneTransform.m03 = boneMatrix[0, 3];

          boneTransform.m10 = boneMatrix[1, 0];
          boneTransform.m11 = boneMatrix[1, 1];
          boneTransform.m12 = boneMatrix[1, 2];
          boneTransform.m13 = boneMatrix[1, 3];

          boneTransform.m20 = boneMatrix[2, 0];
          boneTransform.m21 = boneMatrix[2, 1];
          boneTransform.m22 = boneMatrix[2, 2];
          boneTransform.m23 = boneMatrix[2, 3];

          boneTransform.m30 = boneMatrix[3, 0];
          boneTransform.m31 = boneMatrix[3, 1];
          boneTransform.m32 = boneMatrix[3, 2];
          boneTransform.m33 = boneMatrix[3, 3];*/

    /*boneTransform.m00 = boneMatrix[0, 0];
    boneTransform.m10 = boneMatrix[0, 1];
    boneTransform.m20 = boneMatrix[0, 2];
    boneTransform.m30 = boneMatrix[0, 3];

    boneTransform.m01 = boneMatrix[1, 0];
    boneTransform.m11 = boneMatrix[1, 1];
    boneTransform.m21 = boneMatrix[1, 2];
    boneTransform.m31 = boneMatrix[1, 3];

    boneTransform.m02 = boneMatrix[2, 0];
    boneTransform.m12 = boneMatrix[2, 1];
    boneTransform.m22 = boneMatrix[2, 2];
    boneTransform.m32 = boneMatrix[2, 3];

    boneTransform.m03 = boneMatrix[3, 0];
    boneTransform.m13 = boneMatrix[3, 1];
    boneTransform.m23 = boneMatrix[3, 2];
    boneTransform.m33 = boneMatrix[3, 3];

    fbxBone.BoneTransform = boneTransform;
  }
}
}
}

mesh.ControlPoints.AddRange(positions);

var normalElement = new VertexElementNormal();
normalElement.Data.AddRange(normals);
mesh.AddElement(normalElement);
}

foreach (var bone in bones) {
skd.Bones.Add(bone);
}

mesh.Deformers.Add(skd);

{
var polygonBuilder = new PolygonBuilder(mesh);

foreach (var primitive in model.Skin.Primitives) {
var points = primitive.Vertices;
var pointsCount = points.Count;

switch (primitive.Type) {
case PrimitiveType.TRIANGLES: {
  for (var v = 0; v < pointsCount; v += 3) {
    polygonBuilder.Begin();
    polygonBuilder.AddVertex(points[v + 0].Index);
    polygonBuilder.AddVertex(points[v + 1].Index);
    polygonBuilder.AddVertex(points[v + 2].Index);
    polygonBuilder.End();
  }
  break;
}
case PrimitiveType.TRIANGLE_STRIP: {
  for (var v = 0; v < pointsCount - 2; ++v) {
    polygonBuilder.Begin();
    if (v % 2 == 0) {
      polygonBuilder.AddVertex(points[v + 0].Index);
      polygonBuilder.AddVertex(points[v + 1].Index);
      polygonBuilder.AddVertex(points[v + 2].Index);
    } else {
      // Switches drawing order to maintain proper winding:
      // https://www.khronos.org/opengl/wiki/Primitive
      polygonBuilder.AddVertex(points[v + 1].Index);
      polygonBuilder.AddVertex(points[v + 0].Index);
      polygonBuilder.AddVertex(points[v + 2].Index);
    }
    polygonBuilder.End();
  }
  break;
}
case PrimitiveType.QUADS: {
  for (var v = 0; v < pointsCount; v += 4) {
    polygonBuilder.Begin();
    polygonBuilder.AddVertex(points[v + 0].Index);
    polygonBuilder.AddVertex(points[v + 1].Index);
    polygonBuilder.AddVertex(points[v + 2].Index);
    polygonBuilder.AddVertex(points[v + 3].Index);
    polygonBuilder.End();
  }
  break;
}
default: throw new NotSupportedException();
}
}
}

var success = ctx.ExportFile(scene, outputPath, exportFormatId);
Asserts.True(success, "Failed to export model.");
}

// TODO: Pull this out somewhere else, or make this part of the model creation flow?
private void ApplyFirstFrameToSkeleton_(
ISkeleton skeleton,
IAnimation animation) {
var boneQueue = new Queue<IBone>();
boneQueue.Enqueue(skeleton.Root);

while (boneQueue.Count > 0) {
var bone = boneQueue.Dequeue();

animation.BoneTracks.TryGetValue(bone, out var boneTracks);

var localPosition = boneTracks?.Positions.GetInterpolatedFrame(0);
bone.SetLocalPosition(localPosition?.X ?? 0,
                  localPosition?.Y ?? 0,
                  localPosition?.Z ?? 0);

var localRotation = boneTracks?.Rotations.GetKeyframe(0);
bone.SetLocalRotationRadians(localRotation?.XRadians ?? 0,
                         localRotation?.YRadians ?? 0,
                         localRotation?.ZRadians ?? 0);

// It seems like some animations shrink a bone to 0 to hide it, but
// this prevents us from calculating a determinant to invert the
// matrix. As a result, we can't safely include the scale here.
IScale?
localScale = null; //boneTracks?.Scales.GetInterpolatedAtFrame(0);
bone.SetLocalScale(localScale?.X ?? 1,
               localScale?.Y ?? 1,
               localScale?.Z ?? 1);

foreach (var child in bone.Children) {
boneQueue.Enqueue(child);
}
}
  }*/

    private void ScaleKeyTimes_(List<VectorKey> keys, double scale) {
      for (var i = 0; i < keys.Count; ++i) {
        var key = keys[i];
        key.Time *= scale;
        keys[i] = key;
      }
    }

    private void ScaleKeyTimes_(List<QuaternionKey> keys, double scale) {
      for (var i = 0; i < keys.Count; ++i) {
        var key = keys[i];
        key.Time *= scale;
        keys[i] = key;
      }
    }

    private TextureWrapMode ConvertWrapMode_(WrapMode wrapMode)
      => wrapMode switch {
          WrapMode.CLAMP         => TextureWrapMode.Clamp,
          WrapMode.REPEAT        => TextureWrapMode.Wrap,
          WrapMode.MIRROR_REPEAT => TextureWrapMode.Mirror,
          _ => throw new ArgumentOutOfRangeException(
                   nameof(wrapMode),
                   wrapMode,
                   null)
      };

    private class FinLogStream : LogStream {
      private readonly ILogger impl_ = Logging.Create<FinLogStream>();

      public FinLogStream() {}

      public FinLogStream(string userData) : base(userData) {}

      protected override void LogMessage(string msg, string userData) {
        if (string.IsNullOrEmpty(userData)) {
          this.impl_.LogInformation(msg);
        } else {
          this.impl_.LogInformation(string.Format("{0}: {1}", userData, msg));
        }
      }
    }
  }
}