using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using visceral.api;

namespace visceral;

public sealed class GeoModelGoldenTests
    : BModelGoldenTests<GeoModelFileBundle> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  public override GeoModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new() {
        GeoFiles = directory.FilesWithExtension(".geo").ToArray(),
        RcbFile = directory.GetExistingFiles()
                           .SingleOrDefault(f => f.Name.EndsWith(".rcb.WIN")),
        BnkFileIdsDictionary = new BnkFileIdsDictionary(directory),
        MtlbFileIdsDictionary = new MtlbFileIdsDictionary(directory),
        Tg4hFileIdDictionary = new Tg4hFileIdDictionary(directory),
    };

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}