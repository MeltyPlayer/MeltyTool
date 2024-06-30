using System.Windows.Forms;

using uni.ui.winforms.right_panel.textures;

namespace uni.ui.winforms.right_panel.materials {
  partial class TextureSection {
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
      System.Windows.Forms.GroupBox groupBox;
      this.texturePanel_ = new TexturePanel();
      this.textureSelectorBox_ = new TextureSelectorBox();
      splitContainer = new System.Windows.Forms.SplitContainer();
      groupBox = new System.Windows.Forms.GroupBox();
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      groupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // texturePanel_
      // 
      this.texturePanel_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.texturePanel_.Location = new System.Drawing.Point(0, 0);
      this.texturePanel_.Name = "texturePanel_";
      this.texturePanel_.Size = new System.Drawing.Size(227, 237);
      this.texturePanel_.TabIndex = 0;
      // 
      // textureSelectorBox_
      // 
      this.textureSelectorBox_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textureSelectorBox_.Location = new System.Drawing.Point(0, 0);
      this.textureSelectorBox_.Name = "textureSelectorBox_";
      this.textureSelectorBox_.SelectedTexture = null;
      this.textureSelectorBox_.Size = new System.Drawing.Size(474, 237);
      this.textureSelectorBox_.TabIndex = 0;
      // 
      // splitContainer
      // 
      splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer.Location = new System.Drawing.Point(3, 19);
      splitContainer.Name = "splitContainer";
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.textureSelectorBox_);
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.texturePanel_);
      splitContainer.Size = new System.Drawing.Size(705, 237);
      splitContainer.SplitterDistance = 474;
      splitContainer.TabIndex = 0;
      // 
      // groupBox
      // 
      groupBox.Controls.Add(splitContainer);
      groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      groupBox.Location = new System.Drawing.Point(0, 0);
      groupBox.Name = "groupBox";
      groupBox.Size = new System.Drawing.Size(711, 259);
      groupBox.TabIndex = 1;
      groupBox.TabStop = false;
      groupBox.Text = "Textures";
      // 
      // TextureSection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(groupBox);
      this.Name = "TextureSection";
      this.Size = new System.Drawing.Size(711, 259);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).EndInit();
      splitContainer.ResumeLayout(false);
      groupBox.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private TexturePanel texturePanel_;
    private TextureSelectorBox textureSelectorBox_;
    private GroupBox groupBox;
  }
}
