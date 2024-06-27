using System;
using System.Windows.Forms;

namespace uni.ui.winforms.common;

public partial class CancellableProgressBar : UserControl {
  public CancellableProgressBar() {
      InitializeComponent();
    }

  public string Text {
    get => this.labelledProgressBar_.Text;
    set => this.labelledProgressBar_.Text = value;
  }

  public int Value {
    get => this.labelledProgressBar_.Value;
    set {
        this.labelledProgressBar_.Value = value;
        this.cancelButton_.Enabled = value < 100;
      }
  }

  public event EventHandler Clicked {
    add => this.cancelButton_.Click += value;
    remove => this.cancelButton_.Click -= value;
  }
}