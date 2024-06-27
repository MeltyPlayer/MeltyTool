using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.winforms.common;

public partial class TransparentLabel : Label {
  // hide the BackColor attribute as much as possible.
  // setting the base value has no effect as drawing the
  // background is disabled
  [Browsable(false)]
  [EditorBrowsable(EditorBrowsableState.Never)]
  public override Color BackColor {
    get {
        return Color.Transparent;
      }
    set {
      }
  }

  protected override CreateParams CreateParams {
    get {
        CreateParams cp = base.CreateParams;
        cp.ExStyle |= 0x20; //  WS_EX_TRANSPARENT
        return cp;
      }
  }

  public override string Text {
    get {
        return base.Text;
      }
    set {
        base.Text = value;
        if (Parent != null) Parent.Invalidate(Bounds, false);
      }
  }

  public override ContentAlignment TextAlign {
    get {
        return base.TextAlign;
      }
    set {
        base.TextAlign = value;
        if (Parent != null) Parent.Invalidate(Bounds, false);
      }
  }

  public TransparentLabel() {
      InitializeComponent();

      SetStyle(ControlStyles.Opaque, true);
      SetStyle(ControlStyles.OptimizedDoubleBuffer, false);

      base.BackColor = Color.Transparent;
    }

  protected override void OnMove(EventArgs e) {
      base.OnMove(e);
      //RecreateHandle();
    }

  protected override void OnPaintBackground(PaintEventArgs pevent) {
      // do nothing
    }
}