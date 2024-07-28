using System;
using System.Drawing;
using System.Windows.Forms;

namespace uni.ui.winforms.common;

// From https://www.codeproject.com/Articles/27849/WaterMark-TextBox-For-Desktop-Applications-Using-C.
public class WaterMarkTextBox : TextBox {
  private Font? oldFont = null;
  private Boolean waterMarkTextEnabled = false;

  #region Attributes

  private Color _waterMarkColor = Color.Gray;

  public Color WaterMarkColor {
    get { return this._waterMarkColor; }
    set {
      this._waterMarkColor = value;
      this.Invalidate(); /*thanks to Bernhard Elbl for Invalidate()*/
    }
  }

  private string _waterMarkText = "Water Mark";

  public string WaterMarkText {
    get { return this._waterMarkText; }
    set {
      this._waterMarkText = value;
      this.Invalidate();
    }
  }

  #endregion

  //Default constructor
  public WaterMarkTextBox() {
    this.JoinEvents(true);
  }

  //Override OnCreateControl ... thanks to  "lpgray .. codeproject guy"
  protected override void OnCreateControl() {
    base.OnCreateControl();
    this.WaterMark_Toggle(null, null);
  }

  //Override OnPaint
  protected override void OnPaint(PaintEventArgs args) {
    // Use the same font that was defined in base class
    Font drawFont = new Font(this.Font.FontFamily,
                             this.Font.Size,
                             this.Font.Style,
                             this.Font.Unit);
    //Create new brush with gray color or 
    SolidBrush
        drawBrush = new SolidBrush(this.WaterMarkColor); //use Water mark color
    //Draw Text or WaterMark
    args.Graphics.DrawString((this.waterMarkTextEnabled ? this.WaterMarkText : this.Text),
                             drawFont,
                             drawBrush,
                             new PointF(0.0F, 0.0F));
    base.OnPaint(args);
  }

  private void JoinEvents(Boolean join) {
    if (join) {
      this.TextChanged += new EventHandler(this.WaterMark_Toggle);
      this.LostFocus += new EventHandler(this.WaterMark_Toggle);
      this.FontChanged += new EventHandler(this.WaterMark_FontChanged);
      //No one of the above events will start immeddiatlly 
      //TextBox control still in constructing, so,
      //Font object (for example) couldn't be catched from within
      //WaterMark_Toggle
      //So, call WaterMark_Toggel through OnCreateControl after TextBox
      //is totally created
      //No doupt, it will be only one time call

      //Old solution uses Timer.Tick event to check Create property
    }
  }

  private void WaterMark_Toggle(object? sender, EventArgs? args) {
    if (this.Text.Length <= 0)
      this.EnableWaterMark();
    else
      this.DisbaleWaterMark();
  }

  private void EnableWaterMark() {
    //Save current font until returning the UserPaint style to false (NOTE:
    //It is a try and error advice)
    this.oldFont = new Font(this.Font.FontFamily,
                            this.Font.Size,
                            this.Font.Style,
                            this.Font.Unit);
    //Enable OnPaint event handler
    this.SetStyle(ControlStyles.UserPaint, true);
    this.waterMarkTextEnabled = true;
    //Triger OnPaint immediatly
    this.Refresh();
  }

  private void DisbaleWaterMark() {
    //Disbale OnPaint event handler
    this.waterMarkTextEnabled = false;
    this.SetStyle(ControlStyles.UserPaint, false);
    //Return back oldFont if existed
    if (this.oldFont != null)
      this.Font = new Font(this.oldFont.FontFamily,
                           this.oldFont.Size,
                           this.oldFont.Style,
                           this.oldFont.Unit);
  }

  private void WaterMark_FontChanged(object sender, EventArgs args) {
    if (this.waterMarkTextEnabled) {
      this.oldFont = new Font(this.Font.FontFamily,
                              this.Font.Size,
                              this.Font.Style,
                              this.Font.Unit);
      this.Refresh();
    }
  }
}