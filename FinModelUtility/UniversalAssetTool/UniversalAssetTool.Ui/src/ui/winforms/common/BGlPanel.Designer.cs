using System;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.WinForms;

namespace uni.ui.winforms.common {
  abstract partial class BGlPanel {
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
    private unsafe void InitializeComponent() {
      var glControlSettings = GLControlSettings.Default;

      glControlSettings.Profile = ContextProfile.Compatability;
      glControlSettings.SharedContext = new GLFWGraphicsContext(null);

      this.impl_ = new GLControl(glControlSettings);

      this.SuspendLayout();
      // 
      // impl_
      // 
      this.impl_.BackColor = System.Drawing.Color.Black;
      this.impl_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.impl_.Location = new System.Drawing.Point(0, 0);
      this.impl_.Name = "impl_";
      this.impl_.Size = new System.Drawing.Size(511, 331);
      this.impl_.TabIndex = 0;
      // 
      // BGlPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.Fuchsia;
      this.Controls.Add(this.impl_);
      this.Name = "BGlPanel";
      this.Size = new System.Drawing.Size(511, 331);
      this.ResumeLayout(false);

    }

    #endregion

    protected GLControl impl_;
  }
}
