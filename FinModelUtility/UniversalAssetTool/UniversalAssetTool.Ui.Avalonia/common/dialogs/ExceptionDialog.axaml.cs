using System;

using Avalonia.Controls;

using ReactiveUI;

using uni.ui.avalonia.common.buttons;
using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.dialogs;

public class ExceptionDialogViewModelForDesigner : ExceptionDialogViewModel {
  public ExceptionDialogViewModelForDesigner() {
    try {
      var array = new int[1];
      array[123] = 123;
    } catch (Exception e) {
      this.Exception = e;
    }
  }
}

public class ExceptionDialogViewModel : ViewModelBase {
  public Exception Exception {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.ReportIssueButton = new ReportIssueButtonViewModel {
          ShowText = true,
          Exception = value
      };
    }
  }

  public ReportIssueButtonViewModel ReportIssueButton {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ExceptionDialog : Window {
  public ExceptionDialog() => this.InitializeComponent();
}