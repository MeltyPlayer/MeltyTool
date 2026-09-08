using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

using fin.color;
using fin.data.queues;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.util;
using fin.scene.components;
using fin.schema.vector;
using fin.ui;
using fin.ui.rendering;


namespace fin.scene;

public static class SceneExtensions {
  public static ISceneNode SetPosition(this ISceneNode sceneNode,
                                       in Vector3 position)
    => sceneNode.SetPosition(position.X, position.Y, position.Z);

  public static ISceneNode SetScale(this ISceneNode sceneNode,
                                    float scale)
    => sceneNode.SetScale(scale, scale, scale);

  public static ISceneNode SetScale(this ISceneNode sceneNode,
                                    in Vector3 scale)
    => sceneNode.SetScale(scale.X, scale.Y, scale.Z);

  public static ISceneNode SetRotationRadians(this ISceneNode sceneNode,
                                    in Vector3 rotationRadians)
    => sceneNode.SetRotationRadians(rotationRadians.X,
                                    rotationRadians.Y,
                                    rotationRadians.Z);

  public static ISceneNode SetMatrix(this ISceneNode sceneNode,
                                     Matrix4x4 matrix) {
    matrix.AssertDecompose(out var translation,
                           out var rotation,
                           out var scale);
    return sceneNode.SetPosition(translation)
                    .SetRotationRadians(rotation.ToEulerRadians())
                    .SetScale(scale);
  }

  public static void AddTickComponent(this ISceneNode sceneNode,
                                      Action<ISceneNodeInstance> handler)
    => sceneNode.AddComponent(new LambdaSceneNodeTickComponent(handler));

  private sealed class LambdaSceneNodeTickComponent(
      Action<ISceneNodeInstance> handler) : ISceneNodeTickComponent {
    public void Dispose() { }
    public void Tick(ISceneNodeInstance self) => handler(self);
  }

  public static void AddRenderable(this ISceneNode sceneNode,
                                   IRenderable renderable)
    => sceneNode.AddComponent(
        new RenderableSceneNodeRenderComponent(renderable));

  public static void AddRenderComponent(this ISceneNode sceneNode,
                                        Action<ISceneNodeInstance> handler)
    => sceneNode.AddComponent(new LambdaSceneNodeRenderComponent(handler));

  private sealed class RenderableSceneNodeRenderComponent(IRenderable impl)
      : ISceneNodeRenderComponent {
    public void Dispose() => (impl as IDisposable)?.Dispose();
    public void Render(ISceneNodeInstance self) => impl.Render();
  }

  public static IEnumerable<IReadOnlySceneNode> EnumerateAllNodes(
      this IReadOnlyScene scene) {
    var queue = new FinQueue<IReadOnlySceneNode>(
        scene.Areas.SelectMany(a => a.RootNodes));
    while (queue.TryDequeue(out var node)) {
      yield return node;
      queue.Enqueue(node.ChildNodes);
    }
  }

  public static IEnumerable<IReadOnlyModel> EnumerateAllModels(
      this IReadOnlyScene scene) {
    foreach (var node in scene.EnumerateAllNodes()) {
      foreach (var modelRenderComponent in node.Components
                                               .OfType<IModelRenderComponent>()) {
        yield return modelRenderComponent.Model;
      }
    }
  }

  public static IEnumerable<IReadOnlyModel> EnumerateAllDistinctModels(
      this IReadOnlyScene scene)
    => scene.EnumerateAllModels().Distinct();

  public static ILighting? CreateDefaultLighting(this IScene scene,
                                                 ISceneNode lightingOwner)
    => scene.CreateDefaultLighting(lightingOwner,
                                   scene.EnumerateAllModels().Distinct());

  public static ILighting? CreateDefaultLighting(
      this IScene scene,
      ISceneNode lightingOwner,
      IEnumerable<IReadOnlyModel> finModels) {
    var needsLights = false;
    var neededLightIndices = new HashSet<int>();

    foreach (var finModel in finModels) {
      var useLighting = UseLightingDetector.ShouldUseLightingFor(finModel);
      if (!useLighting) {
        continue;
      }

      foreach (var finMaterial in finModel.MaterialManager.All) {
        if (finMaterial.IgnoreLights) {
          continue;
        }

        needsLights = true;

        if (finMaterial is not IFixedFunctionMaterial
            finFixedFunctionMaterial) {
          continue;
        }

        var equations = finFixedFunctionMaterial.Equations;
        for (var i = 0; i < 8; ++i) {
          if (equations.DoOutputsDependOn([
                  FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 + i,
                  FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 + i,
                  FixedFunctionSource.LIGHT_SPECULAR_COLOR_0 + i,
                  FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0 + i
              ])) {
            neededLightIndices.Add(i);
          }
        }
      }
    }

    if (!needsLights) {
      return null;
    }

    bool attachFirstLightToCamera = false;
    float individualStrength = .8f / neededLightIndices.Count;
    if (neededLightIndices.Count == 0) {
      attachFirstLightToCamera = true;
      individualStrength = .4f;
      for (var i = 0; i < 3; ++i) {
        neededLightIndices.Add(i);
      }
    }

    var enabledCount = neededLightIndices.Count;
    var lightColors = enabledCount == 1
        ? [Color.White]
        : new[] {
            Color.White,
            Color.Pink,
            Color.LightBlue,
            Color.DarkSeaGreen,
            Color.PaleGoldenrod,
            Color.Lavender,
            Color.Bisque,
            Color.Blue,
            Color.Red
        };

    var maxLightIndex = neededLightIndices.Max();
    var currentIndex = 0;
    var lighting = scene.CreateLighting();
    for (var i = 0; i <= maxLightIndex; ++i) {
      var light = lighting.CreateLight();
      if (!(light.Enabled = neededLightIndices.Contains(i))) {
        continue;
      }

      light.SetColor(FinColor.FromSystemColor(lightColors[currentIndex]));
      light.Strength = individualStrength;


      var defaultAttenuation = new Vector3f { X = 1.075f };
      light.SetAttenuationFunction(AttenuationFunction.SPECULAR);
      light.SetCosineAttenuation(defaultAttenuation);
      light.SetDistanceAttenuation(defaultAttenuation);

      var angleInRadians = 2 *
                           MathF.PI *
                           (1f * currentIndex) /
                           (enabledCount + 1);

      var lightNormal = Vector3.Normalize(new Vector3 {
          X = (float) (.5f * Math.Cos(angleInRadians)),
          Y = (float) (.5f * Math.Sin(angleInRadians)),
          Z = -.6f,
      });
      light.SetNormal(new Vector3f {
          X = lightNormal.X,
          Y = lightNormal.Y,
          Z = lightNormal.Z
      });

      currentIndex++;
    }

    if (attachFirstLightToCamera) {
      var camera = Camera.Instance;
      var firstLight = lighting.Lights[0];

      lightingOwner.AddTickComponent(_ => {
        firstLight.SetPosition(camera.Position);
        firstLight.SetNormal(camera.Normal);
      });
    }

    return lighting;
  }
}