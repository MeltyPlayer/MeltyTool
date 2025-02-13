using schema.readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform<TTranslation, TRotation, TScale> {
  new TTranslation? Translation { get; set; }
  new TRotation? Rotation { get; set; }
  new TScale? Scale { get; set; }
}