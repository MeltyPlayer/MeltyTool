using System.Windows.Forms;

namespace uni.ui.winforms.common {
  partial class LabelledProgressBar {
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
      this.progressBar_ = new System.Windows.Forms.ProgressBar();
      this.label_ = new TransparentLabel();
      this.SuspendLayout();
      // 
      // progressBar_
      // 
      this.progressBar_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.progressBar_.Location = new System.Drawing.Point(0, 0);
      this.progressBar_.Name = "progressBar_";
      this.progressBar_.Size = new System.Drawing.Size(150, 150);
      this.progressBar_.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.progressBar_.TabIndex = 0;
      // 
      // label_
      // 
      this.label_.AutoSize = true;
      this.label_.BackColor = System.Drawing.Color.Transparent;
      this.label_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label_.Location = new System.Drawing.Point(0, 0);
      this.label_.Name = "label_";
      this.label_.Size = new System.Drawing.Size(128, 20);
      this.label_.TabIndex = 1;
      this.label_.Text = "transparentLabel1";
      // 
      // LabelledProgressBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label_);
      this.Controls.Add(this.progressBar_);
      this.Name = "LabelledProgressBar";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private ProgressBar progressBar_;
    private TransparentLabel label_;
  }
}
