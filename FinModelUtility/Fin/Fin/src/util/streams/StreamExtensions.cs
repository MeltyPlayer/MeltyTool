using System.IO;
using System;

namespace fin.util.streams;

public static class StreamExtensions {
  public static void CopyFromMiddleToEnd(this Stream stream,
                                         uint srcOffset,
                                         int srcLength) {
    var bufferSize = (int) Math.Min(stream.Length - srcOffset, srcLength);
    Span<byte> buffer = stackalloc byte[bufferSize];

    var remainingLength = srcLength;
    while (remainingLength > 0) {
      var currentLength = Math.Min(remainingLength, bufferSize);
      var currentBuffer = buffer[..currentLength];
      remainingLength -= bufferSize;

      var tmp = stream.Position;
      stream.Position = srcOffset;
      stream.ReadExactly(currentBuffer);
      stream.Position = tmp;

      stream.Write(currentBuffer);
      srcOffset += (uint) currentLength;
    }
  }
}