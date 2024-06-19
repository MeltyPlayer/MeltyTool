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

  private int registerCount_;
  private IReadOnlyList<ColorRegisterPickerViewModel> colorRegisterPickers_;
  private IReadOnlyList<ScalarRegisterPickerViewModel> scalarRegisterPickers_;

  public required IFixedFunctionRegisters Registers {
    get => this.registers_;
    set {
      this.RaiseAndSetIfChanged(ref this.registers_, value);
      this.RegisterCount = this.registers_.ColorRegisters.Count +
                           this.registers_.ScalarRegisters.Count;
      this.ColorRegisterPickers
          = value.ColorRegisters
                 .Select(r => new ColorRegisterPickerViewModel
                             { ColorRegister = r })
                 .ToArray();
      this.ScalarRegisterPickers
          = value.ScalarRegisters
                 .Select(r => new ScalarRegisterPickerViewModel
                             { ScalarRegister = r })
                 .ToArray();
    }
  }

  public int RegisterCount {
    get => this.registerCount_;
    set => this.RaiseAndSetIfChanged(ref this.registerCount_, value);
  }

  public IReadOnlyList<ColorRegisterPickerViewModel> ColorRegisterPickers {
    get => this.colorRegisterPickers_;
    set => this.RaiseAndSetIfChanged(ref this.colorRegisterPickers_, value);
  }

  public IReadOnlyList<ScalarRegisterPickerViewModel> ScalarRegisterPickers {
    get => this.scalarRegisterPickers_;
    set => this.RaiseAndSetIfChanged(ref this.scalarRegisterPickers_, value);
  }
}

public partial class RegistersPanel : UserControl {
  public RegistersPanel() {
    InitializeComponent();
  }
}