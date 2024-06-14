using Avalonia.Controls;

using fin.language.equations.fixedFunction;
using fin.math.floats;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.registers;

public class ScalarRegisterPickerViewModelForDesigner
    : ScalarRegisterPickerViewModel {
  public ScalarRegisterPickerViewModelForDesigner() {
    this.ScalarRegister
        = new FixedFunctionRegisters().GetOrCreateScalarRegister(
            "foobar",
            new ScalarConstant(.3));
  }
}

public class ScalarRegisterPickerViewModel : ViewModelBase {
  private IScalarRegister scalarRegister_;
  private float value_;

  public required IScalarRegister ScalarRegister {
    get => this.scalarRegister_;
    set {
      this.RaiseAndSetIfChanged(ref this.scalarRegister_, value);
      this.Value = this.scalarRegister_.Value;
    }
  }

  public float Value {
    get => this.value_;
    set {
      if (this.value_.IsRoughly(value)) {
        return;
      }

      this.RaiseAndSetIfChanged(ref this.value_, value);
      this.ScalarRegister.Value = value;
    }
  }
}

public partial class ScalarRegisterPicker : UserControl {
  public ScalarRegisterPicker() {
    InitializeComponent();
  }
}