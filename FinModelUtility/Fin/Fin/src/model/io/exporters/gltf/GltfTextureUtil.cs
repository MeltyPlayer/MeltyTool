using System;
using System.Numerics;

using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

namespace fin.model.io.exporters.gltf;

public static class GltfTextureUtil {
  public static TextureBuilder UseTexture(this ChannelBuilder channelBuilder,
                                          IReadOnlyTexture finTexture,
                                          MemoryImage memoryImage) {
    var textureBuilder = channelBuilder.UseTexture();
    textureBuilder
        .WithPrimaryImage(memoryImage)
        .WithCoordinateSet(0)
        .WithSampler(
            ConvertWrapMode_(finTexture.WrapModeU),
            ConvertWrapMode_(finTexture.WrapModeV),
            ConvertMinFilter_(finTexture.MinFilter),
            ConvertMagFilter_(finTexture.MagFilter));

    textureBuilder.WithTransform(
        new Vector2(finTexture.Translation?.X ?? 0, finTexture.Translation?.Y ?? 0),
        new Vector2(finTexture.Scale?.X ?? 1, finTexture.Scale?.Y ?? 1),
        finTexture.RotationRadians?.Z ?? 0);

    return textureBuilder;
  }

  private static TextureWrapMode ConvertWrapMode_(WrapMode wrapMode)
    => wrapMode switch {
        WrapMode.CLAMP  => TextureWrapMode.CLAMP_TO_EDGE,
        WrapMode.REPEAT => TextureWrapMode.REPEAT,
        WrapMode.MIRROR_CLAMP or WrapMode.MIRROR_REPEAT => TextureWrapMode
            .MIRRORED_REPEAT,
        _ => throw new ArgumentOutOfRangeException(
            nameof(wrapMode),
            wrapMode,
            null)
    };

  private static TextureMipMapFilter ConvertMinFilter_(
      TextureMinFilter minFilter)
    => minFilter switch {
        TextureMinFilter.NEAR   => TextureMipMapFilter.NEAREST,
        TextureMinFilter.LINEAR => TextureMipMapFilter.LINEAR,
        TextureMinFilter.NEAR_MIPMAP_NEAR => TextureMipMapFilter
            .NEAREST_MIPMAP_NEAREST,
        TextureMinFilter.NEAR_MIPMAP_LINEAR => TextureMipMapFilter
            .NEAREST_MIPMAP_LINEAR,
        TextureMinFilter.LINEAR_MIPMAP_NEAR => TextureMipMapFilter
            .LINEAR_MIPMAP_NEAREST,
        TextureMinFilter.LINEAR_MIPMAP_LINEAR => TextureMipMapFilter
            .LINEAR_MIPMAP_LINEAR,
    };

  private static TextureInterpolationFilter ConvertMagFilter_(
      TextureMagFilter magFilter)
    => magFilter switch {
        TextureMagFilter.NEAR   => TextureInterpolationFilter.NEAREST,
        TextureMagFilter.LINEAR => TextureInterpolationFilter.LINEAR,
    };
}