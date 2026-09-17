using fin.data.indexable;
using fin.model;
using fin.ui.rendering.gl.texture;

using readOnly;


namespace fin.math;

[GenerateReadOnly]
public partial interface ITextureSwapManager : IDisposable {
  [Const]
  void GenerateGlTexturesIfNull();

  void OverrideGlTexture(IReadOnlyTexture targetFinTexture,
                         IGlTexture glTexture);

  void UpdateCurrentFlipbookSwaps(
      (IReadOnlyModelAnimation, float)? animationAndFrame);

  [Const]
  IGlTexture GetCurrentGlTexture(IReadOnlyTexture finTexture);
}

public sealed class TextureSwapManager : ITextureSwapManager {
  private readonly IReadOnlyList<IReadOnlyTexture> finTextures_;

  private bool hasInit_;
  private (IReadOnlyModelAnimation, float)? previousAnimationAndFrame_;

  private bool hasCreatedTextures_;

  private readonly IndexableDictionary<IReadOnlyTexture, IGlTexture>
      glTextureByFinTexture_;

  private readonly IndexableDictionary<IReadOnlyTexture, IGlTexture>
      overrideGlTextureByFinTexture_;

  private readonly IndexableDictionary<IReadOnlyTexture, IGlTexture>
      finalGlTextureByFinTexture_;

  public TextureSwapManager(IReadOnlyList<IReadOnlyTexture> finTextures) {
    this.finTextures_ = finTextures;
    this.glTextureByFinTexture_ = new(finTextures.Count);
    this.overrideGlTextureByFinTexture_ = new(finTextures.Count);
    this.finalGlTextureByFinTexture_ = new(finTextures.Count);
  }

  ~TextureSwapManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var glTexture in this.glTextureByFinTexture_) {
      glTexture.Dispose();
    }
  }

  public void GenerateGlTexturesIfNull() {
    if (this.hasCreatedTextures_) {
      return;
    }

    this.hasCreatedTextures_ = true;
    foreach (var finTexture in this.finTextures_) {
      var glTexture = this.glTextureByFinTexture_[finTexture]
          = GlTextureSamplerTuple.FromTexture(finTexture);

      if (this.overrideGlTextureByFinTexture_.TryGetValue(
              finTexture,
              out var overrideGlTexture)) {
        glTexture = overrideGlTexture;
      }

      this.finalGlTextureByFinTexture_[finTexture] = glTexture;
    }
  }

  public void OverrideGlTexture(IReadOnlyTexture targetFinTexture,
                                IGlTexture glTexture)
    => this.overrideGlTextureByFinTexture_[targetFinTexture] = glTexture;

  public void UpdateCurrentFlipbookSwaps(
      (IReadOnlyModelAnimation, float)? animationAndFrame) {
    this.GenerateGlTexturesIfNull();

    if (this.hasInit_ && this.previousAnimationAndFrame_ == animationAndFrame) {
      return;
    }

    this.hasInit_ = true;
    this.previousAnimationAndFrame_ = animationAndFrame;
    this.finalGlTextureByFinTexture_.Clear();

    var allTextureTracks = animationAndFrame?.Item1.TextureTracks;
    var frame = animationAndFrame?.Item2 ?? 0;
    foreach (var finTexture in this.finTextures_) {
      IGlTexture? glTexture = null;

      if (this.overrideGlTextureByFinTexture_.TryGetValue(
              finTexture,
              out var overrideGlTexture)) {
        glTexture = overrideGlTexture;
      }

      if (glTexture == null) {
        IReadOnlyTexture? flipbookSwap = null;
        if (allTextureTracks?.TryGetValue(finTexture, out var textureTracks) ??
            false) {
          var flipbookSwaps = textureTracks.FlipbookSwaps;
          flipbookSwaps?.TryGetAtFrame(frame, out flipbookSwap);
        }

        glTexture = this.glTextureByFinTexture_[flipbookSwap ?? finTexture];
      }

      this.finalGlTextureByFinTexture_[finTexture] = glTexture;
    }
  }

  public IGlTexture GetCurrentGlTexture(IReadOnlyTexture finTexture)
    => this.finalGlTextureByFinTexture_[finTexture];
}