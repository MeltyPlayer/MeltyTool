using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.winforms.common {
  partial class ColorPicker {
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
      this.SuspendLayout();
      // 
      // ColorPicker
      // 
      this.AutoScaleDimensions = new SizeF(8F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Name = "ColorPicker";
      this.Size = new Size(201, 41);
      this.ResumeLayout(false);
    }

    #endregion
  }
}
