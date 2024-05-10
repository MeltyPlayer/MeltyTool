using System.Windows.Controls;

namespace uni.ui.wpf.common.textbox {
  public interface ITextBox {
    string Text { get; set; }
    event TextChangedEventHandler TextChanged;

    int CaretIndex { get; set; }
    int MaxLength { get; set; }

    int MaxLines { get; set; }

    CharacterCasing CharacterCasing { get; set; }
  }
}