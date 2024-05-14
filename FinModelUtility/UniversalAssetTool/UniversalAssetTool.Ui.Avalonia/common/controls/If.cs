using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;

namespace uni.ui.avalonia.common.controls;

public class If : Control {
  private object deferredContent_;
  private Control control_;

  public static readonly StyledProperty<bool> ConditionProperty
      = AvaloniaProperty.Register<If, bool>(
          "Condition");

  public bool Condition {
    get => this.GetValue(ConditionProperty);
    set => this.SetValue(ConditionProperty, value);
  }

  [Content, TemplateContent]
  public object DeferredContent {
    get => this.deferredContent_;
    set {
      this.deferredContent_ = value;
      if (this.Condition) {
        this.DoLoad_(true);
      }
    }
  }

  public Control Control => this.control_;

  static If() {
    ConditionProperty.Changed.AddClassHandler<If>(
        (c, e) => {
          if (e.NewValue is bool v) {
            c.DoLoad_(v);
          }
        });
  }

  protected override Size MeasureOverride(Size availableSize)
    => LayoutHelper.MeasureChild(this.control_, availableSize, default);

  protected override Size ArrangeOverride(Size finalSize)
    => LayoutHelper.ArrangeChild(this.control_, finalSize, default);


  private void DoLoad_(bool load) {
    if ((this.control_ != null) == load)
      return;

    if (load) {
      this.control_ = TemplateContent.Load(this.DeferredContent).Result;
      ((ISetLogicalParent) this.control_).SetParent(this);
      this.VisualChildren.Add(this.control_);
      this.LogicalChildren.Add(this.control_);
    } else {
      ((ISetLogicalParent) this.control_).SetParent(null);
      this.LogicalChildren.Clear();
      this.VisualChildren.Remove(this.control_);
      this.control_ = null;
    }

    this.InvalidateMeasure();
  }
}