namespace gx.adpcm;

using System;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/MeleeMedia/blob/master/MeleeMediaLib/Audio/GcAdpcmDecoder.cs
/// </summary>
public static class GcAdpcmDecoder {
  public static void Decode(
      Span<short> pcm,
      ReadOnlySpan<byte> adpcm,
      ReadOnlySpan<short> coefficients,
      ref int sampleIndex,
      ref int hist1,
      ref int hist2) {
    var sampleCount = Math.Min(GcAdpcmMath.ByteCountToSampleCount(adpcm.Length),
                               pcm.Length - sampleIndex);
    if (sampleCount <= 0) {
      return;
    }

    int frameCount = sampleCount.DivideByRoundUp(GcAdpcmMath.SamplesPerFrame);
    int inIndex = 0;

    for (var i = 0; i < frameCount; i++) {
      byte predictorScale = adpcm[inIndex++];
      int scale = (1 << GcAdpcmMath.GetLowNibble(predictorScale)) << 11;
      int predictor = GcAdpcmMath.GetHighNibble(predictorScale);
      short coef1 = coefficients[predictor * 2];
      short coef2 = coefficients[predictor * 2 + 1];

      int samplesToRead
          = Math.Min(GcAdpcmMath.SamplesPerFrame, sampleCount - sampleIndex);

      for (var s = 0; s < samplesToRead; s++) {
        int adpcmSample = s % 2 == 0
            ? GcAdpcmMath.GetHighNibbleSigned(adpcm[inIndex])
            : GcAdpcmMath.GetLowNibbleSigned(adpcm[inIndex++]);
        int distance = scale * adpcmSample;
        int predictedSample = coef1 * hist1 + coef2 * hist2;
        int correctedSample = predictedSample + distance;
        int scaledSample = (correctedSample + 1024) >> 11;
        short clampedSample = GcAdpcmMath.Clamp16(scaledSample);

        hist2 = hist1;
        hist1 = clampedSample;

        pcm[sampleIndex++] = clampedSample;
      }
    }
  }

  public static byte GetPredictorScale(byte[] adpcm, int sample)
    => adpcm[sample / GcAdpcmMath.SamplesPerFrame * GcAdpcmMath.BytesPerFrame];
}