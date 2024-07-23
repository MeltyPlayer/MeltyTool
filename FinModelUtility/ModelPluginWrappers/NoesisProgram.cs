using fin.model.io.exporters;
using fin.model.io.exporters.assimp.indirect;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.util;

using IronPython.Hosting;
using IronPython.Runtime;

using Microsoft.Scripting.Hosting;

using ModelPluginWrappers.src.noesis;

using schema.binary;

namespace ModelPluginWrappers {
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

    public class Rpg {
      private readonly ModelImpl model_ = ModelImpl.CreateForViewer();

      private string name_;

      private Bytes positionBuffer_;
      private NoeFormat positionFormat_;
      private int positionStride_;
      private int positionOffset_;

      public void SetName(string name) {
        this.name_ = name;
      }

      public void BindPositionBufferOffset(Bytes data, NoeFormat format, int stride, int offset) {
        this.positionBuffer_ = data;
        this.positionFormat_ = format;
        this.positionStride_ = stride;
        this.positionOffset_ = offset;
      }

      public void CommitTriangles(byte[]? indexBufferBytes, NoeFormat indexDataType, int numIndices, NoePrimitiveType primitiveType, bool usePlotMap) {
        if (indexBufferBytes == null) {
          this.CommitTrianglesWithoutIndices(primitiveType, usePlotMap);
          return;
        }

        throw new NotImplementedException();
      }

      public unsafe void CommitTrianglesWithoutIndices(NoePrimitiveType primitiveType, bool usePlotMap) {
        switch (primitiveType) {
          case NoePrimitiveType.RPGEO_POINTS: {
              var skin = model_.Skin;

              var mesh = skin.AddMesh();
              mesh.Name = name_;

              var vertices = new List<IVertex>();
              var bytes = positionBuffer_.ToArray();
              using var br = new SchemaBinaryReader(bytes);

              var i = 0;
              while (true) {
                br.Position = positionOffset_ + i * positionStride_;
                if (br.Eof) {
                  break;
                }

                switch (positionFormat_) {
                  case NoeFormat.RPGEODATA_FLOAT: {
                      var x = br.ReadSingle();
                      var y = br.ReadSingle();
                      var z = br.ReadSingle();

                      vertices.Add(skin.AddVertex(x, y, z));
                      break;
                    }
                  default: throw new NotImplementedException();
                }

                ++i;
              }

              mesh.AddPoints(vertices.ToArray());
              break;
            }
        }
      }

      public IModel ConstructModel() => model_;

      public object CreateContext() {
        return new object();
      }
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

    public static INoeBitStream NoeBitStream(byte[] data = null) {
      return new NoeBitStreamReader(data);
    }

    public static void Main() {
      var engine = Python.CreateEngine();

      var noesisDirectory = new FinDirectory("C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\ModelPluginWrappers\\noesis");

      var scope = engine.CreateScope();

      // Hooks up common Python imports
      engine.SetSearchPaths([
        "C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\ModelPluginWrappers\\lib\\3.4",
        "C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\ModelPluginWrappers\\lib\\noesis",
        noesisDirectory.FullPath,
      ]);

      // Hooks up missing Python imports
      {
      }

      var handlesByExtension = new Dictionary<string, Handle>();

      // Hooks up Noesis imports
      {
        {
          var noesisModule = engine.CreateModule("noesis");
          noesisModule.SetVariable("logPopup", () => {});
          noesisModule.SetVariable("register", (string formatName, string extension) => handlesByExtension[extension] = new Handle(formatName, extension));
          noesisModule.SetVariable("setHandlerTypeCheck", (Handle handle, Func<byte[], bool> checkType) => {
            handle.checkType = checkType;
          });
          noesisModule.SetVariable("setHandlerLoadModel", (Handle handle, Func<byte[], PythonList, bool> loadModel) => {
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

        {
          var rpg = new Rpg();

          var rapiModule = engine.CreateModule("rapi");
          rapiModule.SetVariable("rpgReset", () => rpg = new Rpg());
          rapiModule.SetVariable("rpgSetName", rpg.SetName);
          rapiModule.SetVariable("rpgBindPositionBufferOfs", rpg.BindPositionBufferOffset);
          rapiModule.SetVariable("rpgCommitTriangles", rpg.CommitTriangles);
          rapiModule.SetVariable("rpgConstructModel", rpg.ConstructModel);
          rapiModule.SetVariable("rpgCreateContext", rpg.CreateContext);
        }

        {
          var incNoesisModule = engine.ImportModule("inc_noesis");
          incNoesisModule.SetVariable("NoeBitStream", NoeBitStream);
        }
      }

      var name = "midnight_club_2";

      engine.Execute($@"
import {name}

{name}.registerNoesisTypes()
", scope);

      var midnightClub2Handle = handlesByExtension[".xmod"];

      var models = new PythonList();

      {
        var bytes = File.ReadAllBytes("C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\ModelPluginWrappers\\models\\midnight_club_2\\vp_supraa_body_ui_h.xmod");
        midnightClub2Handle.loadModel(bytes, models);
      }

      foreach (var model in models) {
        new AssimpIndirectModelExporter().ExportModel(
            new ModelExporterParams {
              OutputFile = new FinFile("C:\\Users\\Ryan\\Documents\\CSharpWorkspace\\FinModelUtility\\FinModelUtility\\ModelPluginWrappers\\test.fbx"),
              Model = model as IModel,
            });
      }
    }

    public static void PushEnumIntoScope<TEnum>(this ScriptScope scriptScope) 
      where TEnum : struct, Enum {
      foreach (var value in Enum.GetValues<TEnum>()) {
        var name = value.ToString();
        scriptScope.SetVariable(name, value);
      }
    }
  }
}