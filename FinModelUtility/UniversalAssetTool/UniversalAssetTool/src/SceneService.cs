using System.Drawing;
using System.Numerics;

using fin.color;
using fin.data.queues;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.scene;
using fin.schema.vector;
using fin.ui;
using fin.ui.rendering.gl.model;

using uni.api;
using uni.ui.winforms.common.fileTreeView;

namespace uni;

public static class SceneService {
  static SceneService() {
      FileBundleService.OnFileBundleOpened
          += (fileTreeLeafNode, fileBundle) => {
               if (fileBundle is ISceneFileBundle sceneFileBundle) {
                 var scene = new GlobalSceneImporter().Import(sceneFileBundle);
                 OpenScene(fileTreeLeafNode, scene);
               }
             };

      ModelService.OnModelOpened
          += (fileTreeLeafNode, model) => {
               var scene = new SceneImpl {
                   FileBundle = model.FileBundle,
                   Files = model.Files
               };
               var area = scene.AddArea();
               var obj = area.AddObject();
               obj.AddSceneModel(model);

               InjectDefaultLightingForScene_(scene, obj);

               OpenScene(fileTreeLeafNode, scene);
             };
    }

  public static event Action<IFileTreeLeafNode?, IScene> OnSceneOpened;

  public static void OpenScene(IFileTreeLeafNode? fileTreeLeafNode,
                               IScene scene)
    => OnSceneOpened?.Invoke(fileTreeLeafNode, scene);

  private static void InjectDefaultLightingForScene_(
      IScene scene,
      ISceneObject lightingOwner) {
      if (scene.Lighting != null) {
        return;
      }

      var needsLights = false;
      var neededLightIndices = new HashSet<int>();

      var sceneModelQueue = new FinQueue<IReadOnlySceneModel>(
          scene.Areas.SelectMany(
              area => area.Objects.SelectMany(obj => obj.Models)));
      while (sceneModelQueue.TryDequeue(out var sceneModel)) {
        sceneModelQueue.Enqueue(sceneModel.Children.Values);

        var finModel = sceneModel.Model;

        var useLighting =
            new UseLightingDetector().ShouldUseLightingFor(finModel);
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
        return;
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

        var position = new Vector3f();
        var normal = new Vector3f();

        lightingOwner.SetOnTickHandler(_ => {
                                         position.X = camera.X;
                                         position.Y = camera.Y;
                                         position.Z = camera.Z;
                                         firstLight.SetPosition(position);

                                         normal.X = camera.XNormal;
                                         normal.Y = camera.YNormal;
                                         normal.Z = camera.ZNormal;
                                         firstLight.SetNormal(normal);
                                       });
      }
    }
}