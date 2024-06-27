using fin.io;

using uni.platforms.wii.tools;

namespace uni.platforms.wii {
  public class WiiFileHierarchyExtractor {
    private readonly Wit wit_ = new();

    public IFileHierarchy ExtractFromRom(ISystemFile romFile) {
      this.wit_.Run(romFile, out var fileHierarchy);
      return fileHierarchy;
    }
  }
}