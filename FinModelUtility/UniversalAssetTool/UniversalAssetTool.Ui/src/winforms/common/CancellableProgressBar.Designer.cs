using System.Windows.Forms;

namespace uni.ui.winforms.common {
  partial class CancellableProgressBar {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CancellableProgressBar));
      this.labelledProgressBar_ = new LabelledProgressBar();
      this.cancelButton_ = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // labelledProgressBar_
      // 
      this.labelledProgressBar_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.labelledProgressBar_.Location = new System.Drawing.Point(0, 0);
      this.labelledProgressBar_.Name = "labelledProgressBar_";
      this.labelledProgressBar_.Size = new System.Drawing.Size(576, 25);
      this.labelledProgressBar_.TabIndex = 0;
      this.labelledProgressBar_.Value = 0;
      // 
      // cancelButton_
      // 
      this.cancelButton_.BackColor = System.Drawing.Color.Transparent;
      this.cancelButton_.Dock = System.Windows.Forms.DockStyle.Right;
      this.cancelButton_.Enabled = false;
      this.cancelButton_.FlatAppearance.BorderSize = 0;
      this.cancelButton_.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
      this.cancelButton_.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton_.Image")));
      this.cancelButton_.Location = new System.Drawing.Point(551, 0);
      this.cancelButton_.Name = "cancelButton_";
      this.cancelButton_.Size = new System.Drawing.Size(25, 25);
      this.cancelButton_.TabIndex = 1;
      this.cancelButton_.UseVisualStyleBackColor = false;
      // 
      // CancellableProgressBar
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.cancelButton_);
      this.Controls.Add(this.labelledProgressBar_);
      this.Name = "CancellableProgressBar";
      this.Size = new System.Drawing.Size(576, 25);
      this.ResumeLayout(false);

    }

    #endregion

    private LabelledProgressBar labelledProgressBar_;
    private Button cancelButton_;
  }
}
