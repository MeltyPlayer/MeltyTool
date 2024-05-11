using System;

using Avalonia.Controls;

namespace uni.ui.avalonia.common.textboxes {
  public interface ITextBox {
    string? Text { get; set; }

    event EventHandler<TextChangedEventArgs> TextChanged;
  }

  public interface IMultiLineTextBox : ITextBox {
    int CaretIndex { get; set; }
    int MaxLength { get; set; }

    int MaxLines { get; set; }
  }
}