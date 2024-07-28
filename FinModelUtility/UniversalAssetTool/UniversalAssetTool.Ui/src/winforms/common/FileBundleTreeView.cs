using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

using fin.audio.io;
using fin.io.bundles;
using fin.model.io;
using fin.scene;

using uni.img;
using uni.ui.winforms.common.fileTreeView;


namespace uni.ui.winforms.common;

public class FileBundleTreeView : FileTreeView<IFileBundleDirectory> {
  protected override void PopulateImpl(IFileBundleDirectory directoryRoot,
                                       ParentFileNode uiRoot) {
      foreach (var subdir in directoryRoot.Subdirs) {
        this.AddDirectoryToNode_(subdir, uiRoot);
      }
    }

  private void AddDirectoryToNode_(IFileBundleDirectory directory,
                                   ParentFileNode parentNode,
                                   IList<string>? parts = null) {
      var subdirs = directory.Subdirs;
      var fileBundles = directory.FileBundles;

      var subdirCount = subdirs.Count;
      var fileBundlesCount = fileBundles.Count;

      if (subdirCount + fileBundlesCount == 1) {
        parts ??= new List<string>();
        parts.Add(directory.Name);

        if (subdirCount == 1) {
          this.AddDirectoryToNode_(subdirs[0], parentNode, parts);
        } else {
          this.AddFileToNode_(fileBundles[0], parentNode, parts);
        }
      } else {
        string text = directory.Name;
        if (parts != null) {
          parts.Add(text);
          text = Path.Join(parts.ToArray());
        }

        var uiNode = parentNode.AddChild(text);
        uiNode.Directory = directory.Directory;

        foreach (var subdirectory in directory.Subdirs) {
          this.AddDirectoryToNode_(subdirectory, uiNode);
        }

        foreach (var fileBundle in directory.FileBundles) {
          this.AddFileToNode_(fileBundle, uiNode);
        }

        if (DebugFlags.OPEN_DIRECTORIES_BY_DEFAULT) {
          uiNode.Expand();
        }
      }
    }

  private void AddFileToNode_(IAnnotatedFileBundle fileBundle,
                              ParentFileNode parentNode,
                              IList<string>? parts = null) {
      string? text = null;
      if (parts != null) {
        parts.Add(fileBundle.FileBundle.DisplayName);
        text = Path.Join(parts.ToArray());
      }

      parentNode.AddChild(fileBundle, text);
    }

  public override Image GetImageForFile(IFileBundle file)
    => file switch {
        IModelFileBundle => Icons.modelImage,
        IAudioFileBundle => Icons.musicImage,
        ISceneFileBundle => Icons.sceneImage,
    };
}