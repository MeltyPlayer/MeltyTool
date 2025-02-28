using System.Drawing;
using System.Numerics;

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
using fin.math.geometry;
using fin.math.matrix.four;
using fin.math.transform;
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

using LibTessDotNet;

using QuickFont;
using QuickFont.Configuration;

using vrml.schema;

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
    var lazyFontDictionary = new LazyDictionary<FontStyleNode, QFont>(
        fontStyleNode => {
          var baseFontName = fontStyleNode.Family switch {
              Family.SANS  => "OpenSans",
              Family.SERIF => "Merriweather",
              _            => throw new ArgumentOutOfRangeException()
          };

          var styleText = fontStyleNode.Style switch {
              Style.BOLD        => "Bold",
              Style.BOLD_ITALIC => "BoldItalic",
              Style.ITALIC      => "Italic",
              Style.PLAIN       => "Regular",
          };

          var fontFile = CommonFiles.COMMON_DIRECTORY.AssertGetExistingFile(
              $"{baseFontName}-{styleText}.ttf");

          return new QFont(
              fontFile.FullPath,
              fontSize,
              new QFontBuilderConfiguration(false));
        });
    var lazyTextTextureDictionary = new LazyDictionary<TextNode, ITexture>(
        textNode => {
          var text = string.Join('\n', textNode.String);

          var fontAlignment = textNode.FontStyle.MajorJustify switch {
              Justify.BEGIN or Justify.FIRST => QFontAlignment.Left,
              Justify.MIDDLE                 => QFontAlignment.Centre,
              Justify.END                    => QFontAlignment.Right,
          };

          var qFont = lazyFontDictionary[textNode.FontStyle];
          using var glTextTexture
              = new GlTextTexture(text, qFont, Color.White, fontAlignment);
          var image = glTextTexture.ConvertToImage(true);

          var finTexture = finModel.MaterialManager.CreateTexture(image);
          finTexture.Name = text;

          return finTexture;
        });
    var lazyMaterialDictionary
        = new LazyDictionary<(AppearanceNode, TextNode?, bool hasVertexColor),
            IMaterial>(
            tuple => {
              var (appearanceNode, textNode, hasVertexColor) = tuple;
              var vrmlMaterial = appearanceNode.Material;

              var color = vrmlMaterial.DiffuseColor;
              var alpha = 1 - vrmlMaterial.Transparency;

              var finMaterial
                  = finModel.MaterialManager.AddFixedFunctionMaterial();
              finMaterial.DepthCompareType = DepthCompareType.Less;

              var equations = finMaterial.Equations;
              var colorOps = equations.ColorOps;
              var scalarOps = equations.ScalarOps;

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
              }

              var (diffuseSurfaceColor, diffuseSurfaceAlpha)
                  = finMaterial.GenerateDiffuse(
                      (equations.CreateColorConstant(color),
                       equations.CreateScalarConstant(alpha)),
                      finTexture,
                      (hasVertexColor, false));

              IColorValue? ambientColor = vrmlMaterial.AmbientColor != null
                  ? equations.CreateColorConstant(
                      vrmlMaterial.AmbientColor.Value)
                  : colorOps.One;
              ambientColor = colorOps.MultiplyWithConstant(
                  ambientColor,
                  vrmlMaterial.AmbientIntensity);

              var outputColorAlpha = equations.GenerateLighting(
                  (diffuseSurfaceColor, diffuseSurfaceAlpha),
                  ambientColor);

              equations.SetOutputColorAlpha(outputColorAlpha);

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
            var translationTracks = animation.GetOrCreateBoneTracks(translationBone);
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
          var rotationBone = finBone = finBone.AddChild(Vector3.Zero);
          rotationBone.LocalTransform.SetRotation(
              rotation ?? Quaternion.Identity);
          if (isRotationBone) {
            var rotationTracks = animation.GetOrCreateBoneTracks(rotationBone);
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

          var finMaterial = lazyMaterialDictionary[(appearance, null, false)];

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
          var hasVertexColor
              = (shapeNode.Geometry as IndexedFaceSetNode)?.Color != null;
          var finMaterial = lazyMaterialDictionary[(shapeNode.Appearance,
                                                    geometry as TextNode,
                                                    hasVertexColor)];
          var finMesh = finSkin.AddMesh();

          switch (geometry) {
            case BoxNode boxNode: {
              finMesh.AddSimpleCube(finSkin,
                                    -boxNode.Size / 2,
                                    boxNode.Size / 2,
                                    finMaterial,
                                    finBone);
              break;
            }
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
                  var color = colorIndex != null
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
            case SphereNode sphereNode: {
              finMesh.AddSimpleSphere(finSkin,
                                      Vector3.Zero,
                                      sphereNode.Radius,
                                      8,
                                      finMaterial,
                                      finBone);
              break;
            }
            case TextNode textNode: {
              var scale = 1 / fontSize;
              scale *= .6f; //.75
              scale *= textNode.FontStyle.Size;

              var font = lazyFontDictionary[textNode.FontStyle];

              var firstLineHeight
                  = font.Measure(textNode.String[0]).Height * scale;

              var text = string.Join('\n', textNode.String);
              var size = font.Measure(text);
              var width = size.Width * scale;
              var height = size.Height * scale;

              var depth = .05f;

              var point1 = new Vector3(-width / 2f, firstLineHeight, depth);
              var point2
                  = new Vector3(width / 2f, firstLineHeight - height, depth);

              switch (textNode.FontStyle.MajorJustify) {
                case Justify.BEGIN or Justify.FIRST: {
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
    var tess = new Tess();
    tess.AddContour(finVertices.Select(v => {
                                 var p = v.LocalPosition;
                                 return new ContourVertex(
                                     new Vec3(p.X, p.Y, p.Z),
                                     v);
                               })
                               .ToArray());

    tess.Tessellate();

    foreach (var finVertex in finVertices) {
      finVertex.SetLocalNormal(tess.Normal.X, tess.Normal.Y, tess.Normal.Z);
    }

    var allVertices = tess.Vertices
                          .Select(v => v.Data.AssertAsA<INormalVertex>())
                          .ToArray();
    return tess.Elements.Select(e => allVertices[e]).ToArray();
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

    var normal = TriangleUtil.CalculateNormal(a, b, c);
    if (vertexOrder == VertexOrder.CLOCKWISE) {
      normal *= -1;
    }

    foreach (var vertex in vertices) {
      vertex.SetLocalNormal(normal);
    }
  }
}