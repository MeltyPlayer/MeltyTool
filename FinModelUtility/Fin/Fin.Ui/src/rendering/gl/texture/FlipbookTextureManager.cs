using fin.data.indexable;
using fin.model;
using fin.ui.rendering.gl.texture;

using readOnly;


namespace fin.math;

[GenerateReadOnly]
public partial interface ITextureFlipbookSwapManager : IDisposable {
  [Const]
  void GenerateGlTexturesIfNull();
  
  void UpdateCurrentFlipbookSwaps(
      (IReadOnlyModelAnimation, float)? animationAndFrame);

  [Const]
  IGlTexture GetCurrentFlipbookSwap(IReadOnlyTexture texture);
}

public sealed class TextureFlipbookSwapManager : ITextureFlipbookSwapManager {
  private readonly IReadOnlyList<IReadOnlyTexture> textures_;

  private bool hasInit_;
  private (IReadOnlyModelAnimation, float)? previousAnimationAndFrame_;

  private bool hasCreatedTextures_;

  private readonly IndexableDictionary<IReadOnlyTexture, IGlTexture>
      texturesToGlTextures_;

  private readonly IndexableDictionary<IReadOnlyTexture, IGlTexture>
      texturesToCurrentFlipbookSwaps_;

  public TextureFlipbookSwapManager(IReadOnlyList<IReadOnlyTexture> textures) {
    this.textures_ = textures;
    this.texturesToGlTextures_ = new(textures.Count);
    this.texturesToCurrentFlipbookSwaps_ = new(textures.Count);
  }

  ~TextureFlipbookSwapManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var glTexture in this.texturesToGlTextures_) {
      glTexture.Dispose();
    }
  }

  public void GenerateGlTexturesIfNull() {
    if (this.hasCreatedTextures_) {
      return;
    }

    this.hasCreatedTextures_ = true;
    foreach (var texture in this.textures_) {
      this.texturesToCurrentFlipbookSwaps_[texture]
          = this.texturesToGlTextures_[texture]
              = GlTexture.FromTexture(texture);
    }
  }

  public void UpdateCurrentFlipbookSwaps(
      (IReadOnlyModelAnimation, float)? animationAndFrame) {
    this.GenerateGlTexturesIfNull();

    if (this.hasInit_ && this.previousAnimationAndFrame_ == animationAndFrame) {
      return;
    }

    this.hasInit_ = true;
    this.previousAnimationAndFrame_ = animationAndFrame;
    this.texturesToCurrentFlipbookSwaps_.Clear();

    var allTextureTracks = animationAndFrame?.Item1.TextureTracks;
    var frame = animationAndFrame?.Item2 ?? 0;
    foreach (var texture in this.textures_) {
      IReadOnlyTexture? flipbookSwap = null;
      if (allTextureTracks?.TryGetValue(texture, out var textureTracks) ?? false) {
        var flipbookSwaps = textureTracks.FlipbookSwaps;
        flipbookSwaps?.TryGetAtFrame(frame, out flipbookSwap);
      }

      this.texturesToCurrentFlipbookSwaps_[texture] =
          this.texturesToGlTextures_[flipbookSwap ?? texture];
    }
  }

  public IGlTexture GetCurrentFlipbookSwap(IReadOnlyTexture texture)
    => this.texturesToCurrentFlipbookSwaps_[texture];
}