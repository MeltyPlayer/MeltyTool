using Avalonia.Headless;

using uni.ui.avalonia;

[assembly: Parallelizable(ParallelScope.All)]
[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerAssembly)]