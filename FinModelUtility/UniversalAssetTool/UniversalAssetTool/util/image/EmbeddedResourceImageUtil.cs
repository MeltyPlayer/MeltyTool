using System.Drawing;
using System.Reflection;

using fin.util.asserts;

namespace uni.util.image;

public static class EmbeddedResourceImageUtil {
  public static Image Load(Assembly assembly, string embeddedResourceName) {
    var resourceNames = assembly.GetManifestResourceNames();
    Asserts.True(resourceNames.Contains(embeddedResourceName));

    using var stream = assembly.GetManifestResourceStream(embeddedResourceName)!;
    return Bitmap.FromStream(stream);
  }
}