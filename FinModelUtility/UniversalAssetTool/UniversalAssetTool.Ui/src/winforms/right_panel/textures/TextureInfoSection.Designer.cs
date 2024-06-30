using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.textures {
  partial class TextureInfoSection {
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
      this.propertyGrid_ = new System.Windows.Forms.PropertyGrid();
      this.SuspendLayout();
      // 
      // propertyGrid_
      // 
      this.propertyGrid_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.propertyGrid_.HelpVisible = false;
      this.propertyGrid_.Location = new System.Drawing.Point(0, 0);
      this.propertyGrid_.Name = "propertyGrid_";
      this.propertyGrid_.PropertySort = System.Windows.Forms.PropertySort.Categorized;
      this.propertyGrid_.Size = new System.Drawing.Size(266, 282);
      this.propertyGrid_.TabIndex = 0;
      this.propertyGrid_.ToolbarVisible = false;
      // 
      // TextureInfoSection
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.propertyGrid_);
      this.Name = "TextureInfoSection";
      this.Size = new System.Drawing.Size(266, 282);
      this.ResumeLayout(false);

    }

    #endregion

    private PropertyGrid propertyGrid_;
  }
}
