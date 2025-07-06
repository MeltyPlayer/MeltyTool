using fin.io;
using fin.math.transform;
using fin.model;
using fin.model.impl;
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

    var finModel = new ModelImpl {
        FileBundle = modelFileBundle,
        Files = vbFile.AsFileSet(),
    };

    var finRootBone = finModel.Skeleton.Root.AddChild(0, 0, 0);
    finRootBone.LocalTransform.SetRotationDegrees(90, 0, 0);

    var finSkin = finModel.Skin;
    var weights =
        finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                       finRootBone);

    var finMesh = finSkin.AddMesh();
    var triangles = finMesh.AddTriangles(
        vb.Vertices.Select(v => {
            var finVertex = finSkin.AddVertex(v.Position);
            finVertex.SetUv(v.Uv);
            finVertex.SetBoneWeights(weights);
            return finVertex;
          })
          .ToArray());
    triangles.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);

    return finModel;
  }
}