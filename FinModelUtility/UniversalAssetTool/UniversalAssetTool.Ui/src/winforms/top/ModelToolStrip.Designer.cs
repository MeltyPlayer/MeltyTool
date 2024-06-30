using System.Windows.Forms;

namespace uni.ui.winforms.top {
  partial class ModelToolStrip {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Windows.Forms.ToolStrip toolStrip;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModelToolStrip));
      this.exportAllModelsInSelectedDirectoryButton_ = new System.Windows.Forms.ToolStripButton();
      this.exportSelectedModelButton_ = new System.Windows.Forms.ToolStripButton();
      this.showBonesButton_ = new System.Windows.Forms.ToolStripButton();
      this.showGridButton_ = new System.Windows.Forms.ToolStripButton();
      this.automaticallyPlayMusicButton_ = new System.Windows.Forms.ToolStripButton();
      toolStrip = new System.Windows.Forms.ToolStrip();
      toolStrip.SuspendLayout();
      this.SuspendLayout();
      // 
      // toolStrip
      // 
      toolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
      toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllModelsInSelectedDirectoryButton_,
            this.exportSelectedModelButton_,
            this.showBonesButton_,
            this.showGridButton_,
            this.automaticallyPlayMusicButton_});
      toolStrip.Location = new System.Drawing.Point(0, 0);
      toolStrip.Name = "toolStrip";
      toolStrip.Size = new System.Drawing.Size(515, 31);
      toolStrip.Stretch = true;
      toolStrip.TabIndex = 0;
      toolStrip.Text = "toolStrip1";
      // 
      // exportAllModelsInSelectedDirectoryButton_
      // 
      this.exportAllModelsInSelectedDirectoryButton_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.exportAllModelsInSelectedDirectoryButton_.Enabled = false;
      this.exportAllModelsInSelectedDirectoryButton_.Image = ((System.Drawing.Image)(resources.GetObject("exportAllModelsInSelectedDirectoryButton_.Image")));
      this.exportAllModelsInSelectedDirectoryButton_.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.exportAllModelsInSelectedDirectoryButton_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.exportAllModelsInSelectedDirectoryButton_.Name = "exportAllModelsInSelectedDirectoryButton_";
      this.exportAllModelsInSelectedDirectoryButton_.Size = new System.Drawing.Size(29, 28);
      this.exportAllModelsInSelectedDirectoryButton_.Text = "Export All";
      this.exportAllModelsInSelectedDirectoryButton_.ToolTipText = "Export all models in selected directory";
      this.exportAllModelsInSelectedDirectoryButton_.Click += new System.EventHandler(this.exportAllModelsInSelectedDirectoryButton__Click);
      // 
      // exportSelectedModelButton_
      // 
      this.exportSelectedModelButton_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.exportSelectedModelButton_.Enabled = false;
      this.exportSelectedModelButton_.Image = ((System.Drawing.Image)(resources.GetObject("exportSelectedModelButton_.Image")));
      this.exportSelectedModelButton_.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.exportSelectedModelButton_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.exportSelectedModelButton_.Name = "exportSelectedModelButton_";
      this.exportSelectedModelButton_.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
      this.exportSelectedModelButton_.Size = new System.Drawing.Size(29, 28);
      this.exportSelectedModelButton_.Text = "Export";
      this.exportSelectedModelButton_.ToolTipText = "Export selected model";
      this.exportSelectedModelButton_.Click += new System.EventHandler(this.exportSelectedModelButton__Click);
      // 
      // showBonesButton_
      // 
      this.showBonesButton_.Checked = true;
      this.showBonesButton_.CheckOnClick = true;
      this.showBonesButton_.CheckState = System.Windows.Forms.CheckState.Checked;
      this.showBonesButton_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.showBonesButton_.Image = ((System.Drawing.Image)(resources.GetObject("showBonesButton_.Image")));
      this.showBonesButton_.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.showBonesButton_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.showBonesButton_.Name = "showBonesButton_";
      this.showBonesButton_.Size = new System.Drawing.Size(29, 28);
      this.showBonesButton_.Text = "Toggle Show Bones";
      this.showBonesButton_.ToolTipText = "Toggle show bones";
      // 
      // showGridButton_
      // 
      this.showGridButton_.Checked = true;
      this.showGridButton_.CheckOnClick = true;
      this.showGridButton_.CheckState = System.Windows.Forms.CheckState.Checked;
      this.showGridButton_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.showGridButton_.Image = ((System.Drawing.Image)(resources.GetObject("showGridButton_.Image")));
      this.showGridButton_.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.showGridButton_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.showGridButton_.Name = "showGridButton_";
      this.showGridButton_.Size = new System.Drawing.Size(29, 28);
      this.showGridButton_.Text = "Toggle Show Grid";
      this.showGridButton_.ToolTipText = "Toggle show grid";
      // 
      // automaticallyPlayMusicButton_
      // 
      this.automaticallyPlayMusicButton_.CheckOnClick = true;
      this.automaticallyPlayMusicButton_.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
      this.automaticallyPlayMusicButton_.Image = ((System.Drawing.Image)(resources.GetObject("automaticallyPlayMusicButton_.Image")));
      this.automaticallyPlayMusicButton_.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
      this.automaticallyPlayMusicButton_.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.automaticallyPlayMusicButton_.Name = "automaticallyPlayMusicButton_";
      this.automaticallyPlayMusicButton_.Size = new System.Drawing.Size(29, 28);
      this.automaticallyPlayMusicButton_.Text = "Automatically Play Music from Model\'s Game";
      this.automaticallyPlayMusicButton_.ToolTipText = "Automatically play music from model\'s game";
      // 
      // ModelToolStrip
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(toolStrip);
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "ModelToolStrip";
      this.Size = new System.Drawing.Size(515, 41);
      toolStrip.ResumeLayout(false);
      toolStrip.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private ToolStrip toolStrip;
    private ToolStripButton exportSelectedModelButton_;
    private ToolStripButton exportAllModelsInSelectedDirectoryButton_;
        private ToolStripButton showBonesButton_;
    private ToolStripButton showGridButton_;
    private ToolStripButton automaticallyPlayMusicButton_;
  }
}
