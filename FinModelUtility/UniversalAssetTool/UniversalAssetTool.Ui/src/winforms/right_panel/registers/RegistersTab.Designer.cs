using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.winforms.right_panel.registers {
  partial class RegistersTab {
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
      this.flowLayoutPanel_ = new FlowLayoutPanel();
      this.SuspendLayout();
      // 
      // flowLayoutPanel_
      // 
      this.flowLayoutPanel_.AutoScroll = true;
      this.flowLayoutPanel_.AutoSize = true;
      this.flowLayoutPanel_.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      this.flowLayoutPanel_.Dock = DockStyle.Fill;
      this.flowLayoutPanel_.FlowDirection = FlowDirection.TopDown;
      this.flowLayoutPanel_.Location = new Point(0, 0);
      this.flowLayoutPanel_.Name = "flowLayoutPanel_";
      this.flowLayoutPanel_.Size = new Size(280, 451);
      this.flowLayoutPanel_.TabIndex = 0;
      // 
      // RegistersTab
      // 
      this.AutoScaleDimensions = new SizeF(8F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.Controls.Add(this.flowLayoutPanel_);
      this.Margin = new Padding(3, 4, 3, 4);
      this.Name = "RegistersTab";
      this.Size = new Size(280, 451);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    #endregion

    private FlowLayoutPanel flowLayoutPanel_;
  }
}
