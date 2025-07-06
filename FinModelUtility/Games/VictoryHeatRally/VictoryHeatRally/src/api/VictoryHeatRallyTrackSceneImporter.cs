using System.Numerics;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math;
using fin.math.splines;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.scene;
using fin.util.asserts;
using fin.util.sets;

using gm.api;

using Newtonsoft.Json;


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

    var lazySpriteImages = new LazyCaseInvariantStringDictionary<IImage>(
        spriteName => {
          if (!spriteDirectory.TryToGetExistingFile(
                  $"{spriteName}.png",
                  out var spriteFile)) {
            spriteFile =
                spriteDirectory.AssertGetExistingFile($"{spriteName}_0.png");
          }

          return FinImage.FromFile(spriteFile);
        });

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
      textureMaterial.CullingMode = CullingMode.SHOW_BOTH;
      texture.MinFilter = TextureMinFilter.NEAR;
      texture.MagFilter = TextureMagFilter.NEAR;

      trackModel.Skin.Meshes[0].Primitives[0].SetMaterial(textureMaterial);

      var trackObject = finArea.AddObject();
      trackObject.AddSceneModel(trackModel);
    }

    var rawJsonLines = trackJsonFile.ReadAllLines();
    var validJson = $"[{string.Join(',', rawJsonLines)}]";

    var trackItems = JsonConvert.DeserializeObject<List<TrackItem>>(validJson,
      new JsonSerializerSettings {
          Converters = [new SingleOrArrayConverter<string>()]
      })!;

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
      var myStruct = trackItem.my_struct;

      var position =
          myStruct != null
              ? myStruct.follow.IsTruthy()
                  ? nodesSpline.GetPositionAtOffset(
                      myStruct!.position!.Value * 32)
                  : new Vector3(myStruct.x ?? 0,
                                myStruct.y ?? 0,
                                myStruct.z ?? 0)
              : Vector3.Zero;

      var xScale = (myStruct?.scale ?? 1) *
                   (myStruct?.image_xscale ?? 1) *
                   (myStruct?.xscale ?? 1);
      var yScale = (myStruct?.scale ?? 1) *
                   (myStruct?.image_yscale ?? 1) *
                   (myStruct?.yscale ?? 1);

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

          var spriteIndex = trackItem.my_struct.sprite_index.AssertNonnull();
          var spriteImage = lazySpriteImages[spriteIndex];

          var (spriteMaterial, _) = spriteModel.MaterialManager
                                               .AddSimpleTextureMaterialFromImage(
                                                   spriteImage);

          var spriteRootBone = spriteModel.Skeleton.Root;
          spriteRootBone.AlwaysFaceTowardsCamera(Quaternion.Identity);

          var adjBone = spriteRootBone.AddChild(0, 0, 0);
          adjBone.LocalTransform.EulerRadians = new Vector3(MathF.PI / 2, 0, 0);

          var width = spriteImage.Width * xScale;
          var height = spriteImage.Height * yScale;

          var spriteSkin = spriteModel.Skin;
          var spriteMesh = spriteSkin.AddMesh();
          spriteMesh.AddSimpleWall(spriteSkin,
                                   new Vector3(0, -width / 2f, -height),
                                   new Vector3(0, width / 2f, 0),
                                   spriteMaterial,
                                   adjBone);

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

    [JsonConverter(typeof(SingleOrArrayConverter<string>))]
    public List<string>? sprite;

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