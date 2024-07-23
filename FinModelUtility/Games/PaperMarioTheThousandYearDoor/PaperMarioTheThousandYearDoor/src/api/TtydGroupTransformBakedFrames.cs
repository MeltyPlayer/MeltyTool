using ttyd.schema.model.blocks;

namespace ttyd.api;

public interface IGroupTransformBakedFrames {
  void GetTransformsAtFrame(Group group,
                            int frame,
                            Span<float> buffer)
    => this.GetTransformsAtFrame(group, frame, 0, buffer);

  void GetTransformsAtFrame(Group group,
                            int frame,
                            int offsetInGroup,
                            Span<float> buffer);
}

public class TtydGroupTransformBakedFrames(
    int transformCount,
    float[] bakedTransformFrames)
    : IGroupTransformBakedFrames {
  public void GetTransformsAtFrame(
      Group group,
      int frame,
      int offsetInGroup,
      Span<float> buffer) {
      var allTransformsAtFrame
          = bakedTransformFrames.AsSpan(transformCount * frame,
                                              transformCount);
      allTransformsAtFrame
          .Slice(group.TransformBaseIndex + offsetInGroup, buffer.Length)
          .CopyTo(buffer);
    }
}