using System.Text;

using fin.image;
using fin.io;
using fin.model.io;

namespace fin.testing.model;

public abstract class BModelGoldenTests<TModelFileBundle> 
    : BGoldenTests<TModelFileBundle> where TModelFileBundle : IModelFileBundle {
  public async Task AssertGolden(IFileHierarchyDirectory goldenDirectory) {
    FinImage.Initialize();
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    await ModelGoldenAssert.AssertGolden(
        goldenDirectory,
        this.GetFileBundleFromDirectory);
  }
}