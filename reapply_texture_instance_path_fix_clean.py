from pathlib import Path
import sys
import shutil

repo_root = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(r"F:\MeltyToolOosh_buildtest")
target = repo_root / "FinModelUtility" / "Fin" / "Fin" / "src" / "model" / "io" / "exporters" / "assimp" / "indirect" / "BlenderIntermediateExporter.cs"
backup = target.with_suffix(target.suffix + ".bak")

if not target.exists():
    print(f"Target file not found: {target}")
    raise SystemExit(1)

if backup.exists():
    shutil.copy2(backup, target)
    print(f"Restored original backup from {backup}")
else:
    print("Original .bak backup was not found; patching current file in place.")

text = target.read_text(encoding="utf-8-sig")

signature = "  private static Dictionary<IReadOnlyTexture, string> ExportTextures_("
start = text.find(signature)
if start == -1:
    print("Could not find ExportTextures_ method.")
    raise SystemExit(2)

brace_start = text.find("{", start)
if brace_start == -1:
    print("Could not find opening brace for ExportTextures_.")
    raise SystemExit(3)

depth = 0
end = None
for i in range(brace_start, len(text)):
    ch = text[i]
    if ch == "{":
        depth += 1
    elif ch == "}":
        depth -= 1
        if depth == 0:
            end = i + 1
            break

if end is None:
    print("Could not find end of ExportTextures_ method.")
    raise SystemExit(4)

replacement = """  private static Dictionary<IReadOnlyTexture, string> ExportTextures_(
      IReadOnlyModel model,
      ISystemDirectory texturesDirectory) {
    var texturePathByTexture = new Dictionary<IReadOnlyTexture, string>();
    var relativePathByTextureFileName =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    var textures = model.MaterialManager.All
                        .SelectMany(material => material.Textures)
                        .ToArray();

    var exportedTextureCount = 0;
    foreach (var texture in textures) {
      var textureFileName =
          texture.ValidFileName ?? texture.Name ?? $"texture_{exportedTextureCount:D4}";
      if (!relativePathByTextureFileName.TryGetValue(textureFileName,
                                                     out var relativePath)) {
        var stem = Path.GetFileNameWithoutExtension(textureFileName);
        if (string.IsNullOrWhiteSpace(stem)) {
          stem = $"texture_{exportedTextureCount:D4}";
        }

        var fileName = $"{exportedTextureCount:D4}_{SanitizeFileName_(stem)}.png";
        relativePath = Path.Combine("textures", fileName).Replace('\\\\', '/');
        var file = new FinFile(Path.Combine(texturesDirectory.FullPath, fileName));

        using var stream = new MemoryStream();
        texture.Image.ExportToStream(stream, LocalImageFormat.PNG);
        file.WriteAllBytes(stream.ToArray());

        relativePathByTextureFileName[textureFileName] = relativePath;
        ++exportedTextureCount;
      }

      texturePathByTexture[texture] = relativePath;
    }

    return texturePathByTexture;
  }"""

new_text = text[:start] + replacement + text[end:]
target.write_text(new_text, encoding="utf-8-sig")

print(f"Patched {target}")
