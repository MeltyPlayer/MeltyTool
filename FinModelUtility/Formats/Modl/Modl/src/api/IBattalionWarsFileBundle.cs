using fin.io.bundles;
using fin.model.io;

namespace modl.api;

public interface IBattalionWarsFileBundle : IFileBundle { }

public interface IBattalionWarsModelFileBundle : IBattalionWarsFileBundle,
                                                 IModelFileBundle { }