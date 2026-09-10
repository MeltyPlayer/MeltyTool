using System.Reflection;

using System.Numerics;

using fin.io;
using fin.model;
using fin.testing.model;
using fin.testing;

using rollingMadness.api;
using rollingMadness.schema;

namespace rollingMadness;

public sealed class AseMeshModelGoldenTests
    : BModelGoldenTests<AseMeshModelFileBundle, AseMeshModelImporter> {
  [Test]
  public void TestParsesAuthoritativeActorAndSidecarMetadata() {
    var bridge = RollingMadnessMetadataParser.ParseActor(
        "bridge",
        "model \"lev2_bridge.ase.mesh\" loop\n" +
        "type_collide mesh\n" +
        "specular 0.6\n");
    NUnit.Framework.Assert.That(bridge.Specular, Is.EqualTo(.6f));
    NUnit.Framework.Assert.That(bridge.CollisionType, Is.EqualTo("mesh"));
    NUnit.Framework.Assert.That(bridge.Animations.Single().Loop, Is.True);

    var animation = RollingMadnessMetadataParser.ParseActor(
        "test",
        "anim \"rise\" \"stick_bend.ase.mesh\" \"\" reverse\n")
        .Animations.Single();
    NUnit.Framework.Assert.That(animation.Name, Is.EqualTo("rise"));
    NUnit.Framework.Assert.That(animation.Reverse, Is.True);
    NUnit.Framework.Assert.That(animation.SoundFileName, Is.Null);

    var textureMetadata = RollingMadnessMetadataParser.ParseDirectives(
        "animframes 5\nanimduration 0.7\n");
    NUnit.Framework.Assert.That(textureMetadata.Select(item => item.Name),
                                Is.EqualTo(new[] {
                                    "animframes", "animduration"
                                }));

    var modelMetadata = RollingMadnessMetadataParser.ParseDirectives(
        "dots \"dot.png\"\ndizzy \"particle.png\"\n");
    NUnit.Framework.Assert.That(modelMetadata.Select(item => item.Name),
                                Is.EqualTo(new[] { "dots", "dizzy" }));
  }

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestPreservesCompoundLightmapNames(
      IFileHierarchyDirectory goldenDirectory) {
    var inputDirectory = goldenDirectory.AssertGetExistingSubdir("input");
    var model = new AseMeshModelImporter().Import(
        this.GetFileBundleFromDirectory(inputDirectory));

    var actualLightmapNames = model.MaterialManager.Textures
                                   .Select(texture => texture.Name)
                                   .Where(name => name.StartsWith(
                                              "lev1.ase.",
                                              StringComparison.Ordinal))
                                   .OrderBy(name => name)
                                   .ToArray();
    var expectedLightmapNames = Enumerable.Range(0, 6)
                                          .Select(index => $"lev1.ase.{index}")
                                          .ToArray();

    NUnit.Framework.Assert.That(actualLightmapNames,
                                Is.EqualTo(expectedLightmapNames));
  }

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestPreservesFaceRenderFlagsAndTriangleLocalUvs(
      IFileHierarchyDirectory goldenDirectory) {
    var inputDirectory = goldenDirectory.AssertGetExistingSubdir("input");
    var fileBundle = this.GetFileBundleFromDirectory(inputDirectory);
    var aseMesh = fileBundle.AseMeshFile.ReadNew<AseMesh>();
    var model = new AseMeshModelImporter().Import(fileBundle);

    NUnit.Framework.Assert.That(model.Skin.Vertices.Count,
                                Is.EqualTo(aseMesh.Triangles.Length * 3));

    var nonzeroFlags = aseMesh.Triangles.Select(triangle => triangle.RenderFlags)
                              .Where(flags => flags != 0)
                              .Distinct();
    foreach (var flags in nonzeroFlags) {
      NUnit.Framework.Assert.That(
          model.MaterialManager.All.Any(material =>
              material.Name?.Contains($"flags_0x{flags:X8}",
                                      StringComparison.Ordinal) == true),
          Is.True);
    }

    var firstTriangle = aseMesh.Triangles[0];
    var sourceIndices = new[] {
        (int) firstTriangle.Vertex1,
        (int) firstTriangle.Vertex2,
        (int) firstTriangle.Vertex3,
    };
    var decalCenter = sourceIndices.Select(index => aseMesh.UvDatas[index].Uv1)
                                   .Aggregate(Vector2.Zero,
                                              (sum, uv) => sum +
                                                  new Vector2(uv.X,
                                                              1 - uv.Y)) / 3;
    var decalShift = new Vector2(-MathF.Floor(decalCenter.X),
                                 -MathF.Floor(decalCenter.Y));

    for (var corner = 0; corner < 3; ++corner) {
      var sourceUv = aseMesh.UvDatas[sourceIndices[corner]].Uv1;
      var expectedUv = new Vector2(sourceUv.X, 1 - sourceUv.Y) + decalShift;
      var actualUv = ((IReadOnlyMultiUvVertex) model.Skin.Vertices[corner])
          .GetUv(1);
      NUnit.Framework.Assert.That(actualUv, Is.EqualTo(expectedUv));
    }
  }

  public override AseMeshModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.FilesWithExtension(".ase.mesh").Single(), directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}
