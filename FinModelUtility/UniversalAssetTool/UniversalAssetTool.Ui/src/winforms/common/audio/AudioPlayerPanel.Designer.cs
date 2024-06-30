using System.Windows.Forms;

namespace uni.ui.winforms.common.audio {
  partial class AudioPlayerPanel {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (this.components != null)) {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.groupBox_ = new System.Windows.Forms.GroupBox();
      this.impl_ = new AudioPlayerGlPanel();
      this.groupBox_.SuspendLayout();
      this.SuspendLayout();
      // 
      // groupBox_
      // 
      this.groupBox_.Controls.Add(this.impl_);
      this.groupBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox_.Location = new System.Drawing.Point(0, 0);
      this.groupBox_.Name = "groupBox_";
      this.groupBox_.Size = new System.Drawing.Size(462, 310);
      this.groupBox_.TabIndex = 0;
      this.groupBox_.TabStop = false;
      this.groupBox_.Text = "(Select audio)";
      // 
      // impl_
      // 
      this.impl_.AudioFileBundles = null;
      this.impl_.BackColor = System.Drawing.Color.Fuchsia;
      this.impl_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.impl_.Location = new System.Drawing.Point(3, 19);
      this.impl_.Name = "impl_";
      this.impl_.Size = new System.Drawing.Size(456, 288);
      this.impl_.TabIndex = 0;
      // 
      // AudioPlayerPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox_);
      this.Name = "AudioPlayerPanel";
      this.Size = new System.Drawing.Size(462, 310);
      this.groupBox_.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private GroupBox groupBox_;
    private AudioPlayerGlPanel impl_;
  }
}
