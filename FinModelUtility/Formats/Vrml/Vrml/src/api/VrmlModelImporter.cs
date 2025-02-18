using System.Drawing;
using System.Numerics;

using DelaunatorSharp;

using fin.animation.keyframes;
using fin.color;
using fin.common;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.impl;
using fin.math;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.io.importers;
using fin.model.util;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.texture;
using fin.util.asserts;
using fin.util.enumerables;
using fin.util.image;
using fin.util.linq;
using fin.util.sets;
using fin.util.strings;

using QuickFont;
using QuickFont.Configuration;

using vrml.schema;
using vrml.util;

namespace vrml.api;

using IndexedFaceGroup = (int coordIndex, int? texCoordIndex, int? colorIndex);

public class VrmlModelImporter : IModelImporter<VrmlModelFileBundle> {
  public IModel Import(VrmlModelFileBundle fileBundle) {
    var wrlFile = fileBundle.WrlFile.Impl;
    using var wrlFileStream = wrlFile.OpenRead();
    var (vrmlScene, definitions) = VrmlParser.Parse(wrlFileStream);
    var fileSet = fileBundle.WrlFile.AsFileSet();
    return this.Import(vrmlScene, definitions, fileBundle, fileSet);
  }

  public IModel Import(IGroupNode vrmlScene,
                       IReadOnlyDictionary<string, INode> definitions,
                       IVrmlFileBundle fileBundle,
                       HashSet<IReadOnlyGenericFile> fileSet) {
    var wrlFile = fileBundle.WrlFile;
    var wrlDirectory = wrlFile.AssertGetParent();
    var finModel = new ModelImpl { FileBundle = fileBundle, Files = fileSet };

    var lazyTextureDictionary
        = new LazyDictionary<(string, ITextureTransformNode?),
            IReadOnlyTexture>(tuple => {
          var (name, transformNode) = tuple;

          var imageFile = wrlDirectory.AssertGetExistingFile(name);
          fileSet.Add(imageFile);

          var finTexture
              = finModel.MaterialManager.CreateTexture(
                  FinImage.FromFile(imageFile));
          finTexture.Name = imageFile.NameWithoutExtension.ToString();
          finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;

          if (transformNode != null) {
            var center = transformNode.Center;
            if (center != null) {
              finTexture.SetCenter2d(center.Value.X, center.Value.Y);
            }

            var rotation = transformNode.Rotation;
            if (rotation != null) {
              finTexture.SetRotationRadians2d(rotation.Value);
            }

            var scale = transformNode.Scale;
            if (scale != null) {
              finTexture.SetScale2d(scale.Value.X, scale.Value.Y);
            }

            var translation = transformNode.Translation;
            if (translation != null) {
              finTexture.SetTranslation2d(translation.Value.X,
                                          translation.Value.Y);
            }
          }

          return finTexture;
        });

    HeadlessGl.MakeCurrent();
    FreeTypeFontUtil.InitIfNeeded();

    var fontSize = 72f;
    using var qFont = new QFont(
        CommonFiles.SANS_FONT_FILE.FullPath,
        fontSize,
        new QFontBuilderConfiguration(false));
    var lazyTextTextureDictionary = new LazyDictionary<TextNode, ITexture>(
        textNode => {
          var text = string.Join('\n', textNode.String);

          var fontAlignment = textNode.FontStyle.Justify switch {
              Justify.BEGIN => QFontAlignment.Left,
              Justify.MIDDLE => QFontAlignment.Centre,
              Justify.END => QFontAlignment.Right,
          };

          using var glTextTexture
              = new GlTextTexture(text, qFont, Color.White, fontAlignment);
          var image = glTextTexture.ConvertToImage(true);

          var finTexture = finModel.MaterialManager.CreateTexture(image);
          finTexture.Name = text;

          return finTexture;
        });
    var lazyMaterialDictionary
        = new LazyDictionary<(AppearanceNode, TextNode?), IMaterial>(
            tuple => {
              var (appearanceNode, textNode) = tuple;
              var vrmlMaterial = appearanceNode.Material;

              var color = vrmlMaterial.DiffuseColor;
              var alpha = 1 - vrmlMaterial.Transparency;

              var finMaterial
                  = finModel.MaterialManager.AddFixedFunctionMaterial();
              finMaterial.DepthCompareType = DepthCompareType.Less;

              var equations = finMaterial.Equations;
              var colorOps = new ColorFixedFunctionOps(equations);
              var scalarOps = new ScalarFixedFunctionOps(equations);

              IColorValue? diffuseSurfaceColor
                  = equations.CreateColorConstant(color);
              IScalarValue? diffuseSurfaceAlpha
                  = equations.CreateScalarConstant(alpha);

              IReadOnlyTexture? finTexture = null;
              var vrmlTexture = appearanceNode.Texture;
              if (vrmlTexture != null) {
                finTexture = lazyTextureDictionary[
                    (vrmlTexture.Url.ToLower(),
                     appearanceNode.TextureTransform)];
              } else if (textNode != null) {
                finTexture = lazyTextTextureDictionary[textNode];
              }

              if (finTexture != null) {
                finMaterial.Name = finTexture.Name;
                finMaterial.SetTextureSource(0, finTexture);

                var textureColor
                    = equations.CreateOrGetColorInput(
                        FixedFunctionSource.TEXTURE_COLOR_0);
                var textureAlpha
                    = equations.CreateOrGetScalarInput(
                        FixedFunctionSource.TEXTURE_ALPHA_0);

                diffuseSurfaceColor
                    = colorOps.Multiply(diffuseSurfaceColor, textureColor);
                diffuseSurfaceAlpha
                    = scalarOps.Multiply(diffuseSurfaceAlpha, textureAlpha);
              }

              var ambientSurfaceColor = vrmlMaterial.AmbientColor != null
                  ? equations.CreateColorConstant(
                      vrmlMaterial.AmbientColor.Value)
                  : colorOps.One;

              var diffuseLightColor = equations.GetMergedLightDiffuseColor();
              var ambientLightColor = colorOps.MultiplyWithConstant(
                  equations.CreateOrGetColorInput(
                      FixedFunctionSource.LIGHT_AMBIENT_COLOR),
                  vrmlMaterial.AmbientIntensity);

              var ambientAndDiffuseLightingColor = colorOps.Add(
                  colorOps.Multiply(ambientSurfaceColor, ambientLightColor),
                  diffuseLightColor);

              // We double it because all the other kids do. (Other fixed-function games.)
              ambientAndDiffuseLightingColor =
                  colorOps.MultiplyWithConstant(ambientAndDiffuseLightingColor,
                                                2);

              var ambientAndDiffuseComponent = colorOps.Multiply(
                  ambientAndDiffuseLightingColor,
                  diffuseSurfaceColor);

              var outputColor = ambientAndDiffuseComponent;
              var outputAlpha = diffuseSurfaceAlpha;

              equations.CreateColorOutput(FixedFunctionSource.OUTPUT_COLOR,
                                          outputColor ?? colorOps.Zero);
              equations.CreateScalarOutput(FixedFunctionSource.OUTPUT_ALPHA,
                                           outputAlpha ?? scalarOps.Zero);

              finMaterial.TransparencyType = alpha < 1
                  ? TransparencyType.TRANSPARENT
                  : finMaterial.Textures.FirstOrDefault()?.TransparencyType ??
                    TransparencyType.OPAQUE;

              finMaterial.SetDefaultAlphaCompare();

              return finMaterial;
            });

    var finSkeleton = finModel.Skeleton;
    var finSkin = finModel.Skin;

    var allVrmlNodes = vrmlScene.GetAllChildren().ToArray();
    var vertexOrdering
        = allVrmlNodes.WhereIs<INode, ShapeHintsNode>()
                      .SingleOrDefault()
                      ?.VertexOrdering ??
          VertexOrder.COUNTER_CLOCKWISE;

    var maxCycleInterval = allVrmlNodes.WhereIs<INode, TimeSensorNode>()
                                       .Select(t => t.CycleInterval)
                                       .MaxOrDefault();

    IModelAnimation? animation = null;
    if (allVrmlNodes.WhereIs<INode, OrientationInterpolatorNode>().Any() ||
        allVrmlNodes.WhereIs<INode, PositionInterpolatorNode>().Any()) {
      animation = finModel.AnimationManager.AddAnimation();
      animation.FrameRate = 30;
      animation.FrameCount = (int) (animation.FrameRate * maxCycleInterval);
    }

    var translationTracksByName = new Dictionary<string, IBoneTracks>();
    var rotationTracksByName = new Dictionary<string, IBoneTracks>();
    var translationBoneNames = new HashSet<string?>();
    var rotationBoneNames = new HashSet<string?>();
    foreach (var routeNode in allVrmlNodes.WhereIs<INode, RouteNode>()) {
      if (routeNode.Dst.TryRemoveEnd(".translation",
                                     out var translationBoneName)) {
        translationBoneNames.Add(translationBoneName);
      }

      if (routeNode.Dst.TryRemoveEnd(".rotation", out var rotationBoneName)) {
        rotationBoneNames.Add(rotationBoneName);
      }
    }

    var nodeQueue = new FinTuple2Queue<INode, IBone>(
        vrmlScene.Children.Select(n => (n, finSkeleton.Root)));
    while (nodeQueue.TryDequeue(out var vrmlNode, out var finParentBone)) {
      var finBone = finParentBone;

      if (vrmlNode is ITransform transform) {
        // T × C × R × SR × S × -SR × -C
        var translation = transform.Translation;
        var isTranslationBone
            = translationBoneNames.Contains(transform.DefName);
        if (!translation.IsRoughly0() || isTranslationBone) {
          var translationBone
              = finBone = finBone.AddChild(transform.Translation);
          if (isTranslationBone) {
            var translationTracks = animation.AddBoneTracks(translationBone);
            translationTracksByName[transform.DefName!] = translationTracks;
          }
        }

        var center = transform.Center;
        if (center != null && !center.Value.IsRoughly0()) {
          finBone = finBone.AddChild(center.Value);
        }

        var rotation = transform.Rotation;
        var isRotationBone
            = rotationBoneNames.Contains(transform.DefName);
        if ((rotation != null && rotation != Quaternion.Identity) ||
            isRotationBone) {
          var rotationBone = finBone = finBone.AddChild(
              SystemMatrix4x4Util.FromRotation(
                  rotation ?? Quaternion.Identity));
          if (isRotationBone) {
            var rotationTracks = animation.AddBoneTracks(rotationBone);
            rotationTracksByName[transform.DefName!] = rotationTracks;
          }
        }

        var scaleOrientation = transform.ScaleOrientation;
        if (scaleOrientation != null &&
            scaleOrientation != Quaternion.Identity) {
          finBone = finBone.AddChild(
              SystemMatrix4x4Util.FromRotation(scaleOrientation.Value));
        }

        var scale = transform.Scale;
        if (scale != null && !scale.Value.IsRoughly1()) {
          finBone = finBone.AddChild(
              SystemMatrix4x4Util.FromScale(scale.Value));
        }

        if (scaleOrientation != null &&
            scaleOrientation != Quaternion.Identity) {
          finBone = finBone.AddChild(
              FinMatrix4x4Util.FromRotation(scaleOrientation.Value)
                              .InvertInPlace());
        }

        if (center != null && !center.Value.IsRoughly0()) {
          finBone = finBone.AddChild(-center.Value);
        }
      }

      switch (vrmlNode) {
        case IIsbPictureNode pictureNode: {
          var image = pictureNode.Frames[0];
          var appearance = new AppearanceNode {
              Material = new MaterialNode(),
              Texture = image,
          };

          var finMaterial = lazyMaterialDictionary[(appearance, null)];

          var boneWeights = finSkin.GetOrCreateBoneWeights(
              VertexSpace.RELATIVE_TO_BONE,
              finBone);

          var vtx0 = finSkin.AddVertex(0, 0, 1);
          vtx0.SetUv(0, 1 - 0);
          vtx0.SetBoneWeights(boneWeights);
          var vtx1 = finSkin.AddVertex(1, 0, 1);
          vtx1.SetUv(1, 1 - 0);
          vtx1.SetBoneWeights(boneWeights);
          var vtx2 = finSkin.AddVertex(1, 1, 1);
          vtx2.SetUv(1, 1 - 1);
          vtx2.SetBoneWeights(boneWeights);
          var vtx3 = finSkin.AddVertex(0, 1, 1);
          vtx3.SetUv(0, 1 - 1);
          vtx3.SetBoneWeights(boneWeights);

          var finMesh = finSkin.AddMesh();
          var finPrimitive = finMesh.AddQuads([vtx0, vtx1, vtx2, vtx3]);
          finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
          finPrimitive.SetMaterial(finMaterial);

          AddFaceNormal_([vtx0, vtx1, vtx2, vtx3], finPrimitive.VertexOrder);

          break;
        }
        case IShapeNode shapeNode: {
          var geometry = shapeNode.Geometry;
          var finMaterial = lazyMaterialDictionary[(shapeNode.Appearance,
                                                    geometry as TextNode)];
          var finMesh = finSkin.AddMesh();

          switch (geometry) {
            case IndexedFaceSetNode indexedFaceSetNode: {
              foreach (var faceVertices in GetIndexFaceSetCoordGroups_(
                       indexedFaceSetNode)) {
                var finVertices = new LinkedList<INormalVertex>();
                foreach (var vrmlVertex in faceVertices) {
                  var (coordIndex, texCoordIndex, colorIndex) = vrmlVertex;

                  var coord = indexedFaceSetNode.Coord.Point[coordIndex];
                  var texCoord = texCoordIndex != null
                      ? indexedFaceSetNode.TexCoord?.Point[texCoordIndex.Value]
                      : null;
                  var color = texCoordIndex != null
                      ? indexedFaceSetNode.Color?.Color[colorIndex.Value]
                      : null;

                  var finVertex = finSkin.AddVertex(coord);
                  if (texCoord != null) {
                    finVertex.SetUv(texCoord.Value.X, 1 - texCoord.Value.Y);
                  }

                  if (color != null) {
                    var finColor = FinColor.FromRgbFloats(color.Value.X,
                      color.Value.Y,
                      color.Value.Z);
                    finVertex.SetColor(finColor);
                  }

                  finVertex.SetBoneWeights(
                      finSkin.GetOrCreateBoneWeights(
                          VertexSpace.RELATIVE_TO_BONE,
                          finBone));

                  finVertices.AddLast(finVertex);
                }

                var finVerticesArray = finVertices.ToArray();
                if (finVerticesArray.Length >= 3) {
                  IPrimitive finPrimitive;
                  if (finVertices.Count == 3) {
                    AddFaceNormal_(finVerticesArray, vertexOrdering);
                    finPrimitive = finMesh.AddTriangles(finVerticesArray);
                  } else if (finVertices.Count == 4) {
                    AddFaceNormal_(finVerticesArray, vertexOrdering);
                    finPrimitive = finMesh.AddQuads(finVerticesArray);
                  } else {
                    var triangulatedVertices
                        = TriangulateVertices_(finVertices.ToArray());
                    AddFaceNormal_(triangulatedVertices, vertexOrdering);

                    finPrimitive = finMesh.AddTriangles(triangulatedVertices);
                  }

                  finPrimitive.SetVertexOrder(vertexOrdering)
                              .SetMaterial(finMaterial);
                }
              }
              break;
            }
            case TextNode textNode: {
              var finTexture = lazyTextTextureDictionary[textNode];

              var scale = 1 / fontSize;
              scale *= .75f;

              var width = finTexture.Image.Width * scale;
              var height = finTexture.Image.Height * scale;

              var depth = .05f;

              var point1 = new Vector3(-width / 2f, height / 2f, depth);
              var point2 = new Vector3(width / 2f, -height / 2f, depth);

              switch (textNode.FontStyle.Justify) {
                case Justify.BEGIN: {
                  point1 += new Vector3(width / 2, 0, 0);
                  point2 += new Vector3(width / 2, 0, 0);
                  break;
                }
                case Justify.END: {
                  point1 -= new Vector3(width / 2, 0, 0);
                  point2 -= new Vector3(width / 2, 0, 0);
                  break;
                }
              }

              finMesh.AddSimpleFloor(finSkin,
                                     point1,
                                     point2,
                                     finMaterial,
                                     finBone);

              break;
            }
          }

          break;
        }
      }

      if (vrmlNode is IGroupNode groupNode) {
        nodeQueue.Enqueue(groupNode.Children.Select(n => (n, finBone)));
      }
    }

    foreach (var routeNode in allVrmlNodes.WhereIs<INode, RouteNode>()) {
      if (!routeNode.Src.TryRemoveEnd(".value_changed", out var srcName)) {
        continue;
      }

      if (routeNode.Dst.TryRemoveEnd(".translation",
                                     out var translationBoneName)) {
        var translationTracks = translationTracksByName[translationBoneName]
            .UseCombinedTranslationKeyframes();
        var srcNode = definitions[srcName];
        var positionInterpolator
            = srcNode.AssertAsA<PositionInterpolatorNode>();
        foreach (var (frame, value) in positionInterpolator.Keyframes) {
          translationTracks.Add(
              new Keyframe<Vector3>(animation.FrameCount * frame, value));
        }
      }

      if (routeNode.Dst.TryRemoveEnd(".rotation", out var rotationBoneName)) {
        var rotationTracks = rotationTracksByName[rotationBoneName]
            .UseCombinedQuaternionKeyframes();
        var srcNode = definitions[srcName];
        var orientationInterpolator
            = srcNode.AssertAsA<OrientationInterpolatorNode>();
        foreach (var (frame, value) in orientationInterpolator.Keyframes) {
          rotationTracks.Add(
              new Keyframe<Quaternion>(animation.FrameCount * frame, value));
        }
      }
    }

    return finModel;
  }

  private static INormalVertex[] TriangulateVertices_(
      INormalVertex[] finVertices) {
    var points3d = finVertices;
    var points2d
        = CoplanarPointFlattener.FlattenCoplanarPoints(
            points3d.Select(t => t.LocalPosition).ToArray());

    try {
      var vec3sWithIndices = new List<Vector3>();
      foreach (var point2d in points2d) {
        var vec3 = new Vector3(point2d.X, point2d.Y, 0);
        vec3sWithIndices.Add(vec3);
      }

      var earClipping = new EarClipping();
      earClipping.SetPoints(vec3sWithIndices);
      earClipping.Triangulate();

      return earClipping
             .Result
             .Select(i => points3d[i])
             .ToArray();
    } catch {
      var delaunator = new Delaunator(
          points2d
              .Select((p, i) => (IPoint) new PointWithIndex(
                          p.X,
                          p.Y,
                          i))
              .ToArray());
      return delaunator
             .GetTriangles()
             .SelectMany(t => t.Points.Select(
                                   p => finVertices[
                                       p.AssertAsA<PointWithIndex>().Index])
                               .Reverse())
             .ToArray();
    }
  }

  private struct PointWithIndex(double x, double y, int index) : IPoint {
    public double X { get; set; } = x;
    public double Y { get; set; } = y;
    public int Index => index;
  }

  private static IEnumerable<IndexedFaceGroup[]> GetIndexFaceSetCoordGroups_(
      IndexedFaceSetNode indexedFaceSetNode) {
    foreach (var indexedFaceSetGroup in GetIndexFaceSetCoords_(
                     indexedFaceSetNode)
                 .SplitByNull()) {
      if (indexedFaceSetGroup.Length == 0) {
        continue;
      }

      yield return indexedFaceSetGroup;
    }
  }

  private static IEnumerable<IndexedFaceGroup?> GetIndexFaceSetCoords_(
      IndexedFaceSetNode indexedFaceSetNode) {
    var colorPerVertex = indexedFaceSetNode.ColorPerVertex ?? true;
    var coordIndex = indexedFaceSetNode.CoordIndex;
    var texCoordIndex = indexedFaceSetNode.TexCoordIndex;

    var primitiveIndex = 0;
    for (var i = 0; i < indexedFaceSetNode.CoordIndex.Count; ++i) {
      if (coordIndex[i] == -1) {
        primitiveIndex++;
        yield return null;
      } else {
        yield return (coordIndex[i],
                      texCoordIndex?[i],
                      !colorPerVertex ? primitiveIndex : null);
      }
    }
  }

  private static void AddFaceNormal_(IReadOnlyList<INormalVertex> vertices,
                                     VertexOrder vertexOrder) {
    var a = vertices[0].LocalPosition;
    var b = vertices[1].LocalPosition;
    var c = vertices[2].LocalPosition;

    var normal = Vector3.Cross(b - a, c - a);
    normal = Vector3.Normalize(normal);

    if (vertexOrder == VertexOrder.CLOCKWISE) {
      normal *= -1;
    }

    foreach (var vertex in vertices) {
      vertex.SetLocalNormal(normal);
    }
  }
}