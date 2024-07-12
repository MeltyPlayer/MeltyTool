using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using fin.animation;
using fin.data.dictionaries;
using fin.math;
using fin.model;

using schema.readOnly;

namespace fin.scene;

public interface ITickable {
  void Tick();
}

[GenerateReadOnly]
public partial interface ISceneInstance : ITickable, IDisposable {
  IReadOnlyScene Definition { get; }

  IReadOnlyList<ISceneAreaInstance> Areas { get; }

  IReadOnlyLighting? Lighting { get; }

  float ViewerScale { get; set; }
}

/// <summary>
///   A single area in a scene. This is used to split out the different
///   regions into separate pieces that can be loaded separately; for
///   example, in Ocarina of Time, this is used to represent a single room in
///   a dungeon.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneAreaInstance : ITickable, IDisposable {
  IReadOnlySceneArea Definition { get; }

  IReadOnlyList<ISceneObjectInstance> Objects { get; }

  float ViewerScale { get; set; }

  Color? BackgroundColor { get; }
  ISceneObjectInstance? CustomSkyboxObject { get; }
}

/// <summary>
///   An instance of an object in a scene. This can be used for anything that
///   appears in the scene, such as the level geometry, scenery, or
///   characters.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneObjectInstance : ITickable, IDisposable {
  IReadOnlySceneObject Definition { get; }

  Vector3 Position { get; }
  IRotation Rotation { get; }
  Vector3 Scale { get; }

  ISceneObjectInstance SetPosition(float x, float y, float z);

  ISceneObjectInstance SetRotationRadians(float xRadians,
                                          float yRadians,
                                          float zRadians);

  ISceneObjectInstance SetRotationDegrees(float xDegrees,
                                          float yDegrees,
                                          float zDegrees);

  IReadOnlyList<ISceneModelInstance> Models { get; }

  float ViewerScale { get; set; }
}

/// <summary>
///   An instance of a model rendered in a scene. This will automatically
///   take care of rendering animations, and also supports adding sub-models
///   onto bones.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneModelInstance : ITickable, IDisposable {
  IReadOnlySceneModel Definition { get; }

  IReadOnlyListDictionary<IReadOnlyBone, ISceneModelInstance> Children { get; }

  IReadOnlyModel Model { get; }

  IBoneTransformManager BoneTransformManager { get; }
  ITextureTransformManager TextureTransformManager { get; }

  IReadOnlyModelAnimation? Animation { get; set; }
  IAnimationPlaybackManager AnimationPlaybackManager { get; }

  float ViewerScale { get; set; }
}