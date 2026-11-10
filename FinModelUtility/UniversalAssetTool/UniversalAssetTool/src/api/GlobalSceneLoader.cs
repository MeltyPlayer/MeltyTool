using bar.api;

using Celeste64.api;

using fin.scene;

using games.pikmin2.api;

using gdl.api;

using grezzo.api;

using hw.api;

using mk3d.api;

using mkdd.api;

using modl.api;

using pm.api;

using pmdc.api;

using sm64.api;

using vhr.api;

using vrml.api;


namespace uni.api;

public sealed class GlobalSceneImporter : ISceneImporter<ISceneFileBundle> {
  public IScene Import(ISceneFileBundle sceneFileBundle)
    => sceneFileBundle switch {
        BarSceneFileBundle barSceneFileBundle
            => new BarSceneFileImporter().Import(barSceneFileBundle),
        BolSceneFileBundle bolSceneFileBundle
            => new BolSceneImporter().Import(bolSceneFileBundle),
        BwSceneFileBundle bwSceneFileBundle
            => new BwSceneImporter().Import(bwSceneFileBundle),
        Celeste64MapSceneFileBundle celeste64MapSceneFileBundle
            => new Celeste64MapSceneImporter().Import(
                celeste64MapSceneFileBundle),
        GauntletDarkLegacySceneFileBundle gauntletDarkLegacySceneFileBundle
            => new GauntletDarkLegacySceneImporter().Import(
                gauntletDarkLegacySceneFileBundle),
        Mk3dTrackSceneFileBundle mk3dTrackSceneFileBundle
            => new Mk3dTrackSceneImporter().Import(mk3dTrackSceneFileBundle),
        LvlSceneFileBundle lvlSceneFileBundle
            => new LvlSceneImporter().Import(lvlSceneFileBundle),
        PaperMarioMapSceneFileBundle paperMarioMapSceneFileBundle
            => new PaperMarioMapSceneImporter().Import(paperMarioMapSceneFileBundle),
        Pikmin2SceneFileBundle pikmin2SceneFileBundle
            => new Pikmin2SceneImporter().Import(pikmin2SceneFileBundle),
        Sm64LevelSceneFileBundle sm64LevelSceneFileBundle
            => new Sm64LevelSceneImporter().Import(
                sm64LevelSceneFileBundle),
        UvctSceneFileBundle uvctSceneFileBundle
            => new UvctSceneFileImporter().Import(uvctSceneFileBundle),
        VictoryHeatRallyTrackSceneFileBundle vhrSceneFileBundle
            => new VictoryHeatRallyTrackSceneImporter().Import(
                vhrSceneFileBundle),
        VisSceneFileBundle visSceneFileBundle
            => new VisSceneImporter().Import(visSceneFileBundle),
        VrmlSceneFileBundle vrmlSceneFileBundle
            => new VrmlSceneImporter().Import(vrmlSceneFileBundle),
        ZsiSceneFileBundle zsiSceneFileBundle
            => new ZsiSceneImporter().Import(zsiSceneFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(sceneFileBundle))
    };
}