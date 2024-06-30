using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.materials {
  partial class MaterialSelector {
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
      this.comboBox_ = new System.Windows.Forms.ComboBox();
      this.SuspendLayout();
      // 
      // comboBox_
      // 
      this.comboBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.comboBox_.FormattingEnabled = true;
      this.comboBox_.Location = new System.Drawing.Point(0, 0);
      this.comboBox_.Name = "comboBox_";
      this.comboBox_.Size = new System.Drawing.Size(375, 23);
      this.comboBox_.TabIndex = 0;
      // 
      // MaterialSelector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.comboBox_);
      this.Name = "MaterialSelector";
      this.Size = new System.Drawing.Size(375, 23);
      this.ResumeLayout(false);

    }

    #endregion

    private ComboBox comboBox_;
  }
}
