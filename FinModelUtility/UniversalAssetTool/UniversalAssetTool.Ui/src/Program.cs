using System;
using System.Windows.Forms;

using fin.exporter.assimp;
using fin.io;
using fin.model.io.exporters;
using fin.ui;
using fin.ui.rendering.gl;

using pikmin1.api;

using uni.api;
using uni.cli;
using uni.ui.winforms;

namespace uni.ui;

public sealed class Program {
  [STAThread]
  public static void Main(string[] args) {
    UiUtil.Initialize();
    OpenGlVersionService.Init(false);

    Cli.Run(args,
            () => {
              DesignModeUtil.InDesignMode = false;
              ApplicationConfiguration.Initialize();
              Application.Run(new UniversalAssetToolForm());
            },
            () => { });
  }
}