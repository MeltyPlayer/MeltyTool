using System.Drawing;
using System.Windows.Forms;

using uni.ui.winforms.common;
using uni.ui.winforms.common.audio;
using uni.ui.winforms.common.scene;
using uni.ui.winforms.right_panel;
using uni.ui.winforms.top;


namespace uni.ui.winforms {
  partial class UniversalAssetToolForm {
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Windows.Forms.SplitContainer splitContainer1;
      System.Windows.Forms.SplitContainer splitContainer2;
      System.Windows.Forms.MenuStrip menuStrip;
      System.Windows.Forms.SplitContainer splitContainer3;
      this.fileBundleTreeView_ = new FileBundleTreeView();
      this.splitContainer4 = new System.Windows.Forms.SplitContainer();
      this.sceneViewerPanel_ = new SceneViewerPanel();
      this.audioPlayerPanel_ = new AudioPlayerPanel();
      this.modelTabs_ = new ModelTabs();
      this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.importToolStripMenuItem_ = new ToolStripMenuItem();
      this.exportAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.gitHubToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.reportAnIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.modelToolStrip_ = new ModelToolStrip();
      this.splitContainer5 = new System.Windows.Forms.SplitContainer();
      this.cancellableProgressBar_ = new CancellableProgressBar();
      splitContainer1 = new System.Windows.Forms.SplitContainer();
      splitContainer2 = new System.Windows.Forms.SplitContainer();
      menuStrip = new System.Windows.Forms.MenuStrip();
      splitContainer3 = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
      splitContainer1.Panel1.SuspendLayout();
      splitContainer1.Panel2.SuspendLayout();
      splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(splitContainer2)).BeginInit();
      splitContainer2.Panel1.SuspendLayout();
      splitContainer2.Panel2.SuspendLayout();
      splitContainer2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
      this.splitContainer4.Panel1.SuspendLayout();
      this.splitContainer4.Panel2.SuspendLayout();
      this.splitContainer4.SuspendLayout();
      menuStrip.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(splitContainer3)).BeginInit();
      splitContainer3.Panel2.SuspendLayout();
      splitContainer3.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
      this.splitContainer5.Panel1.SuspendLayout();
      this.splitContainer5.Panel2.SuspendLayout();
      this.splitContainer5.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer1
      // 
      splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer1.Location = new System.Drawing.Point(0, 0);
      splitContainer1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      splitContainer1.Panel1.Controls.Add(this.fileBundleTreeView_);
      // 
      // splitContainer1.Panel2
      // 
      splitContainer1.Panel2.Controls.Add(splitContainer2);
      splitContainer1.Size = new System.Drawing.Size(1158, 631);
      splitContainer1.SplitterDistance = 224;
      splitContainer1.SplitterWidth = 6;
      splitContainer1.TabIndex = 2;
      // 
      // fileBundleTreeView_
      // 
      this.fileBundleTreeView_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fileBundleTreeView_.Location = new System.Drawing.Point(0, 0);
      this.fileBundleTreeView_.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
      this.fileBundleTreeView_.Name = "fileBundleTreeView_";
      this.fileBundleTreeView_.Size = new System.Drawing.Size(224, 631);
      this.fileBundleTreeView_.TabIndex = 0;
      // 
      // splitContainer2
      // 
      splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      splitContainer2.Location = new System.Drawing.Point(0, 0);
      splitContainer2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      splitContainer2.Name = "splitContainer2";
      // 
      // splitContainer2.Panel1
      // 
      splitContainer2.Panel1.Controls.Add(this.splitContainer4);
      // 
      // splitContainer2.Panel2
      // 
      splitContainer2.Panel2.Controls.Add(this.modelTabs_);
      splitContainer2.Size = new System.Drawing.Size(928, 631);
      splitContainer2.SplitterDistance = 658;
      splitContainer2.SplitterWidth = 6;
      splitContainer2.TabIndex = 1;
      // 
      // splitContainer4
      // 
      this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer4.IsSplitterFixed = true;
      this.splitContainer4.Location = new System.Drawing.Point(0, 0);
      this.splitContainer4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.splitContainer4.Name = "splitContainer4";
      this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer4.Panel1
      // 
      this.splitContainer4.Panel1.Controls.Add(this.sceneViewerPanel_);
      // 
      // splitContainer4.Panel2
      // 
      this.splitContainer4.Panel2.Controls.Add(this.audioPlayerPanel_);
      this.splitContainer4.Size = new System.Drawing.Size(658, 631);
      this.splitContainer4.SplitterDistance = 568;
      this.splitContainer4.SplitterWidth = 5;
      this.splitContainer4.TabIndex = 0;
      // 
      // sceneViewerPanel_
      // 
      this.sceneViewerPanel_.Animation = null;
      this.sceneViewerPanel_.BackColor = System.Drawing.Color.Transparent;
      this.sceneViewerPanel_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.sceneViewerPanel_.Scene = null;
      this.sceneViewerPanel_.Location = new System.Drawing.Point(0, 0);
      this.sceneViewerPanel_.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
      this.sceneViewerPanel_.Name = "sceneViewerPanel_";
      this.sceneViewerPanel_.Size = new System.Drawing.Size(658, 568);
      this.sceneViewerPanel_.TabIndex = 0;
      // 
      // audioPlayerPanel_
      // 
      this.audioPlayerPanel_.AudioFileBundles = null;
      this.audioPlayerPanel_.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.audioPlayerPanel_.BackColor = System.Drawing.Color.Transparent;
      this.audioPlayerPanel_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.audioPlayerPanel_.Location = new System.Drawing.Point(0, 0);
      this.audioPlayerPanel_.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
      this.audioPlayerPanel_.Name = "audioPlayerPanel_";
      this.audioPlayerPanel_.Size = new System.Drawing.Size(658, 58);
      this.audioPlayerPanel_.TabIndex = 2;
      // 
      // modelTabs_
      // 
      this.modelTabs_.AnimationPlaybackManager = null;
      this.modelTabs_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.modelTabs_.Location = new System.Drawing.Point(0, 0);
      this.modelTabs_.Margin = new System.Windows.Forms.Padding(3, 7, 3, 7);
      this.modelTabs_.Name = "modelTabs_";
      this.modelTabs_.Size = new System.Drawing.Size(264, 631);
      this.modelTabs_.TabIndex = 0;
      // 
      // menuStrip
      // 
      menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
      menuStrip.Location = new System.Drawing.Point(0, 0);
      menuStrip.Name = "menuStrip";
      menuStrip.Padding = new System.Windows.Forms.Padding(8, 4, 0, 4);
      menuStrip.Size = new System.Drawing.Size(1158, 32);
      menuStrip.TabIndex = 1;
      menuStrip.Text = "menuStrip1";
      // 
      // fileToolStripMenuItem
      // 
      this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem_,
            this.exportAsToolStripMenuItem,
            this.exitToolStripMenuItem});
      this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
      this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
      this.fileToolStripMenuItem.Text = "File";
      // 
      // importToolStripMenuItem_
      // 
      this.importToolStripMenuItem_.Name = "importToolStripMenuItem_";
      this.importToolStripMenuItem_.Size = new Size(224, 26);
      this.importToolStripMenuItem_.Text = "Import";
      this.importToolStripMenuItem_.Click += this.importToolstripMenuItem_Click;
      // 
      // exportAsToolStripMenuItem
      // 
      this.exportAsToolStripMenuItem.Enabled = false;
      this.exportAsToolStripMenuItem.Name = "exportAsToolStripMenuItem";
      this.exportAsToolStripMenuItem.Size = new System.Drawing.Size(153, 26);
      this.exportAsToolStripMenuItem.Text = "Export as";
      this.exportAsToolStripMenuItem.Click += new System.EventHandler(this.exportAsToolStripMenuItem_Click);
      // 
      // exitToolStripMenuItem
      // 
      this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
      this.exitToolStripMenuItem.Size = new System.Drawing.Size(153, 26);
      this.exitToolStripMenuItem.Text = "Exit";
      this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
      // 
      // helpToolStripMenuItem
      // 
      this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitHubToolStripMenuItem,
            this.reportAnIssueToolStripMenuItem});
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 24);
      this.helpToolStripMenuItem.Text = "Help";
      // 
      // gitHubToolStripMenuItem
      // 
      this.gitHubToolStripMenuItem.Name = "gitHubToolStripMenuItem";
      this.gitHubToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
      this.gitHubToolStripMenuItem.Text = "View on GitHub";
      this.gitHubToolStripMenuItem.Click += new System.EventHandler(this.gitHubToolStripMenuItem_Click);
      // 
      // reportAnIssueToolStripMenuItem
      // 
      this.reportAnIssueToolStripMenuItem.Name = "reportAnIssueToolStripMenuItem";
      this.reportAnIssueToolStripMenuItem.Size = new System.Drawing.Size(196, 26);
      this.reportAnIssueToolStripMenuItem.Text = "Report an Issue";
      this.reportAnIssueToolStripMenuItem.Click += new System.EventHandler(this.reportAnIssueToolStripMenuItem_Click);
      // 
      // splitContainer3
      // 
      splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer3.IsSplitterFixed = true;
      splitContainer3.Location = new System.Drawing.Point(0, 32);
      splitContainer3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      splitContainer3.Name = "splitContainer3";
      splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer3.Panel1
      // 
      splitContainer3.Panel1.Controls.Add(this.modelToolStrip_);
      splitContainer3.Panel1MinSize = 28;
      // 
      // splitContainer3.Panel2
      // 
      splitContainer3.Panel2.Controls.Add(this.splitContainer5);
      splitContainer3.Size = new System.Drawing.Size(1158, 693);
      splitContainer3.SplitterDistance = 28;
      splitContainer3.SplitterWidth = 5;
      splitContainer3.TabIndex = 4;
      // 
      // modelToolStrip_
      // 
      this.modelToolStrip_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.modelToolStrip_.Location = new System.Drawing.Point(0, 0);
      this.modelToolStrip_.Name = "modelToolStrip_";
      this.modelToolStrip_.Size = new System.Drawing.Size(1013, 28);
      this.modelToolStrip_.TabIndex = 3;
      // 
      // splitContainer5
      // 
      this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer5.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.splitContainer5.IsSplitterFixed = true;
      this.splitContainer5.Location = new System.Drawing.Point(0, 0);
      this.splitContainer5.Name = "splitContainer5";
      this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer5.Panel1
      // 
      this.splitContainer5.Panel1.Controls.Add(splitContainer1);
      // 
      // splitContainer5.Panel2
      // 
      this.splitContainer5.Panel2.Controls.Add(this.cancellableProgressBar_);
      this.splitContainer5.Size = new System.Drawing.Size(1158, 660);
      this.splitContainer5.SplitterDistance = 631;
      this.splitContainer5.TabIndex = 3;
      // 
      // cancellableProgressBar_
      // 
      this.cancellableProgressBar_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.cancellableProgressBar_.Location = new System.Drawing.Point(0, 0);
      this.cancellableProgressBar_.Name = "cancellableProgressBar_";
      this.cancellableProgressBar_.Size = new System.Drawing.Size(1158, 25);
      this.cancellableProgressBar_.TabIndex = 0;
      this.cancellableProgressBar_.Value = 0;
      // 
      // UniversalAssetToolForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1158, 725);
      this.Controls.Add(splitContainer3);
      this.Controls.Add(menuStrip);
      this.MainMenuStrip = menuStrip;
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "UniversalAssetToolForm";
      this.Text = "Universal Asset Tool";
      this.Load += new System.EventHandler(this.UniversalAssetToolForm_Load);
      splitContainer1.Panel1.ResumeLayout(false);
      splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
      splitContainer1.ResumeLayout(false);
      splitContainer2.Panel1.ResumeLayout(false);
      splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer2)).EndInit();
      splitContainer2.ResumeLayout(false);
      this.splitContainer4.Panel1.ResumeLayout(false);
      this.splitContainer4.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
      this.splitContainer4.ResumeLayout(false);
      menuStrip.ResumeLayout(false);
      menuStrip.PerformLayout();
      splitContainer3.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer3)).EndInit();
      splitContainer3.ResumeLayout(false);
      this.splitContainer5.Panel1.ResumeLayout(false);
      this.splitContainer5.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
      this.splitContainer5.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private SceneViewerPanel sceneViewerPanel_;
    private MenuStrip menuStrip;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem importToolStripMenuItem_;
    private ToolStripMenuItem exportAsToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem gitHubToolStripMenuItem;
    private ToolStripMenuItem reportAnIssueToolStripMenuItem;
    private SplitContainer splitContainer1;
    private FileBundleTreeView fileBundleTreeView_;
    private ModelTabs modelTabs_;
    private top.ModelToolStrip modelToolStrip_;
    private SplitContainer splitContainer4;
    private AudioPlayerPanel audioPlayerPanel_;
    private SplitContainer splitContainer5;
    private CancellableProgressBar cancellableProgressBar_;
  }
}