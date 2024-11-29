using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;

using fin.language.equations.fixedFunction;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.resources.registers;

public class RegistersPanelViewModelForDesigner : RegistersPanelViewModel {
  public RegistersPanelViewModelForDesigner() {
    this.Registers = RegistersDesignerUtil.CreateStubRegisters();
  }
}

public class RegistersPanelViewModel : ViewModelBase {
  private IFixedFunctionRegisters registers_;

  public required IFixedFunctionRegisters? Registers {
    get => this.registers_;
    set {
      this.RaiseAndSetIfChanged(ref this.registers_, value);
      this.RegisterCount = this.registers_ != null
          ? this.registers_.ColorRegisters.Count +
            this.registers_.ScalarRegisters.Count
          : 0;
      this.ColorRegisterPickers
          = value?.ColorRegisters
                 .Select(r => new ColorRegisterPickerViewModel
                             { ColorRegister = r })
                 .ToArray();
      this.ScalarRegisterPickers
          = value?.ScalarRegisters
                 .Select(r => new ScalarRegisterPickerViewModel
                             { ScalarRegister = r })
                 .ToArray();
    }
  }

  public int RegisterCount {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IReadOnlyList<ColorRegisterPickerViewModel>? ColorRegisterPickers {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IReadOnlyList<ScalarRegisterPickerViewModel>? ScalarRegisterPickers {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class RegistersPanel : UserControl {
  public RegistersPanel() {
    this.InitializeComponent();
  }
}