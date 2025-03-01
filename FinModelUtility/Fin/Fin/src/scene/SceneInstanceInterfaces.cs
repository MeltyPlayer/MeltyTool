using System;
using System.Collections.Generic;
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
  new IReadOnlyScene Definition { get; }

  new IReadOnlyList<ISceneAreaInstance> Areas { get; }

  new IReadOnlyLighting? Lighting { get; }

  new float ViewerScale { get; set; }
}

/// <summary>
///   A single area in a scene. This is used to split out the different
///   regions into separate pieces that can be loaded separately; for
///   example, in Ocarina of Time, this is used to represent a single room in
///   a dungeon.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneAreaInstance : ITickable, IDisposable {
  new IReadOnlySceneArea Definition { get; }

  new IReadOnlyList<ISceneObjectInstance> Objects { get; }

  new float ViewerScale { get; set; }

  new ISceneObjectInstance? CustomSkyboxObject { get; }
}

/// <summary>
///   An instance of an object in a scene. This can be used for anything that
///   appears in the scene, such as the level geometry, scenery, or
///   characters.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneObjectInstance : ITickable, IDisposable {
  new IReadOnlySceneObject Definition { get; }

  new Vector3 Position { get; }
  new IRotation Rotation { get; }
  new Vector3 Scale { get; }

  ISceneObjectInstance SetPosition(float x, float y, float z);

  ISceneObjectInstance SetPosition(Vector3 position)
    => this.SetPosition(position.X, position.Y, position.Z);

  ISceneObjectInstance SetRotationRadians(float xRadians,
                                          float yRadians,
                                          float zRadians);

  ISceneObjectInstance SetRotationDegrees(float xDegrees,
                                          float yDegrees,
                                          float zDegrees);

  new IReadOnlyList<ISceneModelInstance> Models { get; }

  new float ViewerScale { get; set; }
}

/// <summary>
///   An instance of a model rendered in a scene. This will automatically
///   take care of rendering animations, and also supports adding sub-models
///   onto bones.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneModelInstance : ITickable, IDisposable {
  new IReadOnlySceneModel Definition { get; }

  new IReadOnlyListDictionary<IReadOnlyBone, ISceneModelInstance> Children { get; }

  new IReadOnlyModel Model { get; }

  new IBoneTransformManager BoneTransformManager { get; }
  new ITextureTransformManager TextureTransformManager { get; }

  new IReadOnlyModelAnimation? Animation { get; set; }
  new IAnimationPlaybackManager AnimationPlaybackManager { get; }

  new float ViewerScale { get; set; }
}