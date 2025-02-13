using fin.importers;

using schema.readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface IModel : IResource {
  new ISkeleton Skeleton { get; }
  new ISkin Skin { get; }
  new IMaterialManager MaterialManager { get; }
  new IAnimationManager AnimationManager { get; }
}

[GenerateReadOnly]
public partial interface IModel<out TSkin> : IModel where TSkin : ISkin {
  new TSkin Skin { get; }
}