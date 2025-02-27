using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using fin.data.dictionaries;
using fin.image;
using fin.importers;
using fin.model;

using schema.readOnly;

namespace fin.scene;

public interface ISceneFileBundle : I3dFileBundle;

public interface ISceneImporter<in TSceneFileBundle>
    : I3dImporter<IScene, TSceneFileBundle>
    where TSceneFileBundle : ISceneFileBundle;


// Scene
/// <summary>
///   A single scene from a game. These can be thought of as the parts of the
///   game that are each separated by a loading screen.
/// </summary>
[GenerateReadOnly]
public partial interface IScene : IResource {
  new IReadOnlyList<ISceneArea> Areas { get; }
  ISceneArea AddArea();

  new ILighting? Lighting { get; }
  ILighting CreateLighting();
}

/// <summary>
///   A single area in a scene. This is used to split out the different
///   regions into separate pieces that can be loaded separately; for
///   example, in Ocarina of Time, this is used to represent a single room in
///   a dungeon.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneArea {
  new IReadOnlyList<ISceneObject> Objects { get; }
  ISceneObject AddObject();

  new Color? BackgroundColor { get; set; }

  new IReadOnlyImage? BackgroundImage { get; set; }

  new ISceneObject? CustomSkyboxObject { get; set; }
  ISceneObject CreateCustomSkyboxObject();
}

/// <summary>
///   An instance of an object in a scene. This can be used for anything that
///   appears in the scene, such as the level geometry, scenery, or
///   characters.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneObject {
  new Vector3 Position { get; }
  new IRotation Rotation { get; }
  new Vector3 Scale { get; }

  ISceneObject SetPosition(float x, float y, float z);

  ISceneObject SetRotationRadians(float xRadians,
                                  float yRadians,
                                  float zRadians);

  ISceneObject SetRotationDegrees(float xDegrees,
                                  float yDegrees,
                                  float zDegrees);

  ISceneObject SetScale(float x, float y, float z);

  new IReadOnlyList<ISceneModel> Models { get; }
  ISceneModel AddSceneModel(IReadOnlyModel model);

  IReadOnlyList<ISceneObjectComponent> Components { get; }
  ISceneObject AddComponent(ISceneObjectComponent component);
}

public interface ISceneObjectComponent {
  void Tick(ISceneObjectInstance self);
}

/// <summary>
///   An instance of a model rendered in a scene. This will automatically
///   take care of rendering animations, and also supports adding sub-models
///   onto bones.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneModel {
  new IReadOnlyListDictionary<IReadOnlyBone, IReadOnlySceneModel> Children { get; }
  ISceneModel AddModelOntoBone(IReadOnlyModel model, IReadOnlyBone bone);

  new IReadOnlyModel Model { get; }
}