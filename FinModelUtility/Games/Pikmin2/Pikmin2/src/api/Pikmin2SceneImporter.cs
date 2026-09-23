using fin.io;
using fin.scene;

using games.pikmin2.route;

using jsystem.api;

namespace games.pikmin2.api;

public sealed class Pikmin2SceneImporter : ISceneImporter<Pikmin2SceneFileBundle> {
  public IScene Import(Pikmin2SceneFileBundle sceneFileBundle) {
      var levelBmd = sceneFileBundle.LevelBmd;
      var routeTxt = sceneFileBundle.RouteTxt;

      var scene = new SceneImpl {
          FileBundle = sceneFileBundle,
          Files = new HashSet<IReadOnlyStandaloneFile>([levelBmd, routeTxt]),
      };
      var sceneArea = scene.AddArea();

      var mapObj = sceneArea.AddRootNode();
      mapObj.AddSceneModel(
          new BmdModelImporter().Import(new BmdModelFileBundle {
              BmdFile = levelBmd
          }));

      var routeObj = sceneArea.AddRootNode();

      routeObj.AddSceneModel(new RouteModelImporter().Import(routeTxt));

      return scene;
    }
}