using fin.common;
using fin.io;
using fin.model;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;

using IronPython.Hosting;
using IronPython.Runtime;

using ModelPluginWrappers.noesis;
using ModelPluginWrappers.noesis.rapi;


namespace ModelPluginWrappers;

public static class NoesisProgram {
  public static void Main() {
    var engine = Python.CreateEngine();

    var modelPluginWrappersDirectory =
        DirectoryConstants
            .BASE_DIRECTORY
            .AssertGetParent()
            .AssertGetExistingSubdir("FinModelUtility/ModelPluginWrappers");

    var libDirectory =
        modelPluginWrappersDirectory.AssertGetExistingSubdir("lib");
    var modelsDirectory =
        modelPluginWrappersDirectory.AssertGetExistingSubdir("models");
    var noesisDirectory =
        modelPluginWrappersDirectory.AssertGetExistingSubdir("noesis");

    var scope = engine.CreateScope();

    // Hooks up common Python imports
    engine.SetSearchPaths([
        libDirectory.AssertGetExistingSubdir("3.4").FullPath,
        libDirectory.AssertGetExistingSubdir("noesis").FullPath,
        noesisDirectory.FullPath,
    ]);

    // Hooks up missing Python imports
    { }

    // Hooks up Noesis imports
    engine.CreateModule("inc_noesis").AddStaticMembers<IncNoesis>();
    engine.CreateModule("noesis").AddStaticMembers<Noesis>();
    engine.CreateModule("rapi").AddInstanceMembers(new Rapi());

    var name = "midnight_club_2";

    engine.Execute($@"
import {name}

{name}.registerNoesisTypes()
",
                   scope);

    var midnightClub2Handle = Noesis.HandlesByExtension[".xmod"];

    var models = new PythonList();

    {
      var bytes = modelsDirectory
                  .AssertGetExistingFile(
                      "midnight_club_2/vp_supraa_body_ui_h.xmod")
                  .ReadAllBytes();
      midnightClub2Handle.loadModel(bytes, models);
    }

    foreach (var model in models) {
      new AssimpIndirectModelExporter().ExportModel(
          new ModelExporterParams {
              OutputFile = new FinFile(
                  Path.Join(modelPluginWrappersDirectory.FullPath,
                            "test.fbx")),
              Model = model as IModel,
          });
    }
  }
}