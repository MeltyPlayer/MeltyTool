using Avalonia;
using Avalonia.Controls;

using AvaloniaEdit.Document;
using AvaloniaEdit.TextMate;

using uni.ui.avalonia.resources.model.materials;

namespace uni.ui.avalonia.common.controls;

public enum CodeType {
  UNDEFINED,
  GLSL,
}

public partial class CodeViewer : UserControl {
  public CodeViewer() {
    this.InitializeComponent();
    this.impl_.Options = new() {
        AllowScrollBelowDocument = false
    };

    this.InstallRegistryOptionsForViewer_();
  }

  private void InstallRegistryOptionsForViewer_() {
    switch (this.CodeType) {
      case CodeType.GLSL: {
        var registryOptions = new GlslRegistryOptions();
        var textMateInstallation
            = this.impl_.InstallTextMate(registryOptions, false);
        textMateInstallation.SetGrammar("source.glsl");
        break;
      }
    }
  }

  public static readonly StyledProperty<CodeType> CodeTypeProperty =
      AvaloniaProperty.Register<CodeViewer, CodeType>(
          nameof(CodeType));

  public CodeType CodeType {
    get => this.GetValue(CodeTypeProperty);
    set => this.SetValue(CodeTypeProperty, value);
  }

  public static readonly StyledProperty<object?> SourceProperty =
      AvaloniaProperty.Register<CodeViewer, object?>(
          nameof(Source),
          coerce: CoerceSource_);

  public object? Source {
    get => this.GetValue(SourceProperty);
    set => this.SetValue(SourceProperty, value);
  }

  private static TextDocument CoerceSource_(AvaloniaObject _, object? value)
    => value switch {
        string stringValue        => new TextDocument(stringValue),
        TextDocument textDocument => textDocument,
        { } objectValue           => new TextDocument(objectValue.ToString()),
        null                      => new TextDocument()
    };
}