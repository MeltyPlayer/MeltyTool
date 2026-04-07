using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

using fin.io;
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
      var tempScriptFile = new FinFile(Path.Combine(tempRoot.FullPath,
                                                    "meltytool_manifest_to_fbx.py"));
      var manifestFile = BlenderIntermediateExporter.ExportPackage(tempRoot,
                                                                   modelExporterParams);

      tempScriptFile.WriteAllText(BLENDER_SCRIPT_);

      var outputFbx = outputFile.FileType.Equals(".fbx",
                                                 StringComparison.OrdinalIgnoreCase)
          ? outputFile
          : outputFile.CloneWithFileType(".fbx");

      RunBlender_(blenderExe,
                  tempScriptFile,
                  manifestFile,
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
                                  ISystemFile manifestFile,
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
    startInfo.ArgumentList.Add("--manifest");
    startInfo.ArgumentList.Add(manifestFile.FullPath);
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
import json
import sys
from pathlib import Path

import bpy
import mathutils


def parse_args() -> argparse.Namespace:
    argv = sys.argv
    if "--" not in argv:
        raise SystemExit("Expected Blender args after '--'")

    argv = argv[argv.index("--") + 1 :]

    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", required=True)
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


def load_manifest(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


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


def alpha_mode_to_blend_method(alpha_mode: str | None) -> str:
    if alpha_mode is None:
        return "OPAQUE"
    alpha_mode = str(alpha_mode).upper()
    if alpha_mode == "TRANSPARENT":
        return "BLEND"
    if alpha_mode == "MASK":
        return "CLIP"
    return "OPAQUE"


def wrap_mode_to_extension(wrap_mode_u: str | None, wrap_mode_v: str | None) -> str:
    modes = {str(wrap_mode_u).upper(), str(wrap_mode_v).upper()}
    if "REPEAT" in modes or "MIRROR_REPEAT" in modes:
        return "REPEAT"
    return "EXTEND"


def ensure_uv_layer(mesh_data: bpy.types.Mesh, index: int) -> bpy.types.MeshUVLoopLayer:
    layer_name = f"UV{index}"
    existing = mesh_data.uv_layers.get(layer_name)
    if existing is not None:
        return existing
    return mesh_data.uv_layers.new(name=layer_name)


def build_materials(package: dict, package_root: Path) -> dict[str, bpy.types.Material]:
    materials: dict[str, bpy.types.Material] = {}

    for material_data in package.get("materials", []):
        name = material_data["name"]
        material = bpy.data.materials.new(name=name)
        material.use_nodes = True
        material.use_backface_culling = not material_data.get("doubleSided", True)
        material.blend_method = alpha_mode_to_blend_method(material_data.get("alphaMode"))

        node_tree = material.node_tree
        for node in list(node_tree.nodes):
            node_tree.nodes.remove(node)

        output_node = node_tree.nodes.new("ShaderNodeOutputMaterial")
        output_node.location = (500, 0)
        bsdf_node = node_tree.nodes.new("ShaderNodeBsdfPrincipled")
        bsdf_node.location = (200, 0)
        node_tree.links.new(bsdf_node.outputs["BSDF"], output_node.inputs["Surface"])

        primary = material_data.get("primaryTexture")
        if primary:
            image_node = node_tree.nodes.new("ShaderNodeTexImage")
            image_node.location = (-250, 50)
            image_path = package_root / primary["path"]
            image_node.image = bpy.data.images.load(str(image_path), check_existing=True)
            image_node.extension = wrap_mode_to_extension(primary.get("wrapModeU"),
                                                          primary.get("wrapModeV"))

            uv_node = node_tree.nodes.new("ShaderNodeUVMap")
            uv_node.location = (-500, 50)
            uv_node.uv_map = f"UV{int(primary.get('uvIndex', 0))}"
            node_tree.links.new(uv_node.outputs["UV"], image_node.inputs["Vector"])
            node_tree.links.new(image_node.outputs["Color"], bsdf_node.inputs["Base Color"])

            if material.blend_method != "OPAQUE":
                node_tree.links.new(image_node.outputs["Alpha"], bsdf_node.inputs["Alpha"])

        normal_texture = material_data.get("normalTexture")
        if normal_texture:
            normal_image_node = node_tree.nodes.new("ShaderNodeTexImage")
            normal_image_node.location = (-250, -250)
            normal_image = bpy.data.images.load(str(package_root / normal_texture["path"]),
                                                check_existing=True)
            normal_image.colorspace_settings.name = "Non-Color"
            normal_image_node.image = normal_image
            normal_image_node.extension = wrap_mode_to_extension(normal_texture.get("wrapModeU"),
                                                                 normal_texture.get("wrapModeV"))

            normal_uv_node = node_tree.nodes.new("ShaderNodeUVMap")
            normal_uv_node.location = (-500, -250)
            normal_uv_node.uv_map = f"UV{int(normal_texture.get('uvIndex', 0))}"

            normal_map_node = node_tree.nodes.new("ShaderNodeNormalMap")
            normal_map_node.location = (-25, -250)

            node_tree.links.new(normal_uv_node.outputs["UV"], normal_image_node.inputs["Vector"])
            node_tree.links.new(normal_image_node.outputs["Color"], normal_map_node.inputs["Color"])
            node_tree.links.new(normal_map_node.outputs["Normal"], bsdf_node.inputs["Normal"])

        materials[name] = material

    return materials


def build_armature(package: dict) -> bpy.types.Object:
    armature_data = bpy.data.armatures.new(f"{package.get('name', 'model')}_Armature")
    armature_object = bpy.data.objects.new(armature_data.name, armature_data)
    bpy.context.scene.collection.objects.link(armature_object)
    bpy.context.view_layer.objects.active = armature_object
    armature_object.select_set(True)

    bpy.ops.object.mode_set(mode="EDIT")

    edit_bones: dict[str, bpy.types.EditBone] = {}
    bone_data_by_name: dict[str, dict] = {}
    world_matrix_by_name: dict[str, mathutils.Matrix] = {}

    for bone_data in package.get("bones", []):
        bone_name = bone_data["name"]
        edit_bone = armature_data.edit_bones.new(bone_name)
        edit_bone.head = (0.0, 0.0, 0.0)
        edit_bone.tail = (0.0, max(float(bone_data.get("length", 0.05)), 0.01), 0.0)
        edit_bones[bone_name] = edit_bone
        bone_data_by_name[bone_name] = bone_data

    unresolved = set(edit_bones.keys())
    while unresolved:
        progress = False
        for bone_name in list(unresolved):
            bone_data = bone_data_by_name[bone_name]
            parent_name = bone_data.get("parentName")
            if parent_name and parent_name not in world_matrix_by_name:
                continue

            translation = mathutils.Vector(bone_data.get("translation", [0.0, 0.0, 0.0]))
            rotation_values = bone_data.get("rotation", [0.0, 0.0, 0.0, 1.0])
            rotation = mathutils.Quaternion((rotation_values[3],
                                             rotation_values[0],
                                             rotation_values[1],
                                             rotation_values[2]))
            scale = mathutils.Vector(bone_data.get("scale", [1.0, 1.0, 1.0]))

            local_matrix = (
                mathutils.Matrix.Translation(translation)
                @ rotation.to_matrix().to_4x4()
                @ mathutils.Matrix.Diagonal((scale.x, scale.y, scale.z, 1.0))
            )

            parent_matrix = world_matrix_by_name[parent_name] if parent_name else mathutils.Matrix.Identity(4)
            world_matrix = parent_matrix @ local_matrix

            edit_bone = edit_bones[bone_name]
            if parent_name:
                edit_bone.parent = edit_bones[parent_name]
                edit_bone.use_connect = False

            edit_bone.matrix = world_matrix
            edit_bone.length = max(float(bone_data.get("length", 0.05)), 0.01)

            world_matrix_by_name[bone_name] = world_matrix
            unresolved.remove(bone_name)
            progress = True

        if not progress:
            raise RuntimeError("Failed to resolve armature hierarchy from manifest")

    bpy.ops.object.mode_set(mode="OBJECT")
    for pose_bone in armature_object.pose.bones:
        pose_bone.rotation_mode = "QUATERNION"

    return armature_object


def build_meshes(package: dict,
                 materials_by_name: dict[str, bpy.types.Material],
                 armature_object: bpy.types.Object) -> list[bpy.types.Object]:
    mesh_objects: list[bpy.types.Object] = []

    for mesh_data in package.get("meshes", []):
        vertices = [tuple(vertex["position"]) for vertex in mesh_data.get("vertices", [])]
        faces = [tuple(face["indices"]) for face in mesh_data.get("faces", [])]

        blender_mesh = bpy.data.meshes.new(mesh_data["name"])
        blender_mesh.from_pydata(vertices, [], faces)
        blender_mesh.update()

        max_uv_count = 0
        for vertex_data in mesh_data.get("vertices", []):
            max_uv_count = max(max_uv_count, len(vertex_data.get("uvs", [])))

        for uv_index in range(max_uv_count):
            ensure_uv_layer(blender_mesh, uv_index)

        mesh_object = bpy.data.objects.new(mesh_data["name"], blender_mesh)
        bpy.context.scene.collection.objects.link(mesh_object)

        material_slot_by_name: dict[str, int] = {}
        for material_name in mesh_data.get("materialNames", []):
            material = materials_by_name.get(material_name)
            if material is None:
                continue
            material_slot_by_name[material_name] = len(mesh_object.data.materials)
            mesh_object.data.materials.append(material)

        for polygon_index, face_data in enumerate(mesh_data.get("faces", [])):
            material_index = material_slot_by_name.get(face_data.get("materialName", ""))
            if material_index is not None:
                blender_mesh.polygons[polygon_index].material_index = material_index

        for uv_index in range(max_uv_count):
            uv_layer = blender_mesh.uv_layers.get(f"UV{uv_index}")
            if uv_layer is None:
                continue

            for loop_index, loop in enumerate(blender_mesh.loops):
                vertex_data = mesh_data["vertices"][loop.vertex_index]
                uvs = vertex_data.get("uvs", [])
                if uv_index < len(uvs):
                    uv = uvs[uv_index]
                    uv_layer.data[loop_index].uv = (float(uv[0]), 1.0 - float(uv[1]))
                else:
                    uv_layer.data[loop_index].uv = (0.0, 0.0)

        modifier = mesh_object.modifiers.new(name="Armature", type="ARMATURE")
        modifier.object = armature_object

        for bone in package.get("bones", []):
            mesh_object.vertex_groups.new(name=bone["name"])

        for vertex_index, vertex_data in enumerate(mesh_data.get("vertices", [])):
            for weight_data in vertex_data.get("boneWeights", []):
                group = mesh_object.vertex_groups.get(weight_data["boneName"])
                if group is not None:
                    group.add([vertex_index], float(weight_data["weight"]), "REPLACE")

        mesh_object.parent = armature_object
        mesh_objects.append(mesh_object)

    return mesh_objects


def local_matrix_from_bone_data(bone_data: dict,
                                translation: list[float] | None = None,
                                rotation: list[float] | None = None,
                                scale: list[float] | None = None) -> mathutils.Matrix:
    translation_values = translation if translation is not None else bone_data.get("translation", [0.0, 0.0, 0.0])
    rotation_values = rotation if rotation is not None else bone_data.get("rotation", [0.0, 0.0, 0.0, 1.0])
    scale_values = scale if scale is not None else bone_data.get("scale", [1.0, 1.0, 1.0])

    translation_vec = mathutils.Vector(translation_values)
    rotation_quat = mathutils.Quaternion((rotation_values[3],
                                          rotation_values[0],
                                          rotation_values[1],
                                          rotation_values[2]))
    scale_vec = mathutils.Vector(scale_values)

    return (
        mathutils.Matrix.Translation(translation_vec)
        @ rotation_quat.to_matrix().to_4x4()
        @ mathutils.Matrix.Diagonal((scale_vec.x, scale_vec.y, scale_vec.z, 1.0))
    )


def build_actions(package: dict, armature_object: bpy.types.Object) -> tuple[list[bpy.types.Action], int]:
    actions: list[bpy.types.Action] = []
    max_frame_number = 1
    armature_object.animation_data_create()

    bone_data_by_name = {bone_data["name"]: bone_data for bone_data in package.get("bones", [])}
    rest_local_matrix_by_name = {
        bone_name: local_matrix_from_bone_data(bone_data)
        for bone_name, bone_data in bone_data_by_name.items()
    }

    for animation_data in package.get("animations", []):
        action = bpy.data.actions.new(animation_data["name"])
        action.use_fake_user = True
        armature_object.animation_data.action = action

        frame_rate = float(animation_data.get("frameRate", 20.0))
        bpy.context.scene.render.fps = max(1, int(round(frame_rate)))

        animation_frame_count = int(animation_data.get("frameCount", 1))
        max_frame_number = max(max_frame_number, animation_frame_count)

        for bone_animation_data in animation_data.get("bones", []):
            bone_name = bone_animation_data["boneName"]
            pose_bone = armature_object.pose.bones.get(bone_name)
            bone_data = bone_data_by_name.get(bone_name)
            rest_local_matrix = rest_local_matrix_by_name.get(bone_name)
            if pose_bone is None or bone_data is None or rest_local_matrix is None:
                continue

            pose_bone.rotation_mode = "QUATERNION"
            translations = bone_animation_data.get("translations")
            rotations = bone_animation_data.get("rotations")
            scales = bone_animation_data.get("scales")

            frame_count = 0
            if translations is not None:
                frame_count = max(frame_count, len(translations))
            if rotations is not None:
                frame_count = max(frame_count, len(rotations))
            if scales is not None:
                frame_count = max(frame_count, len(scales))

            for frame_index in range(frame_count):
                frame_number = frame_index + 1
                bpy.context.scene.frame_set(frame_number)

                translation = translations[frame_index] if translations is not None and frame_index < len(translations) else None
                rotation = rotations[frame_index] if rotations is not None and frame_index < len(rotations) else None
                scale = scales[frame_index] if scales is not None and frame_index < len(scales) else None

                animated_local_matrix = local_matrix_from_bone_data(bone_data,
                                                                    translation=translation,
                                                                    rotation=rotation,
                                                                    scale=scale)
                basis_matrix = rest_local_matrix.inverted() @ animated_local_matrix
                basis_location, basis_rotation, basis_scale = basis_matrix.decompose()

                pose_bone.location = basis_location
                pose_bone.rotation_quaternion = basis_rotation
                pose_bone.scale = basis_scale

                pose_bone.keyframe_insert(data_path="location", frame=frame_number)
                pose_bone.keyframe_insert(data_path="rotation_quaternion", frame=frame_number)
                pose_bone.keyframe_insert(data_path="scale", frame=frame_number)

        actions.append(action)

    if actions:
        armature_object.animation_data.action = actions[0]
        bpy.context.scene.frame_start = 1
        bpy.context.scene.frame_end = max_frame_number

    return actions, max_frame_number


def build_scene(package: dict, package_root: Path, include_actions: bool) -> tuple[bpy.types.Object, int]:
    materials_by_name = build_materials(package, package_root)
    armature_object = build_armature(package)
    build_meshes(package, materials_by_name, armature_object)
    max_frame_number = 1
    if include_actions:
        _, max_frame_number = build_actions(package, armature_object)
    remove_non_exportable()
    return armature_object, max_frame_number


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
        bake_anim_use_nla_strips=False,
        bake_anim_use_all_actions=True,
        bake_anim_force_startend_keying=True,
        bake_anim_step=1.0,
        bake_anim_simplify_factor=0.0,
        path_mode="AUTO",
        global_scale=args.global_scale,
        axis_forward=args.axis_forward,
        axis_up=args.axis_up,
    )


def main() -> int:
    args = parse_args()
    manifest_path = Path(args.manifest)
    output_path = Path(args.output)
    output_path.parent.mkdir(parents=True, exist_ok=True)

    animation_only = as_bool(args.animation_only)

    reset_scene()
    package = load_manifest(manifest_path)
    _, _ = build_scene(package, manifest_path.parent, include_actions=animation_only)

    if animation_only:
        export_animation(args)
    else:
        export_model(args)

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
""";
}
