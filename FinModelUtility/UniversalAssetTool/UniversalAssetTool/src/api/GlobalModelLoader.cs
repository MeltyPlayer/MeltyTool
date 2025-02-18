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

using mod.api;

using modl.api;

using nitro.api;

using sm64ds.api;

using ttyd.api;

using UoT.api;

using visceral.api;

using vrml.api;

using xmod.api;


namespace uni.api;

public class GlobalModelImporter : IModelImporter<IModelFileBundle> {
  public IModel Import(IModelFileBundle modelFileBundle)
    => modelFileBundle switch {
        IBattalionWarsModelFileBundle battalionWarsModelFileBundle
            => new BattalionWarsModelImporter().Import(
                battalionWarsModelFileBundle),
        BmdModelFileBundle bmdModelFileBundle
            => new BmdModelImporter().Import(bmdModelFileBundle),
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
        TtydModelFileBundle ttydModelFileBundle
            => new TtydModelImporter().Import(ttydModelFileBundle),
        VrmlModelFileBundle vrmlModelFileBundle
            => new VrmlModelImporter().Import(vrmlModelFileBundle),
        XcModelFileBundle xcModelFileBundle
            => new XcModelImporter().Import(xcModelFileBundle),
        XmodModelFileBundle xmodModelFileBundle
            => new XmodModelImporter().Import(xmodModelFileBundle),
        _ => throw new ArgumentOutOfRangeException(nameof(modelFileBundle))
    };
}