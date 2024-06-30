using System.Windows.Forms;

namespace uni.ui.winforms.common;

public partial class LabelledProgressBar : UserControl {
  public LabelledProgressBar() {
      InitializeComponent();
    }

  public string Text {
    get => this.label_.Text;
    set => this.label_.Text = value;
  }

  public int Value {
    get => this.progressBar_.Value;
    set => this.progressBar_.Value = value;
  }
}