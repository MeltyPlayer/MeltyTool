using System;
using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.winforms.common;

public partial class ColorPicker : UserControl {
  public ColorPicker() {
    this.InitializeComponent();

      this.Value = Color.White;

      this.Click += (_, _) => this.ShowColorPickerDialog_();
    }

  public Color Value {
    get => this.BackColor;
    set {
        if (this.BackColor == value) {
          return;
        }

        this.BackColor = value;
        this.ValueChanged.Invoke(value);
      }
  }

  public event Action<Color> ValueChanged = delegate { };

  private void ShowColorPickerDialog_() {
      var colorDialog = new ColorDialog {
          Color = this.Value, SolidColorOnly = true,
      };

      var result = colorDialog.ShowDialog();
      if (result == DialogResult.OK) {
        this.Value = colorDialog.Color;
      }
    }
}