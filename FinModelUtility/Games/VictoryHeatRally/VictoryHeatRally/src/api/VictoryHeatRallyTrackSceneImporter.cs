using System.Globalization;
using System.Numerics;
using System.Text.Json;

using fin.io;
using fin.math.splines;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.util.sets;

using gm.api;

using Newtonsoft.Json;

using static vhr.api.VictoryHeatRallyTrackSceneImporter;

using JsonSerializer = Newtonsoft.Json.JsonSerializer;


namespace vhr.api;

public class VictoryHeatRallyTrackSceneFileBundle : ISceneFileBundle {
  public IReadOnlyTreeFile TrackJsonFile { get; set; }
  public IReadOnlyTreeDirectory ExtractedDirectory { get; set; }
  public IReadOnlyTreeDirectory DataDirectory { get; set; }
  public IReadOnlyTreeFile? MainFile => TrackJsonFile;
}

public class VictoryHeatRallyTrackSceneImporter
    : ISceneImporter<VictoryHeatRallyTrackSceneFileBundle> {
  public IScene Import(VictoryHeatRallyTrackSceneFileBundle fileBundle) {
    var trackJsonFile = fileBundle.TrackJsonFile;

    var fileSet = fileBundle.MainFile.AsFileSet();
    var finScene = new SceneImpl {FileBundle = fileBundle, Files = fileSet};

    var finArea = finScene.AddArea();
    finScene.CreateDefaultLighting(finArea.AddObject());

    var dataDirectory = fileBundle.DataDirectory;
    var spriteDirectory =
        fileBundle.ExtractedDirectory.AssertGetExistingSubdir("dataWin\\sprt");

    {
      var vbFile = dataDirectory.AssertGetExistingFile(
          Path.Join("TRK\\MODEL",
                    $"{trackJsonFile.NameWithoutExtension}.vbuff"));
      fileSet.Add(vbFile);

      var trackModel =
          new VbModelImporter().Import(new VbModelFileBundle(vbFile));

      var (textureMaterial, texture) =
          trackModel.MaterialManager.AddSimpleTextureMaterialFromFile(
              spriteDirectory.AssertGetExistingFile("spr_roadtex_0.png"));
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;

      trackModel.Skin.Meshes[0].Primitives[0].SetMaterial(textureMaterial);

      var trackObject = finArea.AddObject();
      trackObject.AddSceneModel(trackModel);
    }

    var rawJsonLines = trackJsonFile.ReadAllLines();
    var validJson = $"[{string.Join(',', rawJsonLines)}]";

    var trackItems = JsonConvert.DeserializeObject<List<TrackItem>>(validJson)!;

    var nodes = trackItems.Where(i => i.type is "Node").ToArray();
    var nodePositions = nodes.Select(n => new Vector3(
                                         n.my_array[0],
                                         -n.my_array[2],
                                         n.my_array[1]))
                             .ToArray();
    {
      var nodesModel = new ModelImpl {FileBundle = fileBundle, Files = fileSet};
      var nodesMaterial = nodesModel.MaterialManager.AddNullMaterial();
      nodesMaterial.DepthMode = DepthMode.NONE;
      nodesMaterial.DepthCompareType = DepthCompareType.Always;

      var nodesSkin = nodesModel.Skin;
      var nodeVertices =
          nodePositions.Select(n => nodesSkin.AddVertex(n)).ToArray();

      var nodesMesh = nodesSkin.AddMesh();
      nodesMesh.AddLineStrip(nodeVertices);

      var nodesObject = finArea.AddObject();
      nodesObject.AddSceneModel(nodesModel);
    }

    var nodesSpline = new LinearSpline(nodePositions);
    var visibleItems =
        trackItems.Where(i => i.type is "Model" or "Object" or "Sprite");
    foreach (var trackItem in visibleItems) {
      var position =
          nodesSpline.GetPositionAtOffset(trackItem.my_struct!.position!.Value);

      switch (trackItem.type) {
        case "Model": {
          break;
        }
        case "Object": {
          break;
        }
        case "Sprite": {
          var spriteModel = new ModelImpl
              {FileBundle = fileBundle, Files = fileSet};

          var nodesMaterial = spriteModel.MaterialManager.AddNullMaterial();
          nodesMaterial.DepthMode = DepthMode.NONE;
          nodesMaterial.DepthCompareType = DepthCompareType.Always;

          spriteModel.Skeleton.Root
                     .AlwaysFaceTowardsCamera(Quaternion.Identity);

          var pt = new Vector3(32, 0, 32);

          var spriteSkin = spriteModel.Skin;
          var spriteMesh = spriteSkin.AddMesh();
          spriteMesh.AddSimpleWall(spriteSkin,
                                   -pt,
                                   pt,
                                   nodesMaterial);

          var spriteObject = finArea.AddObject();
          spriteObject.SetPosition(position);
          spriteObject.AddSceneModel(spriteModel);

          break;
        }
      }
    }

    return finScene;
  }

  private class TrackItem {
    public float[]? my_array;
    public TrackItemStruct? my_struct;
    public string? type;

    // Background
    public string[] bgindex;
    public int[]? xoff;
    public int[]? xparallax;
    public int[]? yoff;
    public int[]? yparallax;

    // Other
    public string? bgm;
    public string? floortex;
    public int? fog_enabled;
    public int? laps;
    public int? rally;
    public int? startpos;
    public string? spr_barrier;
    public int? timeofday;
    public string? track_name;
  }

  private class TrackItemStruct {
    public int? alt_texture;
    public int? flip_x;
    public int? follow;
    public int? image_index;
    public float? image_xscale;
    public float? image_yscale;
    public TrackItemModel? model;
    public int? model_index;
    public string? @object;
    public int? position;
    public float? rotation;
    public float? scale;
    public string? sprite_index;
    public string? type;
    public float? x;
    public float? xoffset;
    public float? xoff_percent;
    public float? xscale;
    public float? y;
    public float? yscale;
    public float? z;
  }

  private class TrackItemModel {
    public bool? array;
    public TrackItemCmesh? cmesh;
    public string? model;

    [JsonConverter(typeof(SingleOrArrayCollectionConverter<string[], string>))]
    public string[]? sprite;

    public int? subdiv;
    public int? tilt;
    public string? texture;
  }

  private class TrackItemCmesh {
    public int? group;
    public int? matrix;
    public string? name;
    public string? shapeList;
    public bool? solid;
    public int? submeshes;
    public int? triangle;
    public float[][]? triangles;
    public int? type;
  }
}