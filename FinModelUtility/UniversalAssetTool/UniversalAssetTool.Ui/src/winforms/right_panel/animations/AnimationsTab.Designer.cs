using System.Windows.Forms;

using fin.animation;

namespace uni.ui.winforms.right_panel {
  partial class AnimationsTab {
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
      System.Windows.Forms.SplitContainer splitContainer;
      this.listView_ = new System.Windows.Forms.ListView();
      this.animationPlaybackPanel_ = new AnimationPlaybackPanel();
      splitContainer = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer
      // 
      splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      splitContainer.IsSplitterFixed = true;
      splitContainer.Location = new System.Drawing.Point(0, 0);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.listView_);
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.animationPlaybackPanel_);
      splitContainer.Size = new System.Drawing.Size(245, 338);
      splitContainer.SplitterDistance = 198;
      splitContainer.TabIndex = 0;
      // 
      // listView_
      // 
      this.listView_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listView_.Location = new System.Drawing.Point(0, 0);
      this.listView_.MultiSelect = false;
      this.listView_.Name = "listView_";
      this.listView_.Size = new System.Drawing.Size(245, 198);
      this.listView_.TabIndex = 0;
      this.listView_.UseCompatibleStateImageBehavior = false;
      this.listView_.View = System.Windows.Forms.View.List;
      // 
      // animationPlaybackPanel_
      // 
      this.animationPlaybackPanel_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.animationPlaybackPanel_.Frame = 0D;
      this.animationPlaybackPanel_.FrameRate = 20;
      this.animationPlaybackPanel_.IsPlaying = true;
      this.animationPlaybackPanel_.Location = new System.Drawing.Point(0, 0);
      this.animationPlaybackPanel_.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.animationPlaybackPanel_.Name = "animationPlaybackPanel_";
      this.animationPlaybackPanel_.Config = new AnimationInterpolationConfig { UseLoopingInterpolation = true};
      this.animationPlaybackPanel_.Size = new System.Drawing.Size(245, 136);
      this.animationPlaybackPanel_.TabIndex = 1;
      this.animationPlaybackPanel_.TotalFrames = 0;
      // 
      // AnimationsTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(splitContainer);
      this.Name = "AnimationsTab";
      this.Size = new System.Drawing.Size(245, 338);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).EndInit();
      splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private SplitContainer splitContainer;
    private AnimationPlaybackPanel animationPlaybackPanel_;
    private ListView listView_;
  }
}
