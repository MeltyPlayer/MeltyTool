﻿using System;
using System.IO;


namespace HaloWarsTools {
  public class HWContext {
    public string GameInstallDirectory;
    public string ScratchDirectory;

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

    public HWContext(string gameInstallDirectory, string scratchDirectory) {
      GameInstallDirectory = gameInstallDirectory;
      ScratchDirectory = scratchDirectory;
    }

    public string GetAbsoluteGamePath(string relativePath) {
      return Path.Combine(GameInstallDirectory, relativePath);
    }

    public string GetRelativeGamePath(string absolutePath) {
      return Path.GetRelativePath(GameInstallDirectory, absolutePath);
    }

    public string GetAbsoluteScratchPath(string relativePath) {
      return Path.Combine(ScratchDirectory, relativePath);
    }

    public string GetRelativeScratchPath(string absolutePath) {
      return Path.GetRelativePath(ScratchDirectory, absolutePath);
    }

    public bool UnpackEra(string relativeEraPath) {
      if (IsEraUnpacked(relativeEraPath)) {
        return false;
      }

      Console.WriteLine($"Unpacking {relativeEraPath}");

      var absoluteEraPath = GetAbsoluteGamePath(relativeEraPath);
      var expander =
          new KSoft.Phoenix.Resource.EraFileExpander(absoluteEraPath);

      expander.Options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions.x64);
      expander.Options.Set(KSoft.Phoenix.Resource.EraFileUtilOptions
                                .SkipVerification);

      expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions
                                        .Decrypt);
      expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions
                                        .DontOverwriteExistingFiles);
      expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions
                                        .DecompressUIFiles);
      expander.ExpanderOptions.Set(KSoft.Phoenix.Resource.EraFileExpanderOptions
                                        .TranslateGfxFiles);

      if (!expander.Read()) {
        return false;
      }

      if (!expander.ExpandTo(ScratchDirectory,
                             Path.GetFileNameWithoutExtension(
                                 absoluteEraPath))) {
        return false;
      }

      return true;
    }

    public bool IsEraUnpacked(string relativeEraPath) {
      return File.Exists(Path.Combine(ScratchDirectory,
                                      Path.ChangeExtension(
                                          Path.GetFileName(relativeEraPath),
                                          ".eradef")));
    }

    public void ExpandAllEraFiles() {
      var files = Directory.GetFiles(GameInstallDirectory, "*.era");
      foreach (var eraFile in files) {
        UnpackEra(GetRelativeGamePath(eraFile));
      }
    }
  }
}