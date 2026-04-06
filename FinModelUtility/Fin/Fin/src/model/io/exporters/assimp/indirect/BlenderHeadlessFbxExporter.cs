using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using fin.io;
using fin.model.io.exporters.gltf;
using fin.util.asserts;

namespace fin.model.io.exporters.assimp.indirect;

internal static class BlenderHeadlessFbxExporter {
  private const string BLENDER_EXE_ENV_ = "MELTYTOOL_BLENDER_EXE";
  private const string DEFAULT_BLENDER_EXE_PATH_ = @"F:\Blender 5\Blender.exe";
  private const string KEEP_TEMP_ENV_ = "MELTYTOOL_BLENDER_KEEP_TEMP";
  private const string GLOBAL_SCALE_ENV_ = "MELTYTOOL_BLENDER_GLOBAL_SCALE";
  private const string AXIS_FORWARD_ENV_ = "MELTYTOOL_BLENDER_AXIS_FORWARD";
  private const string AXIS_UP_ENV_ = "MELTYTOOL_BLENDER_AXIS_UP";
  private const string EMBED_TEXTURES_ENV_ = "MELTYTOOL_BLENDER_EMBED_TEXTURES";

  public static bool IsConfigured()
    => TryGetBlenderExe_(out _);

  public static void ExportFbx(IModelExporterParams modelExporterParams,
                               bool animationOnly) {
    Asserts.True(TryGetBlenderExe_(out var blenderExe),
                 $"Set {BLENDER_EXE_ENV_} to a valid Blender executable path before exporting FBX through Blender.");

    var outputFile = modelExporterParams.OutputFile;
    var outputDirectory = outputFile.AssertGetParent();
    outputDirectory.Create();

    var tempRoot = new FinDirectory(
        Path.Combine(Path.GetTempPath(),
                     "meltytool_blender_fbx",
                     Guid.NewGuid().ToString("N")));
    tempRoot.Create();

    try {
      var stem = outputFile.NameWithoutExtension.ToString();
      var tempInputFile = new FinFile(Path.Combine(tempRoot.FullPath,
                                                   $"{stem}.glb"));
      var tempScriptFile = new FinFile(Path.Combine(tempRoot.FullPath,
                                                    "meltytool_glb_to_fbx.py"));

      new GltfModelExporter {
          UvIndices = false,
          Embedded = true,
      }.ExportModel(new ModelExporterParams {
          OutputFile = tempInputFile,
          Model = modelExporterParams.Model,
          Scale = modelExporterParams.Scale,
      });

      tempScriptFile.WriteAllText(BLENDER_SCRIPT_);

      var outputFbx = outputFile.FileType.Equals(".fbx",
                                                 StringComparison.OrdinalIgnoreCase)
          ? outputFile
          : outputFile.CloneWithFileType(".fbx");

      RunBlender_(blenderExe,
                  tempScriptFile,
                  tempInputFile,
                  outputFbx,
                  animationOnly);

      Asserts.True(outputFbx.Exists,
                   $"Blender did not produce the expected FBX file: {outputFbx.FullPath}");
    } finally {
      if (!GetFlagFromEnvironment_(KEEP_TEMP_ENV_)) {
        try {
          Directory.Delete(tempRoot.FullPath, true);
        } catch {
          // Best effort cleanup only.
        }
      }
    }
  }

  private static bool TryGetBlenderExe_(out string blenderExe) {
    blenderExe = Environment.GetEnvironmentVariable(BLENDER_EXE_ENV_) ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(blenderExe) && File.Exists(blenderExe)) {
      return true;
    }

    blenderExe = DEFAULT_BLENDER_EXE_PATH_;
    return File.Exists(blenderExe);
  }

  private static void RunBlender_(string blenderExe,
                                  ISystemFile scriptFile,
                                  ISystemFile inputFile,
                                  ISystemFile outputFile,
                                  bool animationOnly) {
    var startInfo = new ProcessStartInfo {
        FileName = blenderExe,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
    };

    startInfo.ArgumentList.Add("--background");
    startInfo.ArgumentList.Add("--python");
    startInfo.ArgumentList.Add(scriptFile.FullPath);
    startInfo.ArgumentList.Add("--");
    startInfo.ArgumentList.Add("--input");
    startInfo.ArgumentList.Add(inputFile.FullPath);
    startInfo.ArgumentList.Add("--output");
    startInfo.ArgumentList.Add(outputFile.FullPath);
    startInfo.ArgumentList.Add("--animation-only");
    startInfo.ArgumentList.Add(animationOnly ? "true" : "false");
    startInfo.ArgumentList.Add("--global-scale");
    startInfo.ArgumentList.Add(GetEnvironmentOrDefault_(GLOBAL_SCALE_ENV_, "1"));
    startInfo.ArgumentList.Add($"--axis-forward={GetEnvironmentOrDefault_(AXIS_FORWARD_ENV_, "-Z")}");
    startInfo.ArgumentList.Add($"--axis-up={GetEnvironmentOrDefault_(AXIS_UP_ENV_, "Y")}");
    startInfo.ArgumentList.Add("--embed-textures");
    startInfo.ArgumentList.Add(GetFlagFromEnvironment_(EMBED_TEXTURES_ENV_, true)
                                   ? "true"
                                   : "false");

    using var process = Process.Start(startInfo);
    Asserts.True(process != null,
                 $"Failed to launch Blender: {blenderExe}");

    var stdout = process.StandardOutput.ReadToEnd();
    var stderr = process.StandardError.ReadToEnd();
    process.WaitForExit();

    Asserts.True(process.ExitCode == 0,
                 $"Blender FBX export failed with exit code {process.ExitCode}.\nSTDOUT:\n{stdout}\nSTDERR:\n{stderr}");
  }

  private static string GetEnvironmentOrDefault_(string key,
                                                 string defaultValue)
    => Environment.GetEnvironmentVariable(key) is { Length: > 0 } value
        ? value
        : defaultValue;

  private static bool GetFlagFromEnvironment_(string key,
                                              bool defaultValue = false) {
    var raw = Environment.GetEnvironmentVariable(key);
    if (string.IsNullOrWhiteSpace(raw)) {
      return defaultValue;
    }

    if (bool.TryParse(raw, out var boolValue)) {
      return boolValue;
    }

    if (double.TryParse(raw,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out var numericValue)) {
      return numericValue != 0;
    }

    return defaultValue;
  }

  private const string BLENDER_SCRIPT_ = """
from __future__ import annotations

import argparse
import sys
from pathlib import Path

import bpy


def parse_args() -> argparse.Namespace:
    argv = sys.argv
    if "--" not in argv:
        raise SystemExit("Expected Blender args after '--'")

    argv = argv[argv.index("--") + 1 :]

    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--animation-only", required=True)
    parser.add_argument("--global-scale", type=float, default=1.0)
    parser.add_argument("--axis-forward", default="-Z")
    parser.add_argument("--axis-up", default="Y")
    parser.add_argument("--embed-textures", default="true")
    return parser.parse_args(argv)


def as_bool(value: str) -> bool:
    return str(value).strip().lower() in {"1", "true", "yes", "y", "on"}


def reset_scene() -> None:
    bpy.ops.wm.read_factory_settings(use_empty=True)


def import_glb(path: Path) -> None:
    bpy.ops.import_scene.gltf(filepath=str(path))


def remove_non_exportable() -> None:
    for obj in list(bpy.data.objects):
        if obj.type not in {"MESH", "ARMATURE"}:
            bpy.data.objects.remove(obj, do_unlink=True)


def remove_meshes() -> None:
    for obj in list(bpy.data.objects):
        if obj.type == "MESH":
            bpy.data.objects.remove(obj, do_unlink=True)


def select_exportable(types: set[str]) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    active = None
    for obj in bpy.data.objects:
        if obj.type in types:
            obj.select_set(True)
            if active is None:
                active = obj
    if active is not None:
        bpy.context.view_layer.objects.active = active


def export_model(args: argparse.Namespace) -> None:
    select_exportable({"MESH", "ARMATURE"})
    bpy.ops.export_scene.fbx(
        filepath=str(Path(args.output)),
        use_selection=True,
        object_types={"MESH", "ARMATURE"},
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="COPY" if as_bool(args.embed_textures) else "AUTO",
        embed_textures=as_bool(args.embed_textures),
        global_scale=args.global_scale,
        axis_forward=args.axis_forward,
        axis_up=args.axis_up,
    )


def export_animation(args: argparse.Namespace) -> None:
    remove_meshes()
    select_exportable({"ARMATURE"})
    bpy.ops.export_scene.fbx(
        filepath=str(Path(args.output)),
        use_selection=True,
        object_types={"ARMATURE"},
        add_leaf_bones=False,
        bake_anim=True,
        bake_anim_use_all_bones=True,
        bake_anim_simplify_factor=0.0,
        path_mode="AUTO",
        global_scale=args.global_scale,
        axis_forward=args.axis_forward,
        axis_up=args.axis_up,
    )


def main() -> int:
    args = parse_args()
    input_path = Path(args.input)
    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    reset_scene()
    import_glb(input_path)
    remove_non_exportable()

    if as_bool(args.animation_only):
        export_animation(args)
    else:
        export_model(args)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
""";
}
