using System;
using System.IO;

using KSoft.Phoenix.Resource;

namespace HaloWarsTools;

public class HWContext(string gameInstallDirectory, string scratchDirectory) {
  public string GameInstallDirectory = gameInstallDirectory;
  public string ScratchDirectory = scratchDirectory;

  /*public Dictionary<string, HWObjectDefinition> ObjectDefinitions => ValueCache.Get(LoadObjectDefinitions);

  private Dictionary<string, HWObjectDefinition> LoadObjectDefinitions() {
    var manifest = new Dictionary<string, HWObjectDefinition>();

    var source = "data\\objects.xml";

    var objects = XElement.Load(GetAbsoluteScratchPath(source)).Descendants("Object");
    foreach (var obj in objects) {
      if (obj.Attribute("name") != null) {
        var def = new HWObjectDefinition {
            Name = obj.Attribute("name").Value
        };
        var vis = obj.Descendants().FirstOrDefault(xmlElement => xmlElement.Name == "Visual");
        if (vis != null) {
          def.Visual = HWVisResource.FromFile(this, Path.Combine("art", vis.Value));
        }
        manifest.Add(def.Name, def);
      }
    }

    return manifest;
  }*/

  public string GetAbsoluteGamePath(string relativePath) {
    return Path.Combine(this.GameInstallDirectory, relativePath);
  }

  public string GetRelativeGamePath(string absolutePath) {
    return Path.GetRelativePath(this.GameInstallDirectory, absolutePath);
  }

  public string GetAbsoluteScratchPath(string relativePath) {
    return Path.Combine(this.ScratchDirectory, relativePath);
  }

  public string GetRelativeScratchPath(string absolutePath) {
    return Path.GetRelativePath(this.ScratchDirectory, absolutePath);
  }

  public bool UnpackEra(string relativeEraPath) {
    if (this.IsEraUnpacked(relativeEraPath)) {
      return false;
    }

    Console.WriteLine($"Unpacking {relativeEraPath}");

    var absoluteEraPath = this.GetAbsoluteGamePath(relativeEraPath);
    var expander =
        new EraFileExpander(absoluteEraPath);

    expander.Options.Set(EraFileUtilOptions.x64);
    expander.Options.Set(EraFileUtilOptions
                             .SkipVerification);

    expander.ExpanderOptions.Set(EraFileExpanderOptions
                                     .Decrypt);
    expander.ExpanderOptions.Set(EraFileExpanderOptions
                                     .DontOverwriteExistingFiles);
    expander.ExpanderOptions.Set(EraFileExpanderOptions
                                     .DecompressUIFiles);
    expander.ExpanderOptions.Set(EraFileExpanderOptions
                                     .TranslateGfxFiles);

    if (!expander.Read()) {
      return false;
    }

    if (!expander.ExpandTo(this.ScratchDirectory,
                           Path.GetFileNameWithoutExtension(
                               absoluteEraPath))) {
      return false;
    }

    return true;
  }

  public bool IsEraUnpacked(string relativeEraPath) {
    return File.Exists(Path.Combine(this.ScratchDirectory,
                                    Path.ChangeExtension(
                                        Path.GetFileName(relativeEraPath),
                                        ".eradef")));
  }

  public void ExpandAllEraFiles() {
    var files = Directory.GetFiles(this.GameInstallDirectory, "*.era");
    foreach (var eraFile in files) {
      this.UnpackEra(this.GetRelativeGamePath(eraFile));
    }
  }
}