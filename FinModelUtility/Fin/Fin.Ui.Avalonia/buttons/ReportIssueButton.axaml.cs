using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.web;
using fin.util.io;

using ReactiveUI;

namespace fin.ui.avalonia.buttons;

public class ReportIssueButtonViewModel : ViewModelBase {
  public bool ShowText {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public Exception? Exception {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public IExceptionContext? Context {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ReportIssueButton : UserControl {
  public ReportIssueButton() => this.InitializeComponent();

  private ReportIssueButtonViewModel? ViewModel
    => this.DataContext as ReportIssueButtonViewModel;

  private void Button_OnClick(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenUrl(
        GitHubUtil.GetNewIssueUrl(this.ViewModel?.Exception,
                                  this.ViewModel?.Context));
}