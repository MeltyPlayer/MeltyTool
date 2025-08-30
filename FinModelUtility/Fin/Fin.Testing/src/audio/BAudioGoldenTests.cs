using fin.audio.io;
using fin.audio.io.importers;
using fin.io;

namespace fin.testing.audio;

public abstract class BAudioGoldenTests<TAudioFileBundle, TAudioImporter>
    : BGoldenTests<TAudioFileBundle>
    where TAudioFileBundle : IAudioFileBundle
    where TAudioImporter : IAudioImporter<TAudioFileBundle>, new() {
  public void AssertGolden(IFileHierarchyDirectory goldenDirectory)
    => AudioGoldenAssert.AssertGolden(goldenDirectory,
                                      new TAudioImporter(),
                                      this.GetFileBundleFromDirectory);
}