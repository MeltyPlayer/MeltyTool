using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.info {
  partial class InfoTab {
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
      System.Windows.Forms.GroupBox filesGroupBox;
      this.filesListBox_ = new System.Windows.Forms.ListBox();
      filesGroupBox = new System.Windows.Forms.GroupBox();
      filesGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // filesGroupBox
      // 
      filesGroupBox.Controls.Add(this.filesListBox_);
      filesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      filesGroupBox.Location = new System.Drawing.Point(0, 0);
      filesGroupBox.Name = "filesGroupBox";
      filesGroupBox.Size = new System.Drawing.Size(280, 451);
      filesGroupBox.TabIndex = 1;
      filesGroupBox.TabStop = false;
      filesGroupBox.Text = "Files";
      // 
      // filesListBox_
      // 
      this.filesListBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.filesListBox_.FormattingEnabled = true;
      this.filesListBox_.IntegralHeight = false;
      this.filesListBox_.ItemHeight = 20;
      this.filesListBox_.Location = new System.Drawing.Point(3, 23);
      this.filesListBox_.Name = "filesListBox_";
      this.filesListBox_.Size = new System.Drawing.Size(274, 425);
      this.filesListBox_.TabIndex = 0;
      // 
      // InfoTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(filesGroupBox);
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "InfoTab";
      this.Size = new System.Drawing.Size(280, 451);
      filesGroupBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private ListBox filesListBox_;
  }
}
