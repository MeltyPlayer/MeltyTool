using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;

namespace fin.ui.avalonia.controls;

public class If : Control {
  private object contentWhenTrue_;
  private object? contentWhenFalse_;
  private bool? renderedValue_;

  private Control? control_;

  public static readonly StyledProperty<bool> ConditionProperty
      = AvaloniaProperty.Register<If, bool>(nameof(Condition));

  public bool Condition {
    get => this.GetValue(ConditionProperty);
    set => this.SetValue(ConditionProperty, value);
  }

  [Content, TemplateContent]
  public object ContentWhenTrue {
    get => this.contentWhenTrue_;
    set {
      this.contentWhenTrue_ = value;
      if (this.Condition) {
        this.DoLoad_(true, true);
      }
    }
  }

  [TemplateContent]
  public object? ContentWhenFalse {
    get => this.contentWhenFalse_;
    set {
      this.contentWhenFalse_ = value;
      if (!this.Condition) {
        this.DoLoad_(false, true);
      }
    }
  }

  public Control? Control => this.control_;

  static If() {
    ConditionProperty.Changed.AddClassHandler<If>((c, e) => {
      if (e.NewValue is bool v) {
        c.DoLoad_(v, false);
      }
    });
  }

  protected override Size MeasureOverride(Size availableSize)
    => LayoutHelper.MeasureChild(this.control_, availableSize, default);

  protected override Size ArrangeOverride(Size finalSize)
    => LayoutHelper.ArrangeChild(this.control_, finalSize, default);


  private void DoLoad_(bool value, bool force) {
    if (this.renderedValue_ == value && !force) {
      return;
    }

    this.renderedValue_ = value;

    if (this.control_ != null) {
      ((ISetLogicalParent) this.control_).SetParent(null);
      this.LogicalChildren.Clear();
      this.VisualChildren.Remove(this.control_);
      this.control_ = null;
    }

    object? newContent = value ? this.contentWhenTrue_ : this.contentWhenFalse_;
    if (newContent != null) {
      this.control_ = TemplateContent.Load(newContent)?.Result;
    }

    if (this.control_ != null) {
      ((ISetLogicalParent) this.control_).SetParent(this);
      this.VisualChildren.Add(this.control_);
      this.LogicalChildren.Add(this.control_);
    }

    this.InvalidateMeasure();
  }
}