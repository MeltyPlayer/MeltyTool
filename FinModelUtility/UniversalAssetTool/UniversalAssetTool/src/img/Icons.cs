using System.Drawing;
using System.Reflection;

using uni.util.image;

namespace uni.img;

public static class Icons {
  private static readonly Assembly assembly_ =
      Assembly.GetExecutingAssembly();

  public static readonly Image folderClosedImage =
      LoadIcon_("uni.src.img.folder_closed.png");

  public static readonly Image folderOpenImage =
      LoadIcon_("uni.src.img.folder_open.png");

  public static readonly Image modelImage =
      LoadIcon_("uni.src.img.model.png");

  public static readonly Image sceneImage =
      LoadIcon_("uni.src.img.scene.png");

  public static readonly Image musicImage =
      LoadIcon_("uni.src.img.music.png");

  private static Image LoadIcon_(string embeddedResourceName)
    => EmbeddedResourceImageUtil.Load(Icons.assembly_, embeddedResourceName);
}