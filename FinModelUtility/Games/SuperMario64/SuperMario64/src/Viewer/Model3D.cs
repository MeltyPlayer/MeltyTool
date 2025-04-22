using f3dzex2.image;
using f3dzex2.model;

using fin.math.matrix.four;

using sm64.memory;
using sm64.Scripts;

namespace sm64 {
  public class Model3DLods {
    private readonly IN64Hardware<ISm64Memory> sm64Hardware_;
    private List<DlModelBuilder> lods2_ = [];

    public Model3DLods(IN64Hardware<ISm64Memory> sm64Hardware) {
      this.sm64Hardware_ = sm64Hardware;
      this.AddLod(null);
    }

    public IReadOnlyList<DlModelBuilder> Lods2 => this.lods2_;

    public DlModelBuilder HighestLod2
      => this.Lods2.OrderBy(lod => lod.GetNumberOfTriangles())
             .Last();


    public DlModelBuilder Current2 => this.Lods2.LastOrDefault()!;

    public GeoScriptNode? Node { get; set; }


    public void AddLod(GeoScriptNode? node) {
      this.lods2_.Add(new(this.sm64Hardware_));
    }

    public void AddDl(uint address,
                      int currentDepth = 0) {
      var displayList = new F3dParser().Parse(this.sm64Hardware_.Memory, address);
      this.Current2.Matrix = this.Node?.matrix ?? FinMatrix4x4.IDENTITY;
      this.Current2.AddDl(displayList);
    }
  }
}