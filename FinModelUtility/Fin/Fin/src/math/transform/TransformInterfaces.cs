using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform<TTranslation, TRotation, TScale> {
  TTranslation? Translation { get; set; }
  TRotation? Rotation { get; set; }
  TScale? Scale { get; set; }
}