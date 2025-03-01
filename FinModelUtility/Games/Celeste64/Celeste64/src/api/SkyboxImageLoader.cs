using System.Drawing;

using fin.image;
using fin.io;

namespace Celeste64.api;

public class SkyboxImageLoader {
  public static void LoadSkyboxImages(IReadOnlyGenericFile skyboxImageFile,
                                      out IImage topImage,
                                      out IImage backImage,
                                      out IImage rightImage,
                                      out IImage frontImage,
                                      out IImage leftImage,
                                      out IImage bottomImage) {
    var skyboxImage = FinImage.FromFile(skyboxImageFile);

    var sideLength = skyboxImage.Height / 3;

    topImage
        = skyboxImage.SubImage(new Rectangle(0, 0, sideLength, sideLength));
    backImage
        = skyboxImage.SubImage(
            new Rectangle(0 * sideLength, sideLength, sideLength, sideLength));
    rightImage
        = skyboxImage.SubImage(
            new Rectangle(1 * sideLength, sideLength, sideLength, sideLength));
    frontImage
        = skyboxImage.SubImage(
            new Rectangle(2 * sideLength, sideLength, sideLength, sideLength));
    leftImage
        = skyboxImage.SubImage(
            new Rectangle(3 * sideLength, sideLength, sideLength, sideLength));
    bottomImage
        = skyboxImage.SubImage(
            new Rectangle(0, 2 * sideLength, sideLength, sideLength));
  }
}