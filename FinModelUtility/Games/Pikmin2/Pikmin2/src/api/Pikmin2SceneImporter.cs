using fin.io;
using fin.scene;

using games.pikmin2.route;

using jsystem.api;

namespace games.pikmin2.api;

public class Pikmin2SceneImporter : ISceneImporter<Pikmin2SceneFileBundle> {
  public IScene Import(Pikmin2SceneFileBundle sceneFileBundle) {
      var levelBmd = sceneFileBundle.LevelBmd;
      var routeTxt = sceneFileBundle.RouteTxt;

      var scene = new SceneImpl {
          FileBundle = sceneFileBundle,
          Files = new HashSet<IReadOnlyGenericFile>([levelBmd, routeTxt]),
      };
      var sceneArea = scene.AddArea();

      var mapObj = sceneArea.AddObject();
      mapObj.AddSceneModel(
          new BmdModelImporter().Import(new BmdModelFileBundle {
              BmdFile = levelBmd
          }));

      var routeObj = sceneArea.AddObject();

      routeObj.AddSceneModel(new RouteModelImporter().Import(routeTxt));

      return scene;
    }
}