namespace uni.ui.winforms.right_panel.skeleton {
  partial class SkeletonTab {
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
      this.skeletonTreeView_ = new SkeletonTreeView();
      this.SuspendLayout();
      // 
      // skeletonTreeView_
      // 
      this.skeletonTreeView_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.skeletonTreeView_.Location = new System.Drawing.Point(0, 0);
      this.skeletonTreeView_.Margin = new System.Windows.Forms.Padding(4);
      this.skeletonTreeView_.Name = "skeletonTreeView_";
      this.skeletonTreeView_.Size = new System.Drawing.Size(245, 338);
      this.skeletonTreeView_.TabIndex = 0;
      // 
      // SkeletonTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.skeletonTreeView_);
      this.Name = "SkeletonTab";
      this.Size = new System.Drawing.Size(245, 338);
      this.ResumeLayout(false);
    }

    #endregion

    private skeleton.SkeletonTreeView skeletonTreeView_;
  }
}
