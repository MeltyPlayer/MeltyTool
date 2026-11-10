using readOnly;

namespace fin.math.transform;

[GenerateReadOnly]
public partial interface ITransform<TTranslation, TRotation, TScale> {
  new TTranslation LocalTranslation { get; set; }
  new TRotation LocalRotation { get; set; }
  new TScale LocalScale { get; set; }
}