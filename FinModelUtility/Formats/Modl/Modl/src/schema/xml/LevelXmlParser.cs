using System.Collections.Concurrent;
using System.Drawing;
using System.IO.Compression;
using System.Numerics;
using System.Xml;

using fin.color;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.scene;
using fin.util.sets;

using modl.api;
using modl.schema.terrain;

namespace modl.schema.xml;

public interface IBwObject {
  string Id { get; }
  string? ModelName { get; set; }
}

public class LevelObject : IBwObject {
  public required string Id { get; init; }
  public string? ModelName { get; set; }

  public IReadOnlyFinMatrix4x4? Matrix { get; set; }

  public LinkedList<string> Children { get; } = [];

  public string? NextLinkId { get; set; } = null;
  public bool? StickToFloor { get; set; } = null;

  public void AddChild(string child) {
    if (child == "0") {
      return;
    }

    this.Children.AddLast(child);
  }
}

public class SkydomeObject : IBwObject {
  public required string Id { get; init; }
  public string? ModelName { get; set; }
}

public class LevelXmlParser {
  public IScene Parse(BwSceneFileBundle bwSceneFileBundle) {
    var mainXmlFile = bwSceneFileBundle.MainXmlFile;
    var gameVersion = bwSceneFileBundle.GameVersion;

    var files = mainXmlFile.AsFileSet();
    var scene = new SceneImpl {
        FileBundle = bwSceneFileBundle,
        Files = files
    };
    var sceneArea = scene.AddArea();

    var mainXml = new XmlDocument();
    mainXml.LoadXml(mainXmlFile.ReadAllText());

    var mainXmlDirectory = mainXmlFile.AssertGetParent();

    var levelfilesTag = mainXml["levelfiles"];

    var levelFilename =
        levelfilesTag["level"]["objectfiles"]["file"].GetAttribute("name");
    var levelXmlFile = mainXmlDirectory.AssertGetExistingFile(levelFilename);
    files.Add(levelXmlFile);

    var objectTags = this.ReadLevelXmlObjectTags_(levelXmlFile, gameVersion);

    this.ParseLightingAndLightScale_(
        scene,
        objectTags,
        out var terrainLightScale);

    var objectMap = this.ParseObjectMap_(objectTags);

    IBwTerrain bwTerrain;
    {
      var terrainFilename =
          levelfilesTag["terrain"]["file"].GetAttribute("name");
      var outFile = mainXmlDirectory.AssertGetExistingFile(terrainFilename);
      files.Add(outFile);
      this.AddTerrain_(sceneArea,
                       bwSceneFileBundle,
                       outFile,
                       out bwTerrain,
                       terrainLightScale);
    }

    {
      this.AddObjects_(files,
                       sceneArea,
                       levelXmlFile,
                       gameVersion,
                       bwTerrain,
                       objectMap);
    }

    return scene;
  }

  // TODO: Properly parse this into a structure, don't handle the raw XML
  private XmlNode[] ReadLevelXmlObjectTags_(
      IReadOnlyTreeFile levelXmlFile,
      GameVersion gameVersion) {
    Stream levelXmlStream;
    if (gameVersion == GameVersion.BW2) {
      using var gZipStream =
          new GZipStream(levelXmlFile.OpenRead(),
                         CompressionMode.Decompress);

      levelXmlStream = new MemoryStream();
      gZipStream.CopyTo(levelXmlStream);
      levelXmlStream.Position = 0;
    } else {
      levelXmlStream = levelXmlFile.OpenRead();
    }

    using var levelXmlReader = new StreamReader(levelXmlStream);
    var levelXml = new XmlDocument();
    levelXml.LoadXml(levelXmlReader.ReadToEnd());

    var instances = levelXml["Instances"];

    var objectTags = instances.GetElementsByTagName("Object");
    return objectTags.Cast<XmlNode>().ToArray();
  }

  private void ParseLightingAndLightScale_(
      IScene scene,
      XmlNode[] objectTags,
      out float terrainLightScale) {
    terrainLightScale = 1;

    foreach (var objectTag in objectTags) {
      var objectType = objectTag.Attributes["type"].Value;

      if (objectType == "cRenderParams") {
        // TODO: Handle fog color

        terrainLightScale =
            float.Parse(objectTag.GetAttributeValue("mTerrainLightScale"));

        var lighting = scene.CreateLighting();
        lighting.AmbientLightColor =
            objectTag.GetAttributeLightColor("mSunAmbientColor");

        {
          var sunLight = lighting.CreateLight();
          sunLight.SetColor(
              objectTag.GetAttributeLightColor("mSunDirectionalColor"));

          var sunYawRadians =
              float.Parse(objectTag.GetAttributeValue("mSunRotation"));
          var sunPitchRadians =
              MathF.Asin(
                  float.Parse(objectTag.GetAttributeValue("mSunElevation")));
          FinTrig.FromPitchYawRadians(sunPitchRadians,
                                      sunYawRadians,
                                      out var sunXNormal,
                                      out var sunYNormal,
                                      out var sunZNormal);
          sunLight.SetNormal(new Vector3(-sunXNormal, -sunYNormal, -sunZNormal));
        }

        {
          var antiSunLight = lighting.CreateLight();
          antiSunLight.SetColor(
              objectTag.GetAttributeLightColor("mAntiSunDirectionalColor"));

          var antiSunYawRadians =
              float.Parse(objectTag.GetAttributeValue("mAntiSunRotation"));
          var antiSunPitchRadians =
              MathF.Asin(
                  float.Parse(
                      objectTag.GetAttributeValue("mAntiSunElevation")));
          FinTrig.FromPitchYawRadians(antiSunPitchRadians,
                                      antiSunYawRadians,
                                      out var antiSunXNormal,
                                      out var antiSunYNormal,
                                      out var antiSunZNormal);
          antiSunLight.SetNormal(
              new Vector3(-antiSunXNormal, -antiSunYNormal, -antiSunZNormal));
        }
      }
    }
  }

  private IDictionary<string, IBwObject> ParseObjectMap_(
      XmlNode[] objectTags) {
    var objectsById = new Dictionary<string, IBwObject>();
    var types = new HashSet<string>();

    foreach (var objectTag in objectTags) {
      LevelObject? node = null;

      var objectType = objectTag.Attributes["type"].Value;
      types.Add(objectType);

      if (objectType == "cRenderParams") {
        var skydomeId = objectTag.GetAttributeValue("mpWorldSkydome");
        if (objectsById.TryGetValue(skydomeId, out var skydomeModelObject)) {
          var skydomeModelName = skydomeModelObject.ModelName;
          objectsById[skydomeId] = new SkydomeObject {
              Id = skydomeId, ModelName = skydomeModelName,
          };
        }

        continue;
      }

      var isUsefulNode = false;

      var objId = objectTag.Attributes["id"].Value;
      foreach (var childTag in objectTag.Children()) {
        var childNameAttribute = childTag.Attributes["name"]?.Value;

        switch (childTag.Name) {
          case "Attribute": {
            if (childNameAttribute is "mMatrix" or "Mat") {
              var floats = new float[16];
              var floatsText = childTag["Item"].InnerText;

              var currentIndex = 0;
              for (var fI = 0; fI < floats.Length; ++fI) {
                var nextCommaIndex = floatsText.IndexOf(',', currentIndex);

                var subText =
                    nextCommaIndex > 0
                        ? floatsText.Substring(currentIndex,
                                               nextCommaIndex - currentIndex)
                        : floatsText.Substring(currentIndex);
                floats[fI] = float.Parse(subText);

                currentIndex = nextCommaIndex + 1;
              }

              isUsefulNode = true;
              node ??= new LevelObject { Id = objId };
              node.Matrix = new FinMatrix4x4(floats.AsSpan());
            } else if (objectType is "cNodeHierarchyResource" &&
                       childNameAttribute is "mName") {
              isUsefulNode = true;
              node ??= new LevelObject { Id = objId };
              node.ModelName = childTag["Item"].InnerText;
            }

            break;
          }
          case "Enum": {
            if (childNameAttribute is "mStickToFloor") {
              node ??= new LevelObject { Id = objId };
              node.StickToFloor = childTag["Item"].InnerText == "eTrue";
            }

            break;
          }
          case "Pointer": {
            if (childNameAttribute is "mBase") {
              isUsefulNode = true;
              node ??= new LevelObject { Id = objId };
              node.AddChild(childTag["Item"].InnerText);
            }

            if (childNameAttribute is "NextLinkObject") {
              var nextLinkId = childTag["Item"].InnerText;
              if (nextLinkId != "0") {
                node ??= new LevelObject { Id = objId };
                node.NextLinkId = nextLinkId;
              }
            }

            break;
          }
          case "Resource": {
            if (childNameAttribute is "mModel"
                                      or "mBAN_Model"
                                      or "Model"
                                      or "model") {
              isUsefulNode = true;
              node ??= new LevelObject { Id = objId };
              node.AddChild(childTag["Item"].InnerText);
            } else if (childNameAttribute is "Element") {
              isUsefulNode = true;
              node ??= new LevelObject { Id = objId };
              var itemNodes = childTag.ChildNodes;
              for (var itemI = 0; itemI < itemNodes.Count; ++itemI) {
                node.AddChild(itemNodes[itemI].InnerText);
              }
            }

            break;
          }
        }
      }

      if (isUsefulNode) {
        objectsById[objId] = node;
      }
    }

    var ids = objectsById.Keys.Select(ulong.Parse).ToList();
    ids.Sort();

    return objectsById;
  }

  private void AddTerrain_(ISceneArea sceneArea,
                           BwSceneFileBundle sceneFileBundle,
                           IReadOnlyTreeFile outFile,
                           out IBwTerrain bwTerrain,
                           float terrainLightScale) {
    sceneArea.AddObject()
             .AddSceneModel(
                 new OutModelImporter().ImportModel(
                     new OutModelFileBundle {
                         GameVersion = sceneFileBundle.GameVersion,
                         OutFile = outFile
                     },
                     outFile,
                     sceneFileBundle.GameVersion,
                     out bwTerrain,
                     terrainLightScale));
  }

  private void AddObjects_(
      ISet<IReadOnlyGenericFile> files,
      ISceneArea sceneArea,
      IReadOnlyTreeFile levelXmlFile,
      GameVersion gameVersion,
      IBwTerrain bwTerrain,
      IDictionary<string, IBwObject> objectMap) {
    var parentDir = levelXmlFile.AssertGetParent();
    var levelDirectory =
        parentDir.GetExistingSubdirs()
                 .Single(
                     subdir => subdir.FullPath.StartsWith(
                         levelXmlFile.FullNameWithoutExtension[..^4]));
    var modelFiles = levelDirectory
                     .GetExistingFiles()
                     .Where(file => file.Name.EndsWith(".modl"))
                     .ToArray();
    var animationFiles = levelDirectory
                         .GetExistingFiles()
                         .Where(file => file.Name.EndsWith(".anim"))
                         .ToArray();

    var fvAnimFiles =
        animationFiles.Where(
                          animFile =>
                              animFile.NameWithoutExtension.StartsWith("FV"))
                      .ToArray();
    var fgAnimFiles =
        animationFiles.Where(
                          animFile =>
                              animFile.NameWithoutExtension.StartsWith("FG"))
                      .ToArray();

    var modlReader = new ModlModelImporter();

    var modelMap = new ConcurrentDictionary<string, IModel>();
    var task = Parallel.ForEachAsync(
        modelFiles,
        async (modelFile, _) => {
          var modelId = modelFile.NameWithoutExtension.ToString();

          IList<IReadOnlyTreeFile>? animFiles = null;
          if (gameVersion == GameVersion.BW1) {
            if (modelId.Length == 4 && modelId.EndsWith("VET")) {
              var firstTwoCharactersInModelId = modelId.Substring(0, 2);
              animFiles = fvAnimFiles
                          .Concat(animationFiles.Where(
                                      file => file.Name.StartsWith(
                                          firstTwoCharactersInModelId)))
                          .ToArray();
            } else if (modelId.Length == 6 && modelId.EndsWith("GRUNT")) {
              var firstTwoCharactersInModelId = modelId.Substring(0, 2);
              animFiles = fgAnimFiles
                          .Concat(animationFiles.Where(
                                      file => file.Name.StartsWith(
                                          firstTwoCharactersInModelId)))
                          .ToArray();
            }
          }

          modelMap[modelId] = await modlReader.ImportModelAsync(
              null,
              modelFile,
              animFiles,
              gameVersion);
        });
    task.ConfigureAwait(false);
    task.Wait();

    files.Add(modelMap.Values.SelectMany(m => m.Files));

    var levelObjMap = new Dictionary<string, ISceneObject>();

    foreach (var obj in objectMap.Values) {
      switch (obj) {
        case SkydomeObject skyboxObj: {
          if (skyboxObj.ModelName != null) {
            sceneArea.BackgroundColor = Color.Black;

            var skydomeModel = modelMap[skyboxObj.ModelName];
            foreach (var finMaterial in skydomeModel.MaterialManager.All) {
              finMaterial.DepthMode = DepthMode.NONE;
              finMaterial.DepthCompareType = DepthCompareType.Always;
            }

            var skydomeObject = sceneArea.CreateCustomSkyboxObject();
            skydomeObject.AddSceneModel(skydomeModel);
            skydomeObject.SetScale(10f, 10f, 10f);
            skydomeObject.Rotation.SetDegrees(90, 0, 0);
          }

          break;
        }
        case LevelObject levelObj: {
          var rootMatrix = levelObj.Matrix;
          if (rootMatrix == null) {
            continue;
          }

          var childIdQueue =
              new FinTuple3Queue<string, (bool?, string?),
                  IReadOnlyFinMatrix4x4>(
                  levelObj.Children.Select(
                      child => (
                          child, (levelObj.StickToFloor, levelObj.NextLinkId),
                          rootMatrix)));
          while (childIdQueue.TryDequeue(out var childId,
                                         out var stickToFloorAndNextLinkId,
                                         out var parentMatrix)) {
            objectMap.TryGetValue(childId, out var genericChild);
            var child = genericChild as LevelObject;
            if (child == null) {
              continue;
            }

            var (stickToFloor, nextLinkId) = stickToFloorAndNextLinkId;
            stickToFloor ??= child.StickToFloor;
            nextLinkId ??= child.NextLinkId;

            IReadOnlyFinMatrix4x4 childMatrix;
            if (child.Matrix == null) {
              childMatrix = parentMatrix;
            } else {
              childMatrix = parentMatrix.CloneAndMultiply(child.Matrix);
            }

            if (child.ModelName != null) {
              var sceneObject = sceneArea.AddObject();

              childMatrix.Decompose(out var translation,
                                    out var rotation,
                                    out var scale);

              if (stickToFloor != false && nextLinkId != null) {
                // Should be an invalid case...?
              }

              if (stickToFloor != false) {
                sceneObject.SetPosition(
                    translation.X,
                    translation.Y +
                    bwTerrain.Heightmap
                             .GetHeightAtPosition(
                                 translation.X,
                                 translation.Z),
                    translation.Z);
              } else if (nextLinkId != null) {
                var positionObj = levelObjMap[nextLinkId];
                sceneObject.SetPosition(
                    translation.X,
                    positionObj.Position.Y,
                    translation.Z);
              } else {
                sceneObject.SetPosition(translation);
              }

              if (child.ModelName.Contains("WA20H")) {
                ;
              }

              levelObjMap[levelObj.Id] = sceneObject;

              sceneObject.Rotation.SetQuaternion(rotation);
              sceneObject.SetScale(scale.X, scale.Y, scale.Z);

              sceneObject.AddSceneModel(modelMap[child.ModelName]);
            }

            childIdQueue.Enqueue(
                child.Children.Select(grandchild
                                          => (grandchild,
                                              (stickToFloor, nextLinkId),
                                              childMatrix)));
          }

          break;
        }
      }
    }
  }
}

public static class XmlExtensions {
  public static IEnumerable<XmlNode> Children(
      this XmlNode xmlNode) => xmlNode.Cast<XmlNode>();

  public static IEnumerable<XmlNode> Children(
      this XmlNodeList xmlNodeList) => xmlNodeList.Cast<XmlNode>();

  public static string GetAttributeValue(this XmlNode xmlNode, string name)
    => xmlNode.Children()
              .Single(child => child.Attributes?["name"].Value == name)
              .FirstChild?.InnerText!;

  public static IColor GetAttributeColor(this XmlNode xmlNode, string name) {
    var bytes = GetAttributeValue(xmlNode, name)
                .Split(',')
                .Select(byte.Parse)
                .ToArray();

    return FinColor.FromRgbaBytes(bytes[0], bytes[1], bytes[2], bytes[3]);
  }

  public static IColor GetAttributeLightColor(this XmlNode xmlNode,
                                              string name) {
    var rgbaColor = GetAttributeColor(xmlNode, name);
    return FinColor.FromRgbFloats(rgbaColor.Rf * rgbaColor.Af,
                                  rgbaColor.Gf * rgbaColor.Af,
                                  rgbaColor.Bf * rgbaColor.Af);
  }
}