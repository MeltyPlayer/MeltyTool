using Avalonia.Controls;

using fin.language.equations.fixedFunction;
using fin.math.floats;
using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.resources.registers;

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
  public required IScalarRegister ScalarRegister {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.Value = field.Value;
    }
  }

  public float Value {
    get;
    set {
      if (field.IsRoughly(value)) {
        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);
      this.ScalarRegister.Value = value;
    }
  }
}

public partial class ScalarRegisterPicker : UserControl {
  public ScalarRegisterPicker() {
    this.InitializeComponent();
  }
}