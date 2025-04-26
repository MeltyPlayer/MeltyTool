using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using fin.importers;
using fin.model.io.exporters.assimp;
using fin.io;
using fin.math.floats;
using fin.model.io;
using fin.scene;
using fin.util.enumerables;
using fin.util.io;
using fin.util.progress;
using fin.util.time;

using uni.cli;
using uni.config;
using uni.games;
using uni.ui.winforms.common.fileTreeView;
using uni.api;
using uni.util.bundles;

namespace uni.ui.winforms;

public partial class UniversalAssetToolForm : Form {
  private TimedCallback fpsCallback_;

  public UniversalAssetToolForm() {
    this.InitializeComponent();

    this.modelTabs_.OnAnimationSelected += animation =>
        this.sceneViewerPanel_.Animation = animation;
    this.modelTabs_.OnBoneSelected += bone => {
      var skeletonRenderer = this.sceneViewerPanel_.SkeletonRenderer;
      if (skeletonRenderer != null) {
        skeletonRenderer.SelectedBone = bone;
      }
    };

    this.modelToolStrip_.Progress.ProgressChanged +=
        (_, currentProgress) => {
          var fractionalProgress = currentProgress.Item1;
          this.cancellableProgressBar_.Value =
              (int) Math.Round(fractionalProgress * 100);

          var modelFileBundle = currentProgress.Item2;
          if (modelFileBundle == null) {
            if (fractionalProgress.IsRoughly0()) {
              this.cancellableProgressBar_.Text = "Nothing to report";
            } else if (fractionalProgress.IsRoughly1()) {
              this.cancellableProgressBar_.Text = "Done!";
            }
          } else {
            this.cancellableProgressBar_.Text =
                $"Extracting {modelFileBundle.DisplayFullPath}...";
          }
        };
    this.cancellableProgressBar_.Clicked += (sender, args)
        => this.modelToolStrip_.CancellationToken?.Cancel();

    SceneInstanceService.OnSceneInstanceOpened += this.UpdateScene_;
    AudioPlaylistService.OnPlaylistUpdated
        += playlist => this.audioPlayerPanel_.AudioFileBundles = playlist;
  }

  private void UniversalAssetToolForm_Load(object sender, EventArgs e) {
    var progress = new PercentageProgress();
    this.fileBundleTreeView_.Populate(
        new RootFileBundleGatherer().GatherAllFiles(progress));

    this.fpsCallback_ =
        TimedCallback.WithPeriod(
            () => {
              if (!this.Created || this.IsDisposed) {
                return;
              }

              try {
                this.Invoke(() => this.Text = FrameTime.FpsString);
              } catch {
                // ignored, throws after window is closed
              }
            },
            .25f);

    this.fileBundleTreeView_.DirectorySelected += this.OnDirectorySelect_;
    this.fileBundleTreeView_.FileSelected += this.OnFileBundleSelect_;
  }

  private void OnDirectorySelect_(IFileTreeParentNode directoryNode)
    => this.modelToolStrip_.DirectoryNode = directoryNode;

  private void OnFileBundleSelect_(IFileTreeLeafNode fileNode) {
    FileTreeLeafNodeService.OpenFileTreeLeafNode(fileNode);
  }

  private void UpdateScene_(IFileTreeLeafNode? fileNode,
                            ISceneInstance scene) {
    this.sceneViewerPanel_.Scene?.Dispose();
    this.sceneViewerPanel_.Scene = scene;

    var model = this.sceneViewerPanel_.FirstSceneModel?.Model;
    this.modelTabs_.Model = model;
    this.modelTabs_.AnimationPlaybackManager =
        this.sceneViewerPanel_.AnimationPlaybackManager;

    this.modelToolStrip_.DirectoryNode = fileNode?.Parent;
    this.modelToolStrip_.FileNodeAndModel = (fileNode, model);
    this.exportAsToolStripMenuItem.Enabled
        = model?.FileBundle is IModelFileBundle;
  }

  private void importToolstripMenuItem_Click(object sender, EventArgs e) {
    var plugins = PluginUtil.Plugins;
    var supportedExtensions =
        plugins.SelectMany(plugin => plugin.FileExtensions).ToHashSet();

    var dialog = new OpenFileDialog {
        CheckFileExists = true,
        Multiselect = true,
        Title = "Select asset(s) for a single model",
        Filter = $"All supported plugin extensions|{string.Join(';',
          supportedExtensions
              .Select(extension => $"*{extension}"))}",
    };
    dialog.FileOk += (o, args) => {
      var inputFiles =
          dialog.FileNames.Select(
              fileName => (IReadOnlySystemFile) new FinFile(fileName));

      var bestMatch =
          plugins.FirstOrDefault(plugin => plugin.SupportsFiles(inputFiles));
      if (bestMatch == null) {
        // TODO: Show an error dialog
        return;
      }

      var finModel = bestMatch.Import(inputFiles);
      ModelService.OpenModel(null, finModel);
    };

    dialog.ShowDialog();
  }

  private void exportAsToolStripMenuItem_Click(object sender, EventArgs e) {
    var scene = this.sceneViewerPanel_.Scene;
    var fileBundle = scene?.Definition.FileBundle;
    if (fileBundle is not IModelFileBundle modelBundle) {
      return;
    }

    // TODO: Merge models in a scene instead!
    var model = new GlobalModelImporter().Import(modelBundle);

    var allSupportedExportFormats = AssimpUtil.SupportedExportFormats
                                              .OrderBy(ef => ef.Description)
                                              .ToArray();
    var mergedFormat =
        $"Model files|{string.Join(';', allSupportedExportFormats.Select(ef => $"*.{ef.FileExtension}"))}";
    var filter = string.Join('|',
                             mergedFormat.Yield()
                                         .Concat(
                                             allSupportedExportFormats.Select(
                                                 ef
                                                     => $"{ef.Description}|*.{ef.FileExtension}")));

    var fbxIndex = allSupportedExportFormats.Select(ef => ef.FormatId)
                                            .IndexOfOrNegativeOne("fbx");

    var saveFileDialog = new SaveFileDialog();
    saveFileDialog.Filter = filter;
    saveFileDialog.FilterIndex = 2 + fbxIndex;
    saveFileDialog.OverwritePrompt = true;

    var result = saveFileDialog.ShowDialog();
    if (result == DialogResult.OK) {
      var outputFile = new FinFile(saveFileDialog.FileName);
      ExporterUtil.Export(
          modelBundle,
          () => model,
          outputFile.AssertGetParent(),
          new HashSet<ExportedFormat> { outputFile.FileType.AsFormat() },
          true,
          outputFile.NameWithoutExtension.ToString());
    }
  }

  private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    => this.Close();

  private void gitHubToolStripMenuItem_Click(object sender, EventArgs e)
    => WebBrowserUtil.OpenGithub();

  private void reportAnIssueToolStripMenuItem_Click(
      object sender,
      EventArgs e)
    => WebBrowserUtil.OpenGithubNewIssue();
}