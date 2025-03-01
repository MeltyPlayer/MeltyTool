using fin.color;

namespace fin.model.impl.material;

public class FogParams : IFogParams {
  public required float NearDistance { get; set; }
  public required float FarDistance { get; set; }
  public required IColor Color { get; set; }
}