using System;
using System.Windows.Forms;

using uni.cli;
using uni.ui.winforms;

namespace uni.ui;

public class Program {
  [STAThread]
  public static void Main(string[] args) {
    Cli.Run(args,
            () => {
              DesignModeUtil.InDesignMode = false;
              ApplicationConfiguration.Initialize();
              Application.Run(new UniversalAssetToolForm());
            });
  }
}