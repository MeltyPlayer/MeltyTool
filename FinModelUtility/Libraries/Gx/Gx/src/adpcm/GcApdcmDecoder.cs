namespace gx.adpcm;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Ploaj/MeleeMedia/blob/master/MeleeMediaLib/Audio/GcAdpcmDecoder.cs
/// </summary>
public static class GcAdpcmDecoder {
  public static short[] Decode(
      ReadOnlySpan<byte> adpcm,
      ReadOnlySpan<short> coefficients,
      ref short hist1,
      ref short hist2) {
    var sampleCount = GcAdpcmMath.ByteCountToSampleCount(adpcm.Length);
    if (sampleCount == 0) {
      return [];
    }

    var pcm = new short[sampleCount];

    int frameCount = sampleCount.DivideByRoundUp(GcAdpcmMath.SamplesPerFrame);
    int currentSample = 0;
    int inIndex = 0;
    var outIndex = 0;

    for (int f = 0; f < frameCount; f++) {
      byte predictorScale = adpcm[inIndex++];
      int scale = (1 << GcAdpcmMath.GetLowNibble(predictorScale)) * 2048;
      int predictor = GcAdpcmMath.GetHighNibble(predictorScale);
      short coef1 = coefficients[predictor * 2];
      short coef2 = coefficients[predictor * 2 + 1];

      int samplesToRead
          = Math.Min(GcAdpcmMath.SamplesPerFrame, sampleCount - currentSample);

      for (int s = 0; s < samplesToRead; s++) {
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

        pcm[outIndex++] = clampedSample;
        currentSample++;
      }
    }

    return pcm;
  }
}