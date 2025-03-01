using Celeste64.api;

using fin.scene;

using games.pikmin2.api;

using grezzo.api;

using hw.api;

using mkdd.api;

using modl.api;

using pmdc.api;

using sm64.api;

using vrml.api;


namespace uni.api;

public class GlobalSceneImporter : ISceneImporter<ISceneFileBundle> {
  public IScene Import(ISceneFileBundle sceneFileBundle)
    => sceneFileBundle switch {
        BolSceneFileBundle bolSceneFileBundle
            => new BolSceneImporter().Import(bolSceneFileBundle),
        BwSceneFileBundle bwSceneFileBundle
            => new BwSceneImporter().Import(bwSceneFileBundle),
        Celeste64MapSceneFileBundle celeste64MapSceneFileBundle
            => new Celeste64MapSceneImporter().Import(
                celeste64MapSceneFileBundle),
        LvlSceneFileBundle lvlSceneFileBundle
            => new LvlSceneImporter().Import(lvlSceneFileBundle),
        Pikmin2SceneFileBundle pikmin2SceneFileBundle
            => new Pikmin2SceneImporter().Import(pikmin2SceneFileBundle),
        Sm64LevelSceneFileBundle sm64LevelSceneFileBundle
            => new Sm64LevelSceneImporter().Import(
                sm64LevelSceneFileBundle),
        VisSceneFileBundle visSceneFileBundle
            => new VisSceneImporter().Import(visSceneFileBundle),
        VrmlSceneFileBundle vrmlSceneFileBundle
            => new VrmlSceneImporter().Import(vrmlSceneFileBundle),
        ZsiSceneFileBundle zsiSceneFileBundle
            => new ZsiSceneImporter().Import(zsiSceneFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(sceneFileBundle))
    };
}