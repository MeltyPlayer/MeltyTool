using System.Numerics;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.model;
using fin.util.enumerables;

namespace marioartist.api;

public static class TstltUtil {
  public static IMesh? AddDisplayLists(
      IModel model,
      ISegmentChunk segmentChunk,
      IN64Hardware n64Hardware,
      DlModelBuilder dlModelBuilder,
      string meshName,
      bool isLeft,
      IEnumerable<(uint, Matrix4x4?, IBoneWeights)>
          displayListSegmentedOffsetAndBones) {
    n64Hardware.Memory.SetSegment(0xF, segmentChunk);

    var displayListReader = new DisplayListReader();
    var f3dzex2OpcodeParser = new F3dzex2OpcodeParser();
    var displayListTuples = displayListSegmentedOffsetAndBones
                            .Select((t, i) => {
                              try {
                                var displayList
                                    = displayListReader.ReadDisplayList(
                                        n64Hardware.Memory,
                                        f3dzex2OpcodeParser,
                                        t.Item1);
                                return (displayList, t.Item2, t.Item3) as
                                    (IDisplayList, Matrix4x4?, IBoneWeights)?;
                              } catch (Exception e) {
                                return null;
                              }
                            })
                            .WhereNonnull()
                            .Select(t => t!.Value)
                            .ToArray();

    if (!displayListTuples.Any()) {
      return null;
    }

    var mesh = dlModelBuilder.StartNewMesh(meshName);

    var rsp = n64Hardware.Rsp;
    foreach (var (displayList, matrix, boneWeights) in displayListTuples) {
      dlModelBuilder.Matrix = matrix ?? Matrix4x4.Identity;

      rsp.ActiveBoneWeights = boneWeights;

      try {
        dlModelBuilder.AddDl(displayList);
      } catch (Exception e) {
        ;
      }
    }

    dlModelBuilder.Matrix = Matrix4x4.Identity;

    foreach (var p in mesh.Primitives) {
      p.SetVertexOrder(isLeft
                           ? VertexOrder.CLOCKWISE
                           : VertexOrder.COUNTER_CLOCKWISE);
    }

    return mesh;
  }
}