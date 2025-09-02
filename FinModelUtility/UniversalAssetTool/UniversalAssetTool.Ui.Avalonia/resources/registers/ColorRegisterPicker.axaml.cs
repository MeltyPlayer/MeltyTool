using Avalonia.Controls;
using Avalonia.Media;

using fin.color;
using fin.language.equations.fixedFunction;
using fin.ui.avalonia;

using ReactiveUI;

using IColorRegister = fin.language.equations.fixedFunction.IColorRegister;

namespace uni.ui.avalonia.resources.registers;

public class ColorRegisterPickerViewModelForDesigner
    : ColorRegisterPickerViewModel {
  public ColorRegisterPickerViewModelForDesigner() {
    this.ColorRegister
        = new FixedFunctionRegisters().GetOrCreateColorRegister(
            "foobar",
            new ColorConstant(.3f, .4f, .5f));
  }
}

public class ColorRegisterPickerViewModel : ViewModelBase {
  public required IColorRegister ColorRegister {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.Color = new Color(value.Value.Ab,
                             value.Value.Rb,
                             value.Value.Gb,
                             value.Value.Bb);
    }
  }

  public Color Color {
    get;
    set {
      if (field == value) {
        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);
      this.ColorRegister.Value
          = FinColor.FromRgbaBytes(value.R, value.G, value.B, value.A);
    }
  }
}

public partial class ColorRegisterPicker : UserControl {
  public ColorRegisterPicker() {
    this.InitializeComponent();
  }
}