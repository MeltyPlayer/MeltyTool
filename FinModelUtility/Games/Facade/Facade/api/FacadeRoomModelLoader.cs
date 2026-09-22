using System.Drawing;
using System.Numerics;

using fin.data.lazy;
using fin.image;
using fin.io;
using fin.math.floats;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enumerables;
using fin.util.sets;

using schema.binary;

namespace facade.api;

public sealed record FacadeRoomModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory PrereqsDirectory)
    : IModelFileBundle<FacadeRoomModelFileBundle, FacadeRoomModelImporter>;

/// <summary>
///   The models are unfortunately hardcoded in the game's logic. They're
///   defined within the function starting at 0x100925a0 in animEngineDLL.dll.
///   These calls have been replicated here.
/// </summary>
public sealed class FacadeRoomModelImporter
    : IModelImporter<FacadeRoomModelImporter, FacadeRoomModelFileBundle> {
  public IModel Import(FacadeRoomModelFileBundle fileBundle)
    => new FacadeRoomModelBuilder(fileBundle).Model;
}

public enum FilmstripId {
  PLANT = 0x0,
  ASHTRAY = 0x1,
  MARTINI_GLASS = 0x2,
  RED_WINE_BOTTLE = 0x3,
  WHITE_WINE_BOTTLE = 0x4,
  PLAYERS_DRINK = 0x6,
  PAINTING_1 = 0x10,
  LITTLE_PAINTING = 0x11,
  WEDDING_PHOTO = 0x12,
  TUSCANY = 0x13,
  GOWN_SKETCH_1 = 0x14,
  POST_ITS = 0x15,
  GOWN_SKETCH_4 = 0x16,
  PHONE = 0x17,
  ANSWERING_MACHINE = 0x18,
  EIGHT_BALL = 0x19,
  BRASS_BULL = 0x1A,
  TRINKET_1 = 0x1B,
  TRINKET_2 = 0x1C,
  TRINKET_3 = 0x1D,
  TRINKET_4 = 0x1E,
  TRINKET_5 = 0x1F,
  TRINKET_6 = 0x20,
  TRINKET_7 = 0x21,
  TRINKET_8 = 0x22,
  RUG = 0x23,
  FRONT_DOOR = 0x24,
  BEDROOM_DOOR = 0x25,
  ELEVATOR = 0x26,
  ELEVATOR_BUTTON = 0x27,
  WHITE_CABINET_DOOR = 0x2C,
  WHITE_CABINET_DOOR_2 = 0x2D,
  NYC = 0x2E,
}

public enum ColorId {
  FRIDGE_BOTTOM = 0x10,
  WINDOW = 0x20,
  FRIDGE = 0x24,
  COLOR_0x2E = 0x2E,
  COLOR_0x2F = 0x2F,
  COLOR_0x30 = 0x30,
  COLOR_0x31 = 0x31,
  COLOR_0x32 = 0x32,
  COLOR_0x33 = 0x33,
  LIVING_ROOM_FRONT_WALL = 0x34,
  FOYER_WALL = 0x35,
  COLOR_0x36 = 0x36,
  COLOR_0x37 = 0x37,
  CEILING = 0x38,
  COLOR_0x39 = 0x39,
  COLOR_0x3A = 0x3A,
  FLOOR = 0x3b,
  LIVING_ROOM_WALL = 0xFF,
}

file class FacadeRoomModelBuilder {
  private readonly FacadeRoomModelFileBundle fileBundle_;

  private readonly
      LazyDictionary<(
          FilmstripId? filmstripId,
          ColorId? colorId,
          bool removeBackground,
          bool isLine),
          IReadOnlyMaterial> lazyMaterials_;

  public ModelImpl Model { get; }

  public FacadeRoomModelBuilder(FacadeRoomModelFileBundle fileBundle) {
    this.fileBundle_ = fileBundle;

    var files = fileBundle.MainFile.AsFileSet();
    this.Model = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    this.lazyMaterials_
        = new LazyDictionary<(FilmstripId? filmstripId, ColorId? colorId, bool
            removeBackground, bool isLine),
            IReadOnlyMaterial>(tuple => {
          var (filmstripId, colorId, removeBackground, isLine) = tuple;

          var textureFileName = filmstripId switch {
              FilmstripId.PLANT                => "plant.bmp",
              FilmstripId.ASHTRAY              => "ashtray.bmp",
              FilmstripId.MARTINI_GLASS        => "martini_empty_hold.bmp",
              FilmstripId.RED_WINE_BOTTLE      => "redwinebottle.bmp",
              FilmstripId.WHITE_WINE_BOTTLE    => "whitewinebottle.bmp",
              FilmstripId.PAINTING_1           => "painting1.bmp",
              FilmstripId.LITTLE_PAINTING      => "littlepainting.bmp",
              FilmstripId.WEDDING_PHOTO        => "weddingpic.bmp",
              FilmstripId.TUSCANY              => "tuscany.bmp",
              FilmstripId.GOWN_SKETCH_1        => "gownsketch1.bmp",
              FilmstripId.POST_ITS             => "post_its.bmp",
              FilmstripId.GOWN_SKETCH_4        => "gownsketch4.bmp",
              FilmstripId.PHONE                => "phoneontable.bmp",
              FilmstripId.ANSWERING_MACHINE    => "answeringmachine.bmp",
              FilmstripId.EIGHT_BALL           => "eightball.bmp",
              FilmstripId.BRASS_BULL           => "brassbull.bmp",
              FilmstripId.TRINKET_1            => "trinket1.bmp",
              FilmstripId.TRINKET_2            => "trinket2.bmp",
              FilmstripId.TRINKET_3            => "trinket3.bmp",
              FilmstripId.TRINKET_4            => "trinket4.bmp",
              FilmstripId.TRINKET_5            => "trinket5.bmp",
              FilmstripId.TRINKET_6            => "trinket6.bmp",
              FilmstripId.TRINKET_7            => "trinket7.bmp",
              FilmstripId.TRINKET_8            => "trinket8.bmp",
              FilmstripId.RUG                  => "rug.bmp",
              FilmstripId.FRONT_DOOR           => "frontdoor.bmp",
              FilmstripId.BEDROOM_DOOR         => "bedroomdoor.bmp",
              FilmstripId.ELEVATOR             => "elevator.bmp",
              FilmstripId.ELEVATOR_BUTTON      => "elevatorbutton.bmp",
              (FilmstripId) 0x2b               => "blackstripes.bmp",
              FilmstripId.WHITE_CABINET_DOOR   => "whitecabinetdoor.bmp",
              FilmstripId.WHITE_CABINET_DOOR_2 => "whitecabinetdoor2.bmp",
              FilmstripId.NYC                  => "nyc.bmp",
              _                                => null,
          };

          // From function at 0x100910f0
          Color? color = colorId switch {
              (ColorId) 0x0 => Color.FromArgb((byte) (255 * .96f),
                                              (byte) (255 * .87f),
                                              (byte) (255 * .81f)),
              (ColorId) 0x1 => Color.FromArgb((byte) (255 * .628f),
                                              (byte) (255 * .734f),
                                              (byte) (255 * .921f)),
              (ColorId) 0x2 => Color.FromArgb((byte) (255 * .52f),
                                              (byte) (255 * .515f),
                                              (byte) (255 * .52f)),
              (ColorId) 0x3 => Color.FromArgb((byte) (255 * .2f),
                                              (byte) (255 * .2f),
                                              (byte) (255 * .3f)),
              (ColorId) 0x4 => Color.FromArgb((byte) (255 * .42f),
                                              (byte) (255 * .415f),
                                              (byte) (255 * .42f)),
              (ColorId) 0x5 => Color.FromArgb((byte) (255 * .428f),
                                              (byte) (255 * .534f),
                                              (byte) (255 * .721f)),
              (ColorId) 0x6 => Color.White,
              (ColorId) 0x7 => Color.FromArgb((byte) (255 * .25f),
                                              (byte) (255 * .15f),
                                              (byte) (255 * .1f)),
              (ColorId) 0x8 => Color.FromArgb((byte) (255 * 1f),
                                              (byte) (255 * .75f),
                                              (byte) (255 * .7f)),
              (ColorId) 0x9 => Color.FromArgb((byte) (255 * 1f),
                                              (byte) (255 * .8f),
                                              (byte) (255 * 0f)),
              (ColorId) 0xA => Color.FromArgb((byte) (255 * 1f),
                                              (byte) (255 * .6f),
                                              (byte) (255 * 0f)),
              (ColorId) 0xB => Color.FromArgb((byte) (255 * 1f),
                                              (byte) (255 * 1f),
                                              (byte) (255 * .95f)),
              (ColorId) 0xC => Color.FromArgb((byte) (255 * .83f),
                                              (byte) (255 * .83f),
                                              (byte) (255 * .83f)),
              (ColorId) 0xD => Color.FromArgb((byte) (255 * .75f),
                                              (byte) (255 * 1f),
                                              (byte) (255 * .75f)),
              (ColorId) 0xE => Color.FromArgb((byte) (255 * .9f),
                                              (byte) (255 * 1f),
                                              (byte) (255 * 1f)),
              (ColorId) 0xF => Color.FromArgb((byte) (255 * 1f),
                                              (byte) (255 * 1f),
                                              (byte) (255 * .9f)),
              (ColorId) 0x10 => Color.FromArgb((byte) (255 * .9f),
                                               (byte) (255 * .9f),
                                               (byte) (255 * .9f)),
              (ColorId) 0x11 => Color.FromArgb((byte) (255 * 1f),
                                               (byte) (255 * 1f),
                                               (byte) (255 * .2f)),
              (ColorId) 0x12 => Color.FromArgb((byte) (255 * .6f),
                                               (byte) (255 * .6f),
                                               (byte) (255 * .6f)),
              (ColorId) 0x13 => Color.FromArgb((byte) (255 * .5f),
                                               (byte) (255 * .5f),
                                               (byte) (255 * .5f)),
              (ColorId) 0x14 => Color.FromArgb((byte) (255 * .6f),
                                               (byte) (255 * .7f),
                                               (byte) (255 * 1f)),
              (ColorId) 0x15 => Color.FromArgb((byte) (255 * .62f),
                                               (byte) (255 * .53f),
                                               (byte) (255 * .57f)),
              (ColorId) 0x16 => Color.FromArgb((byte) (255 * .42f),
                                               (byte) (255 * .33f),
                                               (byte) (255 * .37f)),
              (ColorId) 0x17 => Color.FromArgb((byte) (255 * .273f),
                                               (byte) (255 * .222f),
                                               (byte) (255 * .191f)),
              (ColorId) 0x18 => Color.FromArgb((byte) (255 * .83f),
                                               (byte) (255 * .77f),
                                               (byte) (255 * .7f)),
              (ColorId) 0x19 => Color.FromArgb((byte) (255 * .4f),
                                               (byte) (255 * .45f),
                                               (byte) (255 * .4f)),
              (ColorId) 0x1A => Color.FromArgb((byte) (255 * .35f),
                                               (byte) (255 * .4f),
                                               (byte) (255 * .35f)),
              (ColorId) 0x1B => Color.FromArgb((byte) (255 * .6f),
                                               (byte) (255 * .5f),
                                               (byte) (255 * .3f)),
              (ColorId) 0x1C => Color.FromArgb((byte) (255 * .5f),
                                               (byte) (255 * .4f),
                                               (byte) (255 * .25f)),
              (ColorId) 0x1D => Color.FromArgb((byte) (255 * .95f),
                                               (byte) (255 * .8f),
                                               (byte) (255 * .75f)),
              (ColorId) 0x1E => Color.FromArgb((byte) (255 * .5f),
                                               (byte) (255 * .9f),
                                               (byte) (255 * .55f)),
              (ColorId) 0x1F => Color.FromArgb((byte) (255 * .77f),
                                               (byte) (255 * .73f),
                                               (byte) (255 * .7f)),
              ColorId.WINDOW => Color.FromArgb((byte) (255 * .2f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .4f)),
              (ColorId) 0x21 => Color.FromArgb((byte) (255 * 1f),
                                               (byte) (255 * 1f),
                                               (byte) (255 * .8f)),
              (ColorId) 0x22 => Color.FromArgb((byte) (255 * .95f),
                                               (byte) (255 * .85f),
                                               (byte) (255 * .78f)),
              (ColorId) 0x23 => Color.FromArgb((byte) (255 * .3f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .2f),
                                               (byte) (255 * .2f)),
              (ColorId) 0x24 => Color.FromArgb((byte) (255 * .9f),
                                               (byte) (255 * .9f),
                                               (byte) (255 * .95f)),
              (ColorId) 0x26 => Color.FromArgb((byte) (255 * 1f),
                                               (byte) (255 * 0f),
                                               (byte) (255 * 0f)),
              (ColorId) 0x27 => Color.FromArgb((byte) (255 * 0f),
                                               (byte) (255 * 0f),
                                               (byte) (255 * 1f)),
              (ColorId) 0x28 => Color.FromArgb((byte) (255 * 0f),
                                               (byte) (255 * 1f),
                                               (byte) (255 * 0f)),
              (ColorId) 0x29 => Color.FromArgb((byte) (255 * 1f),
                                               (byte) (255 * 0f),
                                               (byte) (255 * 1f)),
              (ColorId) 0x2A => Color.FromArgb((byte) (255 * .6f),
                                               (byte) (255 * .8f),
                                               (byte) (255 * 1f)),
              (ColorId) 0x2B => Color.FromArgb((byte) (255 * .33f),
                                               (byte) (255 * .16f),
                                               (byte) (255 * .02f)),
              (ColorId) 0x2C => Color.FromArgb((byte) (255 * .43f),
                                               (byte) (255 * .26f),
                                               (byte) (255 * .12f)),
              (ColorId) 0x2D => Color.FromArgb((byte) (255 * .53f),
                                               (byte) (255 * .36f),
                                               (byte) (255 * .22f)),
              ColorId.COLOR_0x2E => Color.FromArgb(
                  (byte) (255 * .63f),
                  (byte) (255 * .46f),
                  (byte) (255 * .32f)),
              ColorId.COLOR_0x2F => Color.FromArgb((byte) (255 * .23f),
                                                   (byte) (255 * .16f),
                                                   (byte) (255 * .02f)),
              ColorId.COLOR_0x30 => Color.FromArgb((byte) (255 * .33f),
                                                   (byte) (255 * .26f),
                                                   (byte) (255 * .12f)),
              ColorId.COLOR_0x31 => Color.FromArgb((byte) (255 * .43f),
                                                   (byte) (255 * .36f),
                                                   (byte) (255 * .22f)),
              ColorId.COLOR_0x32 => Color.FromArgb((byte) (255 * .48f),
                                                   (byte) (255 * .41f),
                                                   (byte) (255 * .27f)),

              // Dark grays
              ColorId.COLOR_0x33 => Color.FromArgb((byte) (255 * .05f),
                                                   (byte) (255 * .05f),
                                                   (byte) (255 * .05f)),
              ColorId.LIVING_ROOM_FRONT_WALL => Color.FromArgb((byte) (255 * .1f),
                                                   (byte) (255 * .1f),
                                                   (byte) (255 * .1f)),
              ColorId.FOYER_WALL => Color.FromArgb((byte) (255 * .15f),
                                                   (byte) (255 * .15f),
                                                   (byte) (255 * .15f)),
              ColorId.COLOR_0x36 => Color.FromArgb((byte) (255 * .2f),
                                                   (byte) (255 * .2f),
                                                   (byte) (255 * .2f)),
              ColorId.COLOR_0x37 => Color.FromArgb((byte) (255 * .25f),
                                                   (byte) (255 * .25f),
                                                   (byte) (255 * .25f)),
              ColorId.CEILING => Color.FromArgb((byte) (255 * .3f),
                                                   (byte) (255 * .3f),
                                                   (byte) (255 * .3f)),
              ColorId.COLOR_0x39 => Color.FromArgb((byte) (255 * .35f),
                                                   (byte) (255 * .35f),
                                                   (byte) (255 * .35f)),
              ColorId.COLOR_0x3A => Color.FromArgb((byte) (255 * .4f),
                                                   (byte) (255 * .4f),
                                                   (byte) (255 * .4f)),
              ColorId.FLOOR => Color.FromArgb((byte) (255 * .45f),
                                              (byte) (255 * .45f),
                                              (byte) (255 * .45f)),
              ColorId.LIVING_ROOM_WALL => Color.Black,
              _            => null,
          };

          // This is a guess
          if (isLine && color != null) {
            var colorValue = color.Value;
            color = Color.FromArgb(255 - colorValue.R,
                                   255 - colorValue.G,
                                   255 - colorValue.B);
          }

          if (textureFileName == null) {
            var colorMaterial
                = this.Model.MaterialManager.AddColorMaterial(
                    color ?? Color.Magenta);
            colorMaterial.CullingMode = CullingMode.SHOW_BOTH;
            return colorMaterial;
          } else {
            if (!fileBundle.PrereqsDirectory.TryToGetExistingFile(
                    textureFileName,
                    out var textureFile)) {
              ;
            }

            /*var textureFile
                = fileBundle.PrereqsDirectory.AssertGetExistingFile(
                    textureFileName);*/
            files.Add(textureFile);

            var image = FinImage.FromFile(textureFile);
            if (removeBackground) {
              image = image.RemoveBackgroundColor(Color.White);
            }

            var (textureMaterial, _)
                = this.Model.MaterialManager.AddSimpleTextureMaterialFromImage(
                    image,
                    textureFile.NameWithoutExtension.ToString());
            textureMaterial.CullingMode = CullingMode.SHOW_BOTH;

            if (color != null) {
              textureMaterial.DiffuseColor = color;
            }

            return textureMaterial;
          }
        });

    this.AddScenery_();
    this.AddRoom_();
    this.AddSprites_();
  }

  private void AddScenery_() {
    this.AddSceneryObject_(
        "couch",
        new Vector3(150, 0, -350),
        0x1ac5fc,
        0x1aeeb4,
        []);
    this.AddSceneryObject_(
        "cabinet",
        new Vector3(-177, 0, -350),
        0x1b4c24,
        0x1b70dc,
        [
            (0, 1, 2, 3),
            (0xe, 0xf, 0x10, 0x11),
            (0x12, 0x13, 0x14, 0x15),
            (6, 7, 8, 9),
            (0x1e, 0x1f, 0x20, 0x21),
            (0x22, 0x23, 0x24, 0x25),
            (0x26, 0x27, 0x28, 0x29),
            (0x2a, 0x2b, 0x2c, 0x2d),
        ]);
    this.AddSceneryObject_(
        "side table",
        new Vector3(170, 0, -210),
        0x1af6b4,
        0x1b1b6c,
        [
            (0x21, 0x24, 0x26, 0x22),
        ]);
    this.AddSceneryObject_(
        "work table",
        new Vector3(160, 0, -30),
        0x1b1f6c,
        0x1b4424,
        []);
    this.AddSceneryObject_(
        "bar",
        new Vector3(-160, 0, -120),
        0x1bcfdc,
        0x1bf494,
        [
            (4, 10, 9, 6),
            (2, 4, 10, 0xb),
            (1, 2, 0xb, 0xc),
        ]);
    this.AddSceneryObject_(
        "side table 2",
        new Vector3(-180, 0, -60),
        0x1af6b4,
        0x1b1b6c,
        []);
  }

  private void AddSceneryObject_(
      string name,
      Vector3 objectPosition,
      uint vertexOffset,
      uint faceOffset,
      IEnumerable<(int, int, int, int)> collisionQuadIndices) {
    var animEngineDll = this.fileBundle_.PrereqsDirectory.AssertGetExistingFile(
        "animEngineDLL.dll");
    using var br = animEngineDll.OpenReadAsBinary();

    var bone = this.Model.Skeleton.Root.AddChild(objectPosition);
    bone.Name = name;

    var skin = this.Model.Skin;
    var boneWeights
        = skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);

    br.Position = vertexOffset;

    var positions = new List<Vector3>();
    while (!br.ReadSingle().IsRoughly(666)) {
      br.Position -= sizeof(float);
      positions.Add(br.ReadVector3());
    }

    var vertices
        = new LazyDictionary<(int index, int corner), IReadOnlyVertex>(tuple
            => {
            var (index, corner) = tuple;

            var vertex = skin.AddVertex(positions[index]);
            vertex.SetBoneWeights(boneWeights);
            vertex.SetUv(corner switch {
                0 => new Vector2(0, 0),
                1 => new Vector2(1, 0),
                2 => new Vector2(1, 1),
                3 => new Vector2(0, 1),
            });

            return vertex;
          });

    br.Position = faceOffset;
    var allFaceInts = new List<int>();
    while (br.ReadInt32() != 666) {
      br.Position -= sizeof(int);
      allFaceInts.AddRange(br.ReadInt32s(60));
    }

    // (The collision is just the subset of vertices from collisionQuadIndices.)

    var mesh = skin.AddMesh();
    mesh.Name = name;

    allFaceInts.ForEachNlet(
        15,
        faceInts => {
          if (faceInts[0] == 666) {
            return false;
          }

          var v0 = vertices[(faceInts[0], 0)];
          var v1 = vertices[(faceInts[1], 1)];
          var v2 = vertices[(faceInts[2], 2)];
          var v3 = vertices[(faceInts[3], 3)];

          var faceColorId = (ColorId) (faceInts[5] - 1);
          var faceFilmstripId = (FilmstripId) faceInts[14];
          var faceMaterial
              = this.lazyMaterials_[
                  (faceFilmstripId, faceColorId, false, false)];

          var hasLine0 = faceInts[6] == 0;
          var hasLine1 = faceInts[7] == 0;
          var hasLine2 = faceInts[8] == 0;
          var hasLine3 = faceInts[9] == 0;

          var lineColorId0 = (ColorId) (faceInts[10] - 1);
          var lineColorId1 = (ColorId) (faceInts[11] - 1);
          var lineColorId2 = (ColorId) (faceInts[12] - 1);
          var lineColorId3 = (ColorId) (faceInts[13] - 1);

          var quad = mesh.AddQuads((v0, v1, v2, v3));
          quad.SetMaterial(faceMaterial);
          quad.SetInversePriority(1);

          uint linePriority = 0;

          if (hasLine0) {
            var line0 = mesh.AddLines((v0, v1));
            line0.SetMaterial(
                this.lazyMaterials_[(null, lineColorId0, false, true)]);
            line0.SetInversePriority(linePriority);
          }

          if (hasLine1) {
            var line1 = mesh.AddLines((v1, v2));
            line1.SetMaterial(
                this.lazyMaterials_[(null, lineColorId1, false, true)]);
            line1.SetInversePriority(linePriority);
          }

          if (hasLine2) {
            var line2 = mesh.AddLines((v2, v3));
            line2.SetMaterial(
                this.lazyMaterials_[(null, lineColorId2, false, true)]);
            line2.SetInversePriority(linePriority);
          }

          if (hasLine3) {
            var line3 = mesh.AddLines((v3, v0));
            line3.SetMaterial(
                this.lazyMaterials_[(null, lineColorId3, false, true)]);
            line3.SetInversePriority(linePriority);
          }

          return true;
        });
  }

  private void AddRoom_() {
    this.AddPlane_(
        "LR_backwall1",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(70, 160),
        new Vector3(-165, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver1",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(6.5f, 160),
        new Vector3(-66, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwallsliver2",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(6.5f, 160),
        new Vector3(6, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall3",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(30, 160),
        new Vector3(85, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwall4",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(150, 80, -500),
        0);
    this.AddPlane_(
        "LR_backwalltoppanel",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(200, 10),
        new Vector3(-30, 155, -500),
        0);

    this.AddPlane_(
        "LR_leftwall0",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(70, 160),
        new Vector3(-200, 80, -5),
        90);
    this.AddPlane_(
        "LR_leftwall1",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -90),
        90);
    this.AddPlane_(
        "LR_leftwall2",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -190),
        90);
    this.AddPlane_(
        "LR_leftwall3",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -290),
        90);
    this.AddPlane_(
        "LR_leftwall4",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(60, 160),
        new Vector3(-200, 80, -370),
        90);
    this.AddPlane_(
        "LR_leftwall5",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(-200, 80, -450),
        90);

    this.AddPlane_(
        "LR_rightwall0",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, 50),
        90);
    this.AddPlane_(
        "LR_rightwall1",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -50),
        90);
    this.AddPlane_(
        "LR_rightwall2",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -150),
        90);
    this.AddPlane_(
        "LR_rightwall3",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -250),
        90);
    this.AddPlane_(
        "LR_rightwall4",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -350),
        90);
    this.AddPlane_(
        "LR_rightwall5",
        null,
        ColorId.LIVING_ROOM_WALL,
        new Vector2(100, 160),
        new Vector3(200, 80, -450),
        90);

    this.AddPlane_(
        "LR_backwindow1",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(-101, 77, -498),
        0);
    this.AddPlane_(
        "LR_backwindow2",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(-30, 77, -498),
        0);
    this.AddPlane_(
        "LR_backwindow3",
        null,
        ColorId.WINDOW,
        new Vector2(66, 150),
        new Vector3(41, 77, -498),
        0);

    this.AddPlane_(
        "LR_frontwallL",
        null,
        ColorId.LIVING_ROOM_FRONT_WALL,
        new Vector2(60, 160),
        new Vector3(170, 80, 98),
        0);
    this.AddPlane_(
        "LR_frontwallR1",
        null,
        ColorId.LIVING_ROOM_FRONT_WALL,
        new Vector2(66, 160),
        new Vector3(17, 80, 98),
        0);
    this.AddPlane_(
        "LR_frontwallR1_real",
        null,
        ColorId.LIVING_ROOM_FRONT_WALL,
        new Vector2(50, 160),
        new Vector3(2, 80, 108),
        0);
    this.AddPlane_(
        "LR_frontwallR2",
        null,
        ColorId.LIVING_ROOM_FRONT_WALL,
        new Vector2(80, 160),
        new Vector3(-46, 80, 98),
        0);

    this.AddPlane_(
        "FY_leftwall1",
        null,
        ColorId.FOYER_WALL,
        new Vector2(100, 160),
        new Vector3(-225, 80, 72),
        60);

    this.AddPlane_(
        "KI_fridgeside1",
        null,
        ColorId.FRIDGE,
        new Vector2(60, 120),
        new Vector3(-20, 60, 430),
        90);
    this.AddPlane_(
        "KI_fridgeside2",
        null,
        ColorId.FRIDGE,
        new Vector2(60, 120),
        new Vector3(40, 60, 430),
        90);
    this.AddPlane_(
        "KI_fridgefront1",
        null,
        ColorId.FRIDGE,
        new Vector2(60, 40),
        new Vector3(10, 100, 400),
        0);
    this.AddPlane_(
        "KI_fridgefront2",
        null,
        ColorId.FRIDGE,
        new Vector2(60, 72),
        new Vector3(10, 44, 400),
        0);
    this.AddPlane_(
        "KI_fridgefrontbottom",
        null,
        ColorId.FRIDGE_BOTTOM,
        new Vector2(60, 8),
        new Vector3(10, 4, 401),
        0);

    this.AddFloor_(
        "ceiling1",
        null,
        ColorId.CEILING,
        new Vector2(500, 600),
        new Vector3(-250, 160, 300));
    this.AddFloor_(
        "ceiling2",
        null,
        ColorId.CEILING,
        new Vector2(500, 600),
        new Vector3(250, 160, 300));
    this.AddFloor_(
        "ceiling3",
        null,
        ColorId.CEILING,
        new Vector2(500, 600),
        new Vector3(-250, 160, -300));
    this.AddFloor_(
        "ceiling4",
        null,
        ColorId.CEILING,
        new Vector2(500, 600),
        new Vector3(250, 160, -300));

    this.AddFloor_(
        "floor1",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(-250, 0, 300));
    this.AddFloor_(
        "floor2",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(250, 0, 300));
    this.AddFloor_(
        "floor3",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(-250, 0, -300));
    this.AddFloor_(
        "floor4",
        null,
        ColorId.FLOOR,
        new Vector2(500, 600),
        new Vector3(250, 0, -300));

    this.AddFloor_(
        "rug",
        FilmstripId.RUG,
        null,
        new Vector2(50, 100),
        new Vector3(85, 3, -350));
  }

  private void AddSprites_() {
    this.AddSprite_(
        "plant",
        FilmstripId.PLANT,
        new Vector2(120),
        new Vector3(160, 60, 65));
    this.AddSprite_(
        "ashtray",
        FilmstripId.ASHTRAY,
        new Vector2(10, 5),
        new Vector3(165, 60, 10));
    this.AddSprite_(
        "phone",
        FilmstripId.PHONE,
        new Vector2(16),
        new Vector3(145, 60, -60));
    this.AddSprite_(
        "answering machine",
        FilmstripId.ANSWERING_MACHINE,
        new Vector2(16),
        new Vector3(170, 60, -70));
    this.AddSprite_(
        "red wine bottle",
        FilmstripId.RED_WINE_BOTTLE,
        new Vector2(25),
        new Vector3(-185, 80, -108));
    this.AddSprite_(
        "white wine bottle",
        FilmstripId.WHITE_WINE_BOTTLE,
        new Vector2(25),
        new Vector3(-193, 80, -112));
    this.AddSprite_("martini glass 1",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-184, 41, -274));
    this.AddSprite_("martini glass 2",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-183, 41, -286));
    this.AddSprite_("martini glass 3",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-183.5f, 41, -297));
    this.AddSprite_("martini glass 4",
                    FilmstripId.MARTINI_GLASS,
                    new Vector2(12),
                    new Vector3(-183f, 41, -308));
    this.AddSprite_("bar_redwinebottle",
                    FilmstripId.RED_WINE_BOTTLE,
                    new Vector2(25),
                    new Vector3(-100));
    this.AddSprite_("bar_whitewinebottle",
                    FilmstripId.WHITE_WINE_BOTTLE,
                    new Vector2(25),
                    new Vector3(-100));
    /*this.AddSprite_("player's drink",
                    6,
                    new Vector2(12),
                    new Vector3(-170f, 73, -110));
    this.AddSprite_("Trip's drink",
                    6,
                    new Vector2(12),
                    new Vector3(-100));
    this.AddSprite_("Grace's drink",
                    5,
                    new Vector2(12),
                    new Vector3(-100));*/

    this.AddPlane_(
        "big abstract painting",
        FilmstripId.PAINTING_1,
        null,
        new Vector2(52, 70),
        new Vector3(190, 100, -380),
        90);
    this.AddPlane_(
        "little painting",
        FilmstripId.LITTLE_PAINTING,
        null,
        new Vector2(20, 32),
        new Vector3(190, 118, -335),
        90);
    this.AddPlane_(
        "maoPainting",
        FilmstripId.LITTLE_PAINTING,
        null,
        new Vector2(40, 50),
        new Vector3(215, 100, 400),
        90);
    this.AddPlane_(
        "wedding photo",
        FilmstripId.WEDDING_PHOTO,
        null,
        new Vector2(60, 70),
        new Vector3(190, 90, -145),
        90);
    this.AddPlane_(
        "italy picture",
        FilmstripId.TUSCANY,
        null,
        new Vector2(32),
        new Vector3(-199, 90, -60),
        270);
    this.AddPlane_(
        "wedding gown sketch",
        FilmstripId.GOWN_SKETCH_1,
        null,
        new Vector2(32),
        new Vector3(190, 90, 0),
        90);
    this.AddPlane_(
        "wedding gown post its",
        FilmstripId.POST_ITS,
        null,
        new Vector2(36),
        new Vector3(190, 50, -28),
        90);
    this.AddPlane_(
        "wedding gown sketch 3",
        FilmstripId.GOWN_SKETCH_4,
        null,
        new Vector2(30),
        new Vector3(190, 83, -35),
        90);
    this.AddPlane_(
        "wedding gown sketch 4",
        FilmstripId.GOWN_SKETCH_4,
        null,
        new Vector2(30),
        new Vector3(190, 83, -35),
        90);
    this.AddPlane_(
        "tuxedosketch1",
        FilmstripId.GOWN_SKETCH_1,
        null,
        new Vector2(32),
        new Vector3(190, 95, 25),
        90);
    this.AddPlane_(
        "tuxedosketch2",
        FilmstripId.GOWN_SKETCH_1,
        null,
        new Vector2(25),
        new Vector3(160, 58, -15),
        90);
    this.AddSprite_(
        "advice ball",
        FilmstripId.EIGHT_BALL,
        new Vector2(8),
        new Vector3(-149, 71, -170));
    this.AddSprite_(
        "brass bull",
        FilmstripId.BRASS_BULL,
        new Vector2(16),
        new Vector3(-185, 75, -300));
    this.AddSprite_(
        "trinket 1",
        FilmstripId.TRINKET_1,
        new Vector2(28),
        new Vector3(-185, 48, -450));
    this.AddSprite_(
        "trinket 2",
        FilmstripId.TRINKET_2,
        new Vector2(28),
        new Vector3(-186, 48, -425));
    this.AddSprite_(
        "trinket 3",
        FilmstripId.TRINKET_3,
        new Vector2(20),
        new Vector3(-184, 46, -400));
    this.AddSprite_(
        "trinket 4",
        FilmstripId.TRINKET_4,
        new Vector2(16),
        new Vector3(-185, 75, -443));
    this.AddSprite_(
        "trinket 5",
        FilmstripId.TRINKET_5,
        new Vector2(16),
        new Vector3(-186, 75, -405));
    this.AddSprite_(
        "trinket 6",
        FilmstripId.TRINKET_6,
        new Vector2(20),
        new Vector3(-186, 98, -399));
    this.AddSprite_(
        "trinket 7",
        FilmstripId.TRINKET_7,
        new Vector2(16),
        new Vector3(-186, 96, -424));
    this.AddSprite_(
        "trinket 8",
        FilmstripId.TRINKET_8,
        new Vector2(16),
        new Vector3(-186, 96, -450));
  }

  private void AddSprite_(
      string meshName,
      FilmstripId filmstripId,
      Vector2 widthAndHeight,
      Vector3 position) {
    var bone = this.Model.Skeleton.Root.AddChild(position);
    bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY,
                                 QuaternionUtil.CreateZyxRadians(
                                     -MathF.PI / 2,
                                     0,
                                     0));

    var material = this.lazyMaterials_[(filmstripId, null, true, false)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var ul = new Vector3(-width / 2, height / 2, 0);
    var ur = new Vector3(width / 2, height / 2, 0);
    var lr = new Vector3(width / 2, -height / 2, 0);
    var ll = new Vector3(-width / 2, -height / 2, 0);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(0, 0), null),
                       (ur, new Vector2(1, 0), null),
                       (lr, new Vector2(1, 1), null),
                       (ll, new Vector2(0, 1), null),
                       material,
                       bone);
  }

  private void AddPlane_(
      string meshName,
      FilmstripId? filmstripId,
      ColorId? colorId,
      in Vector2 widthAndHeight,
      Vector3 position,
      float degrees) {
    var material = this.lazyMaterials_[(filmstripId, colorId, false, false)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var xN = MathF.Cos(degrees / 180 * MathF.PI);
    var yN = -MathF.Sin(degrees / 180 * MathF.PI);

    var rotationX = width / 2 * xN;
    var rotationY = width / 2 * yN;

    var ul = position + new Vector3(-rotationX, height / 2, -rotationY);
    var ur = position + new Vector3(rotationX, height / 2, rotationY);
    var lr = position + new Vector3(rotationX, -height / 2, rotationY);
    var ll = position + new Vector3(-rotationX, -height / 2, -rotationY);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(1, 0), null),
                       (ur, new Vector2(0, 0), null),
                       (lr, new Vector2(0, 1), null),
                       (ll, new Vector2(1, 1), null),
                       material);
  }

  private void AddFloor_(
      string meshName,
      FilmstripId? filmstripId,
      ColorId? colorId,
      in Vector2 widthAndHeight,
      Vector3 position) {
    var material = this.lazyMaterials_[(filmstripId, colorId, false, false)];

    var (width, height) = (widthAndHeight.X, widthAndHeight.Y);

    var ul = position + new Vector3(-width / 2, 0, height / 2);
    var ur = position + new Vector3(width / 2, 0, height / 2);
    var lr = position + new Vector3(width / 2, 0, -height / 2);
    var ll = position + new Vector3(-width / 2, 0, -height / 2);

    var skin = this.Model.Skin;
    var mesh = skin.AddMesh();
    mesh.Name = meshName;
    mesh.AddSimpleQuad(skin,
                       (ul, new Vector2(1, 0), null),
                       (ur, new Vector2(0, 0), null),
                       (lr, new Vector2(0, 1), null),
                       (ll, new Vector2(1, 1), null),
                       material);
  }
}