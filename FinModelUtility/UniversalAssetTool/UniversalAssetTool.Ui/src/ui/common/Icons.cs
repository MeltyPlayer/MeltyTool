using System.Drawing;
using System.Reflection;

using uni.util.image;

namespace uni.ui.common {
  public static class Icons {
    private static readonly Assembly assembly_ =
        Assembly.GetExecutingAssembly();

    public static readonly Image folderClosedImage =
        LoadIcon_("uni.img.folder_closed.png");

    public static readonly Image folderOpenImage =
        LoadIcon_("uni.img.folder_open.png");

    public static readonly Image modelImage =
        LoadIcon_("uni.img.model.png");

    public static readonly Image sceneImage =
        LoadIcon_("uni.img.scene.png");

    public static readonly Image musicImage =
        LoadIcon_("uni.img.music.png");

    private static Image LoadIcon_(string embeddedResourceName)
      => EmbeddedResourceImageUtil.Load(Icons.assembly_, embeddedResourceName);
  }
}