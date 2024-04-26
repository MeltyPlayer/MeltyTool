using schema.readOnly;

namespace fin.model {
  [GenerateReadOnly]
  public partial interface IModel {
    ISkeleton Skeleton { get; }
    ISkin Skin { get; }
    IMaterialManager MaterialManager { get; }
    IAnimationManager AnimationManager { get; }
  }

  [GenerateReadOnly]
  public partial interface IModel<out TSkin> : IModel where TSkin : ISkin {
    new TSkin Skin { get; }
  }
}