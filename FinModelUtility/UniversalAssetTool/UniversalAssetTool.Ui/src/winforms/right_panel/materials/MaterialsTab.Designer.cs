using System.Windows.Forms;

using uni.ui.winforms.right_panel.textures;

namespace uni.ui.winforms.right_panel.materials {
  partial class MaterialsTab {
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
      SplitContainer splitContainer;
      this.materialSelector_ = new MaterialSelector();
      this.splitContainer2 = new SplitContainer();
      this.textureSection_ = new TextureSection();
      this.shaderSection_ = new ShaderSection();
      splitContainer = new SplitContainer();
      ((System.ComponentModel.ISupportInitialize) splitContainer).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) this.splitContainer2).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer
      // 
      splitContainer.Dock = DockStyle.Fill;
      splitContainer.FixedPanel = FixedPanel.Panel1;
      splitContainer.IsSplitterFixed = true;
      splitContainer.Location = new System.Drawing.Point(0, 0);
      splitContainer.Margin = new Padding(3, 4, 3, 4);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.materialSelector_);
      splitContainer.Panel1MinSize = 23;
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.splitContainer2);
      splitContainer.Size = new System.Drawing.Size(338, 608);
      splitContainer.SplitterDistance = 25;
      splitContainer.SplitterWidth = 5;
      splitContainer.TabIndex = 0;
      // 
      // materialSelector_
      // 
      this.materialSelector_.Dock = DockStyle.Fill;
      this.materialSelector_.Location = new System.Drawing.Point(0, 0);
      this.materialSelector_.Margin = new Padding(3, 5, 3, 5);
      this.materialSelector_.Name = "materialSelector_";
      this.materialSelector_.SelectedMaterial = null;
      this.materialSelector_.Size = new System.Drawing.Size(338, 25);
      this.materialSelector_.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Margin = new Padding(3, 4, 3, 4);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.textureSection_);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.shaderSection_);
      this.splitContainer2.Size = new System.Drawing.Size(338, 578);
      this.splitContainer2.SplitterDistance = 196;
      this.splitContainer2.SplitterWidth = 5;
      this.splitContainer2.TabIndex = 1;
      // 
      // textureSection_
      // 
      this.textureSection_.Dock = DockStyle.Fill;
      this.textureSection_.Location = new System.Drawing.Point(0, 0);
      this.textureSection_.Margin = new Padding(3, 5, 3, 5);
      this.textureSection_.Name = "textureSection_";
      this.textureSection_.Size = new System.Drawing.Size(338, 196);
      this.textureSection_.TabIndex = 0;
      // 
      // shaderSection_
      // 
      this.shaderSection_.Dock = DockStyle.Fill;
      this.shaderSection_.Location = new System.Drawing.Point(0, 0);
      this.shaderSection_.Margin = new Padding(3, 5, 3, 5);
      this.shaderSection_.Name = "shaderSection_";
      this.shaderSection_.Size = new System.Drawing.Size(338, 377);
      this.shaderSection_.TabIndex = 0;
      // 
      // MaterialsTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add(splitContainer);
      this.Margin = new Padding(3, 4, 3, 4);
      this.Name = "MaterialsTab";
      this.Size = new System.Drawing.Size(338, 608);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) splitContainer).EndInit();
      splitContainer.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) this.splitContainer2).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);
    }

    #endregion

    private MaterialSelector materialSelector_;
    private SplitContainer splitContainer2;
    private TextureSection textureSection_;
    private ShaderSection shaderSection_;
  }
}
