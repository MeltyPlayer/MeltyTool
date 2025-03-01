using fin.color;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IFogParams {
  new float NearDistance { get; set; }
  new float FarDistance { get; set; }

  // TODO: Just use RGB
  new IColor Color { get; set; }
}