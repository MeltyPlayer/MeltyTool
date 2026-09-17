using System.Collections.Generic;
using System.Linq;

using fin.data.queues;
using fin.scene.components;
using fin.util.linq;


namespace fin.scene;

public static class SceneInstanceExtensions {
  public static IEnumerable<ISceneNodeInstance> EnumerateAllNodes(
      this ISceneInstance sceneInstance)
    => sceneInstance.Areas.SelectMany(a => a.RootNodes)
                    .SelectMany(EnumerateSelfAndDescendants);

  public static IEnumerable<ISceneNodeInstance> EnumerateSelfAndDescendants(
      this ISceneNodeInstance root) {
    var queue = new FinQueue<ISceneNodeInstance>(root);
    while (queue.TryDequeue(out var node)) {
      yield return node;
      queue.Enqueue(node.ChildNodes);
    }
  }

  public static IEnumerable<IAnimatableModel>
      EnumerateAllNonSkyboxAnimatableModels(this ISceneInstance scene)
    => scene.EnumerateAllNodes().SelectMany(EnumerateAllAnimatableModelsInSelf);

  public static IEnumerable<IAnimatableModel>
      EnumerateAllAnimatableModelsInSelf(this ISceneNodeInstance node)
    => node.Definition.Components
           .WhereIs<ISceneNodeComponent, IModelRenderComponent>();
}