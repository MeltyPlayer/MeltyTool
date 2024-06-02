using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

using ReactiveUI;

using uni.ui.avalonia.model.materials;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common {
  public class KeyValueGridViewModelForDesigner
      : KeyValueGridViewModel {
    public KeyValueGridViewModelForDesigner() {
      this.KeyValuePairs = [
          ("Foo", "Bar"),
          ("Number", 123),
          ("Null", null),
          ("Long wrappable text",
           "This is very long text that could theoretically wrap at any of the spaces."),
          ("Long unwrappable text",
           "Thisisverylongtextthatcannotwrapbecausetherearenospaces."),
          ("Newline text",
           "This text\nhas some newlines\nthat should be shown."),
      ];
    }
  }

  public class KeyValueGridViewModel : ViewModelBase {
    private ObservableCollection<KeyValuePairViewModel> keyValuePairs_ = [];

    public ObservableCollection<KeyValuePairViewModel> KeyValuePairs {
      get => this.keyValuePairs_;
      set => this.RaiseAndSetIfChanged(ref this.keyValuePairs_, value);
    }
  }

  public class KeyValuePairViewModel(string key, string? value)
      : ViewModelBase {
    private static Cursor copyCursor_ = new(StandardCursorType.DragCopy);

    public string Key => key;
    public string? Value => value;

    public static implicit operator KeyValuePairViewModel(
        (string key, object? value) tuple)
      => new(tuple.key, tuple.value?.ToString());

    public Cursor Cursor => copyCursor_;
  }

  public partial class KeyValueGrid : UserControl {
    public KeyValueGrid() {
      InitializeComponent();
    }
  }
}