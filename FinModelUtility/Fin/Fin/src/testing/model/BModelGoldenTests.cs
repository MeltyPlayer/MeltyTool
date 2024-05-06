using fin.io;
using fin.model.io;
using fin.model.io.importers;

namespace fin.testing.model {
  public abstract class BModelGoldenTests<TModelFileBundle, TModelImporter>
      : BGoldenTests<TModelFileBundle>
      where TModelFileBundle : IModelFileBundle
      where TModelImporter : IModelImporter<TModelFileBundle>, new() {
    public void AssertGolden(IFileHierarchyDirectory goldenDirectory)
      => ModelGoldenAssert.AssertGolden(goldenDirectory,
                                        new TModelImporter(),
                                        this.GetFileBundleFromDirectory);
  }
}