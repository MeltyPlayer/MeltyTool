using ac.api;

using bar.api;

using Celeste64.api;

using facade.api;

using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.io.importers.assimp;
using fin.model.io.importers.gltf;

using gdl.api;

using glo.api;

using gm.api;

using grezzo.api;

using hm64.api;

using hw.api;

using jsystem.api;

using level5.api;

using marioartist.api;

using mk3d.api;

using modl.api;

using nitro.api;

using pikmin1.api;

using pmdc.api;

using rollingMadness.api;

using sm64ds.api;

using sonicadventure.api;

using sysdolphin.api;

using tlpe.api;

using ttyd.api;

using UoT.api;

using visceral.api;

using vrml.api;

using xmod.api;


namespace uni.api;

public sealed class GlobalModelImporter : IModelImporter<IModelFileBundle> {
  public IModel Import(IModelFileBundle modelFileBundle)
    => modelFileBundle switch {
        AnimalCrossingModelFileBundle animalCrossingModelFileBundle
            => new AnimalCrossingModelImporter().Import(animalCrossingModelFileBundle),
        AseMeshModelFileBundle aseMeshModelFileBundle
            => new AseMeshModelImporter().Import(aseMeshModelFileBundle),
        AssimpModelFileBundle assimpModelFileBundle
            => new AssimpModelImporter().Import(assimpModelFileBundle),
        IBattalionWarsModelFileBundle battalionWarsModelFileBundle
            => new BattalionWarsModelImporter().Import(
                battalionWarsModelFileBundle),
        BmdModelFileBundle bmdModelFileBundle
            => new BmdModelImporter().Import(bmdModelFileBundle),
        Celeste64MapModelFileBundle celeste64MapModelFileBundle
            => new Celeste64MapModelImporter().Import(
                celeste64MapModelFileBundle),
        CmbModelFileBundle cmbModelFileBundle
            => new CmbModelImporter().Import(cmbModelFileBundle),
        DatModelFileBundle datModelFileBundle
            => new DatModelImporter().Import(datModelFileBundle),
        D3dModelFileBundle modModelFileBundle
            => new D3dModelImporter().Import(modModelFileBundle),
        FacadeRoomModelFileBundle facadeRoomModelFileBundle
            => new FacadeRoomModelImporter().Import(facadeRoomModelFileBundle),
        GauntletDarkLegacyModelFileBundle animModelFileBundle
            => new GauntletDarkLegacyModelImporter().Import(animModelFileBundle),
        GauntletDarkLegacyWorldModelFileBundle animModelFileBundle
            => new GauntletDarkLegacyWorldModelImporter().Import(animModelFileBundle),
        GeoModelFileBundle geoModelFileBundle
            => new GeoModelImporter().Import(geoModelFileBundle),
        GloModelFileBundle gloModelFileBundle
            => new GloModelImporter().Import(gloModelFileBundle),
        GltfModelFileBundle gltfModelFileBundle
            => new GltfModelImporter().Import(gltfModelFileBundle),
        Hm64MapModelFileBundle hm64MapModelFileBundle
            => new Hm64MapModelImporter().Import(hm64MapModelFileBundle),
        Ma3d1ModelFileBundle ma3d1ModelFileBundle
            => new Ma3d1ModelLoader().Import(ma3d1ModelFileBundle),
        Mk3dKartModelFileBundle mk3dKartModelFileBundle
            => new Mk3dKartModelImporter().Import(mk3dKartModelFileBundle),
        MeleeModelFileBundle meleeModelFileBundle
            => new MeleeModelImporter().Import(meleeModelFileBundle),
        ModModelFileBundle modModelFileBundle
            => new ModModelImporter().Import(modModelFileBundle),
        NsbmdModelFileBundle nsbmdModelFileBundle
            => new NsbmdModelImporter().Import(nsbmdModelFileBundle),
        OmdModelFileBundle omdModelFileBundle
            => new OmdModelImporter().Import(omdModelFileBundle),
        OotModelFileBundle ootModelFileBundle
            => new OotModelImporter().Import(ootModelFileBundle),
        PedModelFileBundle pedModelFileBundle
            => new PedModelImporter().Import(pedModelFileBundle),
        PmdcCharacterModelFileBundle pmdcCharacterModelFileBundle
            => new PmdcCharacterModelImporter().Import(
                pmdcCharacterModelFileBundle),
        ScbModelFileBundle scbModelFileBundle
            => new ScbModelImporter().Import(scbModelFileBundle),
        Sm64dsModelFileBundle sm64dsModelFileBundle
            => new Sm64dsModelImporter().Import(sm64dsModelFileBundle),
        SonicAdventureModelFileBundle sonicAdventureModelFileBundle
            => new SonicAdventureModelFileImporter().Import(
                sonicAdventureModelFileBundle),
        TstltModelFileBundle tstltModelFileBundle
            => new TstltModelImporter().Import(tstltModelFileBundle),
        TtydModelFileBundle ttydModelFileBundle
            => new TtydModelImporter().Import(ttydModelFileBundle),
        UvctModelFileBundle uvctModelFileBundle
            => new UvctModelFileImporter().Import(uvctModelFileBundle),
        UvmdModelFileBundle uvmdModelFileBundle
            => new UvmdModelFileImporter().Import(uvmdModelFileBundle),
        VbModelFileBundle vbModelFileBundle
            => new VbModelImporter().Import(vbModelFileBundle),
        VrmlModelFileBundle vrmlModelFileBundle
            => new VrmlModelImporter().Import(vrmlModelFileBundle),
        XcModelFileBundle xcModelFileBundle
            => new XcModelImporter().Import(xcModelFileBundle),
        XmodModelFileBundle xmodModelFileBundle
            => new XmodModelImporter().Import(xmodModelFileBundle),
        XtdModelFileBundle xtdModelFileBundle
            => new XtdModelImporter().Import(xtdModelFileBundle),
        ZsiModelFileBundle zsiModelFileBundle
            => new ZsiModelImporter().Import(zsiModelFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(modelFileBundle))
    };
}