using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using fin.io.bundles;
using fin.math.floats;
using fin.model;
using fin.model.io;
using fin.util.progress;

using uni.api;
using uni.config;
using uni.games;
using uni.ui.winforms.common.fileTreeView;

using static uni.games.ExporterUtil;

namespace uni.ui.winforms.top;

public partial class ModelToolStrip : UserControl {
  private IFileTreeParentNode? directoryNode_;
  private (IFileTreeLeafNode, IReadOnlyModel)? fileNodeAndModel_;

  private bool hasModelsInDirectory_;
  private bool isModelSelected_;

  public ModelToolStrip() {
    InitializeComponent();

    var config = Config.Instance;
    var viewerSettings = config.Viewer;

    var showBonesButton = this.showBonesButton_;
    showBonesButton.Checked = viewerSettings.ShowSkeleton;
    showBonesButton.CheckedChanged += (_, e) => {
      viewerSettings.ShowSkeleton = showBonesButton.Checked;
    };

    var showGridButton = this.showGridButton_;
    showGridButton.Checked = viewerSettings.ShowGrid;
    showGridButton.CheckedChanged += (_, e) => {
      viewerSettings.ShowGrid = showGridButton.Checked;
    };

    var automaticallyPlayMusicButton = this.automaticallyPlayMusicButton_;
    automaticallyPlayMusicButton.Checked =
        viewerSettings.AutomaticallyPlayGameAudioForModel;
    automaticallyPlayMusicButton.CheckedChanged += (_, e) => {
      viewerSettings.AutomaticallyPlayGameAudioForModel =
          showGridButton.Checked;
    };

    this.Progress.ProgressChanged += (_, e) => {
      this.AttemptToUpdateExportSelectedModelButtonEnabledState_();
      this
          .AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_();
    };
  }

  public MemoryProgress<(float, IModelFileBundle?)> Progress { get; } =
    new((0, null));

  public CancellationTokenSource? CancellationToken { get; private set; }

  public bool IsStarted => this.CancellationToken != null;

  public bool IsInProgress
    => this.IsStarted && !this.Progress.Current.Item1.IsRoughly1();

  public IFileTreeParentNode? DirectoryNode {
    set {
      var hasDirectory = value != null;
      this.directoryNode_ = value;

      var tooltipText = "Export all models in selected directory";
      var modelCount = 0;
      if (hasDirectory) {
        modelCount = value!.GetFilesOfType<IModelFileBundle>(true)
                           .Count();

        var totalText = this.GetTotalNodeText_(value!);
        tooltipText = modelCount == 1
            ? $"Export {modelCount} model in '{totalText}'"
            : $"Export all {modelCount} models in '{totalText}'";
      }

      this.hasModelsInDirectory_ = modelCount > 0;
      this
          .AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_();
      this.exportAllModelsInSelectedDirectoryButton_.ToolTipText =
          tooltipText;
    }
  }

  public (IFileTreeLeafNode?, IReadOnlyModel?) FileNodeAndModel {
    set {
      var (fileNode, model) = value;

      this.isModelSelected_ =
          fileNode?.File.IsOfType<IModelFileBundle>(out _) ?? false;

      if (this.isModelSelected_) {
        this.fileNodeAndModel_ = (fileNode, model!);
      } else {
        this.fileNodeAndModel_ = null;
      }

      var tooltipText = "Export selected model";
      if (this.isModelSelected_) {
        var totalText = this.GetTotalNodeText_(fileNode!);
        tooltipText = $"Export '{totalText}'";
      }

      this.AttemptToUpdateExportSelectedModelButtonEnabledState_();
      this.exportSelectedModelButton_.ToolTipText = tooltipText;
    }
  }

  public void AttemptToUpdateExportSelectedModelButtonEnabledState_() {
    this.exportSelectedModelButton_.Enabled =
        !this.IsInProgress && this.isModelSelected_;
  }

  public void
      AttemptToUpdateExportAllModelsInSelectedDirectoryButtonEnabledState_() {
    this.exportAllModelsInSelectedDirectoryButton_.Enabled =
        !this.IsInProgress && this.hasModelsInDirectory_;
  }

  private void exportAllModelsInSelectedDirectoryButton__Click(
      object sender,
      EventArgs e) {
    var models =
        this.directoryNode_!.GetFilesOfType<IModelFileBundle>(true)
            .ToArray();
    this.StartExportingModelsInBackground_(models);
  }

  private void exportSelectedModelButton__Click(object sender, EventArgs e) {
    if (this.fileNodeAndModel_ == null) {
      return;
    }

    var (fileNode, _) = this.fileNodeAndModel_.Value;
    if (fileNode.File.IsOfType<IModelFileBundle>(out var modelFileBundle)) {
      this.StartExportingModelsInBackground_([modelFileBundle]);
    }
  }

  private void StartExportingModelsInBackground_(
      IReadOnlyList<IAnnotatedFileBundle<IModelFileBundle>>
          modelFileBundles) {
    var extractorPromptChoice =
        PromptIfModelFileBundlesAlreadyExported_(
            modelFileBundles,
            Config.Instance.Exporter.General.ExportedFormats);
    if (extractorPromptChoice != ExporterPromptChoice.CANCEL) {
      this.CancellationToken = new CancellationTokenSource();

      Task.Run(() => {
        ExporterUtil.ExportAll(modelFileBundles,
                               new GlobalModelImporter(),
                               this.Progress,
                               this.CancellationToken,
                               Config.Instance.Exporter.General
                                     .ExportedFormats,
                               extractorPromptChoice ==
                               ExporterPromptChoice
                                   .OVERWRITE_EXISTING);
      });
    }
  }

  private string GetTotalNodeText_(IFileTreeNode node) {
    var totalText = "";
    var directory = node;
    while (true) {
      if (totalText.Length > 0) {
        totalText = "/" + totalText;
      }

      totalText = directory.Text + totalText;

      directory = directory.Parent;
      if (directory?.Parent == null) {
        break;
      }
    }

    return totalText;
  }

  private static ExporterPromptChoice
      PromptIfModelFileBundlesAlreadyExported_(
          IReadOnlyList<IAnnotatedFileBundle> modelFileBundles,
          IReadOnlyList<string> extensions) {
    if (ExporterUtil.CheckIfModelFileBundlesAlreadyExported(
            modelFileBundles,
            extensions,
            out var existingOutputFiles)) {
      var totalCount = modelFileBundles.Count;
      if (totalCount == 1) {
        var result =
            MessageBox.Show(
                $"Model defined in \"{existingOutputFiles.First().FileBundle.DisplayFullPath}\" has already been exported. Would you like to overwrite it?",
                "Model has already been exported!",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);
        return result switch {
            DialogResult.Yes => ExporterPromptChoice.OVERWRITE_EXISTING,
            DialogResult.No  => ExporterPromptChoice.CANCEL,
        };
      } else {
        var existingCount = existingOutputFiles.Count();
        var result =
            MessageBox.Show(
                $"{existingCount} model{(existingCount != 1 ? "s have" : " has")} already been exported. Select 'Yes' to overwrite them, 'No' to skip them, or 'Cancel' to abort this operation.",
                $"{existingCount}/{totalCount} models have already been exported!",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1);
        return result switch {
            DialogResult.Yes    => ExporterPromptChoice.OVERWRITE_EXISTING,
            DialogResult.No     => ExporterPromptChoice.SKIP_EXISTING,
            DialogResult.Cancel => ExporterPromptChoice.CANCEL,
        };
      }
    }

    return ExporterPromptChoice.SKIP_EXISTING;
  }
}