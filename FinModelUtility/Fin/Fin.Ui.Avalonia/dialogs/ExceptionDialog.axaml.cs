using System;

using Avalonia.Controls;

using fin.io.web;
using fin.ui.avalonia.buttons;

using ReactiveUI;

namespace fin.ui.avalonia.dialogs;

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
      this.UpdateButton_();
    }
  }

  public IExceptionContext? Context {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value); 
      this.UpdateButton_();
    }
  }

  private void UpdateButton_() {
    if (Exception == null) {
      return;
    }

    this.ReportIssueButton = new ReportIssueButtonViewModel {
        ShowText = true,
        Exception = Exception,
        Context = Context
    };
  }


  public ReportIssueButtonViewModel ReportIssueButton {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ExceptionDialog : Window {
  public ExceptionDialog() => this.InitializeComponent();
}