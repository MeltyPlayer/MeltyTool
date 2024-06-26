using ttyd.schema.model.blocks;

namespace ttyd.api {
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

  public class TtydGroupTransformBakedFrames
      : IGroupTransformBakedFrames {
    private readonly int transformCount_;
    private readonly float[] bakedTransformFrames_;

    public TtydGroupTransformBakedFrames(
        int transformCount,
        float[] bakedTransformFrames) {
      this.transformCount_ = transformCount;
      this.bakedTransformFrames_ = bakedTransformFrames;
    }

    public void GetTransformsAtFrame(
        Group group,
        int frame,
        int offsetInGroup,
        Span<float> buffer) {
      var allTransformsAtFrame
          = this.bakedTransformFrames_.AsSpan(this.transformCount_ * frame,
                                              this.transformCount_);
      allTransformsAtFrame
          .Slice(group.TransformBaseIndex + offsetInGroup, buffer.Length)
          .CopyTo(buffer);
    }
  }
}