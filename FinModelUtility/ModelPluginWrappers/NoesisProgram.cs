using fin.common;
using fin.io;
using fin.model;
using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;

using IronPython.Hosting;
using IronPython.Runtime;

using Microsoft.Scripting.Hosting;

using ModelPluginWrappers.noesis.rapi;
using ModelPluginWrappers.src.noesis;


namespace ModelPluginWrappers;

public enum NoeFormat {
  RPGEODATA_FLOAT,
  RPGEODATA_USHORT,
}

public enum NoePrimitiveType {
  RPGEO_POINTS,
}

public static class NoesisProgram {
  public record Handle(string FormatName, string Extension) {
    public Func<byte[], bool> checkType;
    public Func<byte[], PythonList, bool> loadModel;
  }

  enum PixelType {
    NOESISTEX_RGBA32
  }

  enum InterpolationType {
    NOEKF_INTERPOLATE_LINEAR,
  }

  enum KeyframeType {
    NOEKF_ROTATION_QUATERNION_4,
    NOEKF_TRANSLATION_VECTOR_3,
    NOEKF_SCALE_SCALAR_1,
  }

  public static INoeBitStream NoeBitStream(byte[]? data = null) {
    return new NoeBitStreamReader(data ?? []);
  }

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

    var handlesByExtension = new Dictionary<string, Handle>();

    // Hooks up Noesis imports
    {
      {
        var noesisModule = engine.CreateModule("noesis");
        noesisModule.SetVariable("logPopup", () => { });
        noesisModule.SetVariable("register",
                                 (string formatName, string extension)
                                     => handlesByExtension[extension] =
                                         new Handle(formatName, extension));
        noesisModule.SetVariable("setHandlerTypeCheck",
                                 (Handle handle, Func<byte[], bool> checkType)
                                     => {
                                   handle.checkType = checkType;
                                 });
        noesisModule.SetVariable("setHandlerLoadModel",
                                 (Handle handle,
                                  Func<byte[], PythonList, bool> loadModel) => {
                                   handle.loadModel = loadModel;
                                 });
        noesisModule.SetVariable("vec3Validate", (dynamic _) => { });
        noesisModule.SetVariable("vec4Validate", (dynamic _) => { });
        noesisModule.PushEnumIntoScope<PixelType>();
        noesisModule.PushEnumIntoScope<InterpolationType>();
        noesisModule.PushEnumIntoScope<KeyframeType>();
        noesisModule.PushEnumIntoScope<NoeFormat>();
        noesisModule.PushEnumIntoScope<NoePrimitiveType>();
      }

      engine.CreateModule("rapi").AddClassMembers(new Rapi());

      {
        var incNoesisModule = engine.ImportModule("inc_noesis");
        incNoesisModule.SetVariable("NoeBitStream", NoeBitStream);
      }
    }

    var name = "midnight_club_2";

    engine.Execute($@"
import {name}

{name}.registerNoesisTypes()
",
                   scope);

    var midnightClub2Handle = handlesByExtension[".xmod"];

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