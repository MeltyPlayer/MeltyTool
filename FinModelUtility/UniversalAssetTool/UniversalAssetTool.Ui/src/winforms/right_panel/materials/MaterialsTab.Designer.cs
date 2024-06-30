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
      System.Windows.Forms.SplitContainer splitContainer;
      this.materialSelector_ = new MaterialSelector();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.splitContainer2 = new System.Windows.Forms.SplitContainer();
      this.textureSection_ = new TextureSection();
      this.shaderSection_ = new ShaderSection();
      this.materialViewerPanel1 = new MaterialViewerPanel();
      splitContainer = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).BeginInit();
      splitContainer.Panel1.SuspendLayout();
      splitContainer.Panel2.SuspendLayout();
      splitContainer.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
      this.splitContainer2.Panel1.SuspendLayout();
      this.splitContainer2.Panel2.SuspendLayout();
      this.splitContainer2.SuspendLayout();
      this.SuspendLayout();
      // 
      // splitContainer
      // 
      splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
      splitContainer.IsSplitterFixed = true;
      splitContainer.Location = new System.Drawing.Point(0, 0);
      splitContainer.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      splitContainer.Name = "splitContainer";
      splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer.Panel1
      // 
      splitContainer.Panel1.Controls.Add(this.materialSelector_);
      splitContainer.Panel1MinSize = 23;
      // 
      // splitContainer.Panel2
      // 
      splitContainer.Panel2.Controls.Add(this.splitContainer1);
      splitContainer.Size = new System.Drawing.Size(338, 608);
      splitContainer.SplitterDistance = 25;
      splitContainer.SplitterWidth = 5;
      splitContainer.TabIndex = 0;
      // 
      // materialSelector_
      // 
      this.materialSelector_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.materialSelector_.Location = new System.Drawing.Point(0, 0);
      this.materialSelector_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.materialSelector_.Materials = new fin.model.IMaterial[0];
      this.materialSelector_.Name = "materialSelector_";
      this.materialSelector_.SelectedMaterial = null;
      this.materialSelector_.Size = new System.Drawing.Size(338, 25);
      this.materialSelector_.TabIndex = 0;
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.Location = new System.Drawing.Point(0, 0);
      this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.splitContainer1.Name = "splitContainer1";
      this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.materialViewerPanel1);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
      this.splitContainer1.Size = new System.Drawing.Size(338, 578);
      this.splitContainer1.SplitterDistance = 143;
      this.splitContainer1.SplitterWidth = 5;
      this.splitContainer1.TabIndex = 0;
      // 
      // splitContainer2
      // 
      this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer2.Location = new System.Drawing.Point(0, 0);
      this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.splitContainer2.Name = "splitContainer2";
      this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // splitContainer2.Panel1
      // 
      this.splitContainer2.Panel1.Controls.Add(this.textureSection_);
      // 
      // splitContainer2.Panel2
      // 
      this.splitContainer2.Panel2.Controls.Add(this.shaderSection_);
      this.splitContainer2.Size = new System.Drawing.Size(338, 430);
      this.splitContainer2.SplitterDistance = 210;
      this.splitContainer2.SplitterWidth = 5;
      this.splitContainer2.TabIndex = 0;
      // 
      // textureSection_
      // 
      this.textureSection_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textureSection_.Location = new System.Drawing.Point(0, 0);
      this.textureSection_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.textureSection_.Name = "textureSection_";
      this.textureSection_.Size = new System.Drawing.Size(338, 210);
      this.textureSection_.TabIndex = 0;
      // 
      // shaderSection_
      // 
      this.shaderSection_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.shaderSection_.Location = new System.Drawing.Point(0, 0);
      this.shaderSection_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.shaderSection_.Name = "shaderSection_";
      this.shaderSection_.Size = new System.Drawing.Size(338, 215);
      this.shaderSection_.TabIndex = 0;
      // 
      // materialViewerPanel1
      // 
      this.materialViewerPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.materialViewerPanel1.Location = new System.Drawing.Point(0, 0);
      this.materialViewerPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.materialViewerPanel1.Name = "materialViewerPanel1";
      this.materialViewerPanel1.Size = new System.Drawing.Size(338, 143);
      this.materialViewerPanel1.TabIndex = 0;
      // 
      // MaterialsTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(splitContainer);
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "MaterialsTab";
      this.Size = new System.Drawing.Size(338, 608);
      splitContainer.Panel1.ResumeLayout(false);
      splitContainer.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(splitContainer)).EndInit();
      splitContainer.ResumeLayout(false);
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.splitContainer2.Panel1.ResumeLayout(false);
      this.splitContainer2.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
      this.splitContainer2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private MaterialSelector materialSelector_;
    private SplitContainer splitContainer1;
    private SplitContainer splitContainer2;
    private ShaderSection shaderSection_;
    private TextureSection textureSection_;
    private MaterialViewerPanel materialViewerPanel1;
  }
}
