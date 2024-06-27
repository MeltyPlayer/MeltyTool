namespace fin.image;

public interface IImageReader<in TImageFileBundle>
    where TImageFileBundle : IImageFileBundle {
  IImage ReadImage(TImageFileBundle imageFileBundle);
}