using System.Windows.Forms;

namespace uni.ui.winforms.common;

public class TransparentTrackBar : TrackBar {

  public TransparentTrackBar() {
      this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

  protected override void OnCreateControl() {
      if (this.Parent != null) {
        this.BackColor = this.Parent.BackColor;
      }

      base.OnCreateControl();
    }
}