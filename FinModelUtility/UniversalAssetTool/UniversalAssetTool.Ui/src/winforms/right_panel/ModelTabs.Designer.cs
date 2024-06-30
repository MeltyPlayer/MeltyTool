using System.Windows.Forms;

using uni.ui.winforms.right_panel.info;
using uni.ui.winforms.right_panel.materials;
using uni.ui.winforms.right_panel.registers;
using uni.ui.winforms.right_panel.skeleton;
using uni.ui.winforms.right_panel.textures;

namespace uni.ui.winforms.right_panel {
  partial class ModelTabs {
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
      System.Windows.Forms.TabControl tabControl;
      System.Windows.Forms.TabPage infoTabPage;
      System.Windows.Forms.TabPage skeletonTabPage;
      System.Windows.Forms.TabPage animationsTabPage;
      System.Windows.Forms.TabPage texturesTabPage;
      System.Windows.Forms.TabPage materialsTabPage;
      this.skeletonTab_ = new SkeletonTab();
      this.animationsTab_ = new AnimationsTab();
      this.texturesTab_ = new TexturesTab();
      this.materialsTab_ = new MaterialsTab();
      this.registersTabPage = new System.Windows.Forms.TabPage();
      this.infoTab_ = new InfoTab();
      this.registersTab_ = new RegistersTab();
      tabControl = new System.Windows.Forms.TabControl();
      infoTabPage = new System.Windows.Forms.TabPage();
      skeletonTabPage = new System.Windows.Forms.TabPage();
      animationsTabPage = new System.Windows.Forms.TabPage();
      texturesTabPage = new System.Windows.Forms.TabPage();
      materialsTabPage = new System.Windows.Forms.TabPage();
      tabControl.SuspendLayout();
      infoTabPage.SuspendLayout();
      skeletonTabPage.SuspendLayout();
      animationsTabPage.SuspendLayout();
      texturesTabPage.SuspendLayout();
      materialsTabPage.SuspendLayout();
      this.registersTabPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // tabControl
      // 
      tabControl.Controls.Add(infoTabPage);
      tabControl.Controls.Add(skeletonTabPage);
      tabControl.Controls.Add(animationsTabPage);
      tabControl.Controls.Add(texturesTabPage);
      tabControl.Controls.Add(materialsTabPage);
      tabControl.Controls.Add(this.registersTabPage);
      tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      tabControl.Location = new System.Drawing.Point(0, 0);
      tabControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      tabControl.Multiline = true;
      tabControl.Name = "tabControl";
      tabControl.SelectedIndex = 0;
      tabControl.Size = new System.Drawing.Size(247, 580);
      tabControl.TabIndex = 0;
      // 
      // infoTabPage
      // 
      infoTabPage.Controls.Add(this.infoTab_);
      infoTabPage.Location = new System.Drawing.Point(4, 54);
      infoTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      infoTabPage.Name = "infoTabPage";
      infoTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
      infoTabPage.Size = new System.Drawing.Size(239, 522);
      infoTabPage.TabIndex = 0;
      infoTabPage.Text = "Info";
      infoTabPage.UseVisualStyleBackColor = true;
      // 
      // skeletonTabPage
      // 
      skeletonTabPage.Controls.Add(this.skeletonTab_);
      skeletonTabPage.Location = new System.Drawing.Point(4, 54);
      skeletonTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      skeletonTabPage.Name = "skeletonTabPage";
      skeletonTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
      skeletonTabPage.Size = new System.Drawing.Size(239, 522);
      skeletonTabPage.TabIndex = 4;
      skeletonTabPage.Text = "Skeleton";
      skeletonTabPage.UseVisualStyleBackColor = true;
      // 
      // skeletonTab_
      // 
      this.skeletonTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.skeletonTab_.Location = new System.Drawing.Point(3, 4);
      this.skeletonTab_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.skeletonTab_.Name = "skeletonTab_";
      this.skeletonTab_.Size = new System.Drawing.Size(233, 514);
      this.skeletonTab_.TabIndex = 1;
      // 
      // animationsTabPage
      // 
      animationsTabPage.Controls.Add(this.animationsTab_);
      animationsTabPage.Location = new System.Drawing.Point(4, 54);
      animationsTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      animationsTabPage.Name = "animationsTabPage";
      animationsTabPage.Size = new System.Drawing.Size(239, 522);
      animationsTabPage.TabIndex = 2;
      animationsTabPage.Text = "Animations";
      animationsTabPage.UseVisualStyleBackColor = true;
      // 
      // animationsTab_
      // 
      this.animationsTab_.AnimationPlaybackManager = null;
      this.animationsTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.animationsTab_.Location = new System.Drawing.Point(0, 0);
      this.animationsTab_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.animationsTab_.Name = "animationsTab_";
      this.animationsTab_.Size = new System.Drawing.Size(239, 522);
      this.animationsTab_.TabIndex = 0;
      // 
      // texturesTabPage
      // 
      texturesTabPage.Controls.Add(this.texturesTab_);
      texturesTabPage.Location = new System.Drawing.Point(4, 54);
      texturesTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      texturesTabPage.Name = "texturesTabPage";
      texturesTabPage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
      texturesTabPage.Size = new System.Drawing.Size(239, 522);
      texturesTabPage.TabIndex = 1;
      texturesTabPage.Text = "Textures";
      texturesTabPage.UseVisualStyleBackColor = true;
      // 
      // texturesTab_
      // 
      this.texturesTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.texturesTab_.Location = new System.Drawing.Point(3, 4);
      this.texturesTab_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.texturesTab_.Name = "texturesTab_";
      this.texturesTab_.Size = new System.Drawing.Size(233, 514);
      this.texturesTab_.TabIndex = 0;
      // 
      // materialsTabPage
      // 
      materialsTabPage.Controls.Add(this.materialsTab_);
      materialsTabPage.Location = new System.Drawing.Point(4, 54);
      materialsTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      materialsTabPage.Name = "materialsTabPage";
      materialsTabPage.Size = new System.Drawing.Size(239, 522);
      materialsTabPage.TabIndex = 3;
      materialsTabPage.Text = "Materials";
      materialsTabPage.UseVisualStyleBackColor = true;
      // 
      // materialsTab_
      // 
      this.materialsTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.materialsTab_.Location = new System.Drawing.Point(0, 0);
      this.materialsTab_.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
      this.materialsTab_.Name = "materialsTab_";
      this.materialsTab_.Size = new System.Drawing.Size(239, 522);
      this.materialsTab_.TabIndex = 0;
      // 
      // registersTabPage
      // 
      this.registersTabPage.Controls.Add(this.registersTab_);
      this.registersTabPage.Location = new System.Drawing.Point(4, 54);
      this.registersTabPage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.registersTabPage.Name = "registersTabPage";
      this.registersTabPage.Size = new System.Drawing.Size(239, 522);
      this.registersTabPage.TabIndex = 5;
      this.registersTabPage.Text = "Registers";
      this.registersTabPage.UseVisualStyleBackColor = true;
      // 
      // infoTab_
      // 
      this.infoTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.infoTab_.Location = new System.Drawing.Point(3, 4);
      this.infoTab_.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.infoTab_.Name = "infoTab_";
      this.infoTab_.Size = new System.Drawing.Size(233, 514);
      this.infoTab_.TabIndex = 0;
      // 
      // registersTab_
      // 
      this.registersTab_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.registersTab_.Location = new System.Drawing.Point(0, 0);
      this.registersTab_.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.registersTab_.Name = "registersTab_";
      this.registersTab_.Size = new System.Drawing.Size(239, 522);
      this.registersTab_.TabIndex = 0;
      // 
      // ModelTabs
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(tabControl);
      this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.Name = "ModelTabs";
      this.Size = new System.Drawing.Size(247, 580);
      tabControl.ResumeLayout(false);
      infoTabPage.ResumeLayout(false);
      skeletonTabPage.ResumeLayout(false);
      animationsTabPage.ResumeLayout(false);
      texturesTabPage.ResumeLayout(false);
      materialsTabPage.ResumeLayout(false);
      this.registersTabPage.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private TabControl tabControl;
    private AnimationsTab animationsTab_;
    private textures.TexturesTab texturesTab_;
    private materials.MaterialsTab materialsTab_;
    private skeleton.SkeletonTab skeletonTab_;
    private TabPage registersTabPage;
    private info.InfoTab infoTab_;
    private registers.RegistersTab registersTab_;
  }
}
