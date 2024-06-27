using System.Collections.Generic;
using System.Collections.ObjectModel;

using fin.language.equations.fixedFunction;

namespace fin.model.impl;

public partial class ModelImpl<TVertex> {
  public IMaterialManager MaterialManager { get; } =
    new MaterialManagerImpl();

  private partial class MaterialManagerImpl : IMaterialManager {
    private IList<IMaterial> materials_ = new List<IMaterial>();
    private IList<ITexture> textures_ = new List<ITexture>();

    public MaterialManagerImpl() {
      this.All = new ReadOnlyCollection<IMaterial>(this.materials_);
      this.Textures = new ReadOnlyCollection<ITexture>(this.textures_);
    }

    public IReadOnlyList<IMaterial> All { get; }
    public IFixedFunctionRegisters? Registers { get; private set; }
  }
}