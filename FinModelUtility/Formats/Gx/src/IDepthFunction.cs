namespace gx;

public interface IDepthFunction  {
  bool Enable { get; }
  GxCompareType Func { get; }
  bool WriteNewValueIntoDepthBuffer { get; }
}

public class DepthFunctionImpl : IDepthFunction {
  public bool Enable { get; set; }
  public GxCompareType Func { get; set; }
  public bool WriteNewValueIntoDepthBuffer { get; set; }
}