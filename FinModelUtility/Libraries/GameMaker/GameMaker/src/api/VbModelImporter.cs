using fin.io;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using gm.schema.vb;


namespace gm.api;

public record VbModelFileBundle(IReadOnlyTreeFile VbFile) : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.VbFile;
}

public class VbModelImporter : IModelImporter<VbModelFileBundle> {
  public IModel Import(VbModelFileBundle modelFileBundle) {
    var vbFile = modelFileBundle.VbFile;
    var vb = vbFile.ReadNew<Vb>();

    var (finModel, finRootBone) =
        D3dModelImporter.CreateModel((modelFileBundle, vbFile.AsFileSet()));

    var finSkin = finModel.Skin;
    var weights =
        finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                       finRootBone);

    var finMesh = finSkin.AddMesh();
    finMesh.AddTriangles(
        vb.Vertices.Select(v => {
            var finVertex = finSkin.AddVertex(v.Position);
            finVertex.SetUv(v.Uv);
            finVertex.SetBoneWeights(weights);
            return finVertex;
          })
          .ToArray());

    return finModel;
  }
}