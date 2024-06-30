using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.textures {
  partial class TexturePanel {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TexturePanel));
      this.groupBox_ = new System.Windows.Forms.GroupBox();
      this.pictureBox_ = new System.Windows.Forms.PictureBox();
      this.groupBox_.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox_)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox_
      // 
      this.groupBox_.Controls.Add(this.pictureBox_);
      this.groupBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox_.Location = new System.Drawing.Point(0, 0);
      this.groupBox_.Name = "groupBox_";
      this.groupBox_.Size = new System.Drawing.Size(205, 184);
      this.groupBox_.TabIndex = 0;
      this.groupBox_.TabStop = false;
      this.groupBox_.Text = "groupBox1";
      // 
      // pictureBox_
      // 
      this.pictureBox_.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox_.BackgroundImage")));
      this.pictureBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.pictureBox_.Location = new System.Drawing.Point(3, 19);
      this.pictureBox_.Name = "pictureBox_";
      this.pictureBox_.Size = new System.Drawing.Size(199, 162);
      this.pictureBox_.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.pictureBox_.TabIndex = 1;
      this.pictureBox_.TabStop = false;
      // 
      // TexturePanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.groupBox_);
      this.Name = "TexturePanel";
      this.Size = new System.Drawing.Size(205, 184);
      this.groupBox_.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox_)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private GroupBox groupBox_;
    private PictureBox pictureBox_;
  }
}
