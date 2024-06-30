namespace uni.ui.winforms.common.fileTreeView {
  partial class FileTreeView<TFiles> {
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
      this.filterTextBox_ = new WaterMarkTextBox();
      this.fileTreeView_ = new System.Windows.Forms.TreeView();
      splitContainer = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize) (splitContainer)).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer
      // 
      splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer.IsSplitterFixed = true;
      splitContainer.Location = new System.Drawing.Point(0, 0);
      splitContainer.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.filterTextBox_);
      splitContainer.Panel1MinSize = 20;
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.fileTreeView_);
      splitContainer.Size = new System.Drawing.Size(300, 965);
      splitContainer.SplitterDistance = 25;
      splitContainer.SplitterWidth = 6;
      splitContainer.TabIndex = 0;
      // 
      // filterTextBox_
      // 
      this.filterTextBox_.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.filterTextBox_.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.filterTextBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.filterTextBox_.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
      this.filterTextBox_.Location = new System.Drawing.Point(0, 0);
      this.filterTextBox_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.filterTextBox_.Name = "filterTextBox_";
      this.filterTextBox_.Size = new System.Drawing.Size(300, 23);
      this.filterTextBox_.TabIndex = 1;
      this.filterTextBox_.WaterMarkColor = System.Drawing.Color.Gray;
      this.filterTextBox_.WaterMarkText = "Filter files...";
      // 
      // fileTreeView_
      // 
      this.fileTreeView_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fileTreeView_.HideSelection = false;
      this.fileTreeView_.HotTracking = true;
      this.fileTreeView_.Location = new System.Drawing.Point(0, 0);
      this.fileTreeView_.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.fileTreeView_.Name = "fileTreeView_";
      this.fileTreeView_.Size = new System.Drawing.Size(300, 934);
      this.fileTreeView_.TabIndex = 15;
      // 
      // FileTreeView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(splitContainer);
      this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      this.Name = "FileTreeView";
      this.Size = new System.Drawing.Size(300, 965);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel1.PerformLayout();
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) (splitContainer)).EndInit();
      splitContainer.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.TreeView fileTreeView_;
    private WaterMarkTextBox filterTextBox_;
  }
}