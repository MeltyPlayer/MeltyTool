using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.io.web;
using fin.util.asserts;
using fin.util.io;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

namespace uni.ui.avalonia.common.buttons;

public class ReportIssueButtonViewModel : ViewModelBase {
  private Exception? exception_;

  public Exception? Exception {
    get => this.exception_;
    set => this.RaiseAndSetIfChanged(ref this.exception_, value);
  }
}

public partial class ReportIssueButton : UserControl {
  public ReportIssueButton() => this.InitializeComponent();

  private ReportIssueButtonViewModel? ViewModel
    => this.DataContext as ReportIssueButtonViewModel;

  private void Button_OnClick(object? sender, RoutedEventArgs e)
    => WebBrowserUtil.OpenUrl(
        GitHubUtil.GetNewIssueUrl(this.ViewModel?.Exception));
}