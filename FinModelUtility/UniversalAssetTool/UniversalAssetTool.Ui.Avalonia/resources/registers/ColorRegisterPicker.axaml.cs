using Avalonia.Controls;
using Avalonia.Media;

using fin.color;
using fin.language.equations.fixedFunction;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

using IColorRegister = fin.language.equations.fixedFunction.IColorRegister;

namespace uni.ui.avalonia.resources.registers;

public class ColorRegisterPickerViewModelForDesigner
    : ColorRegisterPickerViewModel {
  public ColorRegisterPickerViewModelForDesigner() {
    this.ColorRegister
        = new FixedFunctionRegisters().GetOrCreateColorRegister(
            "foobar",
            new ColorConstant(.3, .4, .5));
  }
}

public class ColorRegisterPickerViewModel : ViewModelBase {
  private IColorRegister colorRegister_;
  private Color color_;

  public required IColorRegister ColorRegister {
    get => this.colorRegister_;
    set {
      this.RaiseAndSetIfChanged(ref this.colorRegister_, value);
      this.Color = new Color(value.Value.Ab,
                             value.Value.Rb,
                             value.Value.Gb,
                             value.Value.Bb);
    }
  }

  public Color Color {
    get => this.color_;
    set {
      if (this.color_ == value) {
        return;
      }

      this.RaiseAndSetIfChanged(ref this.color_, value);
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