using Celeste64.api;

using grezzo.api;

using sysdolphin.api;

using fin.model;
using fin.model.io;
using fin.model.io.importers;

using glo.api;

using gm.api;

using hw.api;

using jsystem.api;

using level5.api;

using pikmin1.api;

using modl.api;

using nitro.api;

using sm64ds.api;

using ttyd.api;

using UoT.api;

using visceral.api;

using vrml.api;

using xmod.api;

using fin.model.io.importers.assimp;
using fin.model.io.importers.gltf;

using rollingMadness.api;

using sonicadventure.api;


namespace uni.api;

public class GlobalModelImporter : IModelImporter<IModelFileBundle> {
  public IModel Import(IModelFileBundle modelFileBundle)
    => modelFileBundle switch {
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
        GeoModelFileBundle geoModelFileBundle
            => new GeoModelImporter().Import(geoModelFileBundle),
        GloModelFileBundle gloModelFileBundle
            => new GloModelImporter().Import(gloModelFileBundle),
        GltfModelFileBundle gltfModelFileBundle
            => new GltfModelImporter().Import(gltfModelFileBundle),
        XtdModelFileBundle xtdModelFileBundle
            => new XtdModelImporter().Import(xtdModelFileBundle),
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
        Sm64dsModelFileBundle sm64dsModelFileBundle
            => new Sm64dsModelImporter().Import(sm64dsModelFileBundle),
        SonicAdventureModelFileBundle sonicAdventureModelFileBundle
            => new SonicAdventureModelFileImporter().Import(
                sonicAdventureModelFileBundle),
        TtydModelFileBundle ttydModelFileBundle
            => new TtydModelImporter().Import(ttydModelFileBundle),
        VbModelFileBundle vbModelFileBundle
            => new VbModelImporter().Import(vbModelFileBundle),
        VrmlModelFileBundle vrmlModelFileBundle
            => new VrmlModelImporter().Import(vrmlModelFileBundle),
        XcModelFileBundle xcModelFileBundle
            => new XcModelImporter().Import(xcModelFileBundle),
        XmodModelFileBundle xmodModelFileBundle
            => new XmodModelImporter().Import(xmodModelFileBundle),
        ZsiModelFileBundle zsiModelFileBundle
            => new ZsiModelImporter().Import(zsiModelFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(modelFileBundle))
    };
}