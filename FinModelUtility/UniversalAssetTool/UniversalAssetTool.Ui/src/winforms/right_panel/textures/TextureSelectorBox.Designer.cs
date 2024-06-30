using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.textures {
  partial class TextureSelectorBox {
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
      this.listView_ = new System.Windows.Forms.ListView();
      this.SuspendLayout();
      // 
      // listView_
      // 
      this.listView_.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listView_.Location = new System.Drawing.Point(0, 0);
      this.listView_.MultiSelect = false;
      this.listView_.Name = "listView_";
      this.listView_.Size = new System.Drawing.Size(266, 282);
      this.listView_.TabIndex = 2;
      this.listView_.UseCompatibleStateImageBehavior = false;
      this.listView_.View = System.Windows.Forms.View.List;
      // 
      // TextureSelectorBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.listView_);
      this.Name = "TextureSelectorBox";
      this.Size = new System.Drawing.Size(266, 282);
      this.ResumeLayout(false);

    }

    #endregion

    private ListView listView_;
  }
}
