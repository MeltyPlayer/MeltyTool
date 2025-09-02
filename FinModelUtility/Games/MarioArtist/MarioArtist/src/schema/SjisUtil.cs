using System.Text;

using schema.binary;

namespace marioartist.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/Util/SJISUtil.cs
/// </summary>
public static class SjisUtil {
  static Dictionary<ushort, char> leoMapping = new Dictionary<ushort, char>();
  static bool isMappingReady = false;

  static void PrepareMapping() {
    if (isMappingReady)
      return;

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    leoMapping = new Dictionary<ushort, char>();
    leoMapping.Add(0x86A3, '\u2660'); //Spade
    leoMapping.Add(0x86A4, '\u2663'); //Club
    leoMapping.Add(0x86A5, '\u2665'); //Heart
    leoMapping.Add(0x86A6, '\u2666'); //Diamond

    isMappingReady = true;
  }

  public static string ReadString(IBinaryReader br, int maxLength) {
    PrepareMapping();

    Span<byte> bytes = stackalloc byte[maxLength];
    br.ReadBytes(bytes);

    return Encoding
           .GetEncoding(932,
                        new EncoderReplacementFallback(),
                        new CustomDecoder())
           .GetString(bytes)
           .TrimEnd('\x00');
  }

  //Decoder
  public class CustomDecoder : DecoderFallback {
    public string DefaultString;
    internal Dictionary<ushort, char> mapping;

    public CustomDecoder() : this("*") { }

    public CustomDecoder(string defaultString) {
      this.DefaultString = defaultString;

      // Create table of mappings
      mapping = leoMapping;
    }

    public override DecoderFallbackBuffer CreateFallbackBuffer() {
      return new CustomDecoderFallbackBuffer(this);
    }

    public override int MaxCharCount {
      get { return 2; }
    }
  }

  public class CustomDecoderFallbackBuffer : DecoderFallbackBuffer {
    int count = -1; // Number of characters to return
    int index = -1; // Index of character to return
    CustomDecoder fb;
    string charsToReturn;

    public CustomDecoderFallbackBuffer(CustomDecoder fallback) {
      this.fb = fallback;
    }

    public override bool Fallback(byte[] bytesUnknown, int index) {
      // Return false if there are already characters to map.
      if (count >= 1) return false;

      // Determine number of characters to return.
      charsToReturn = String.Empty;

      if (bytesUnknown.Length == 2) {
        ushort key = (ushort) ((bytesUnknown[0] << 8) + bytesUnknown[1]);
        if (this.fb.mapping.TryGetValue(key, out char value)) {
          charsToReturn = Convert.ToString(value);
          count = 1;
        } else {
          // Return default.
          charsToReturn = fb.DefaultString;
          count = 1;
        }

        this.index = charsToReturn.Length - 1;
      } else {
        //Only full width
        charsToReturn = fb.DefaultString;
        count = 1;
      }

      return true;
    }

    public override char GetNextChar() {
      // We'll return a character if possible, so subtract from the count of chars to return.
      count--;
      // If count is less than zero, we've returned all characters.
      if (count < 0)
        return '\u0000';

      this.index--;
      return charsToReturn[this.index + 1];
    }

    public override bool MovePrevious() {
      // Original: if count >= -1 and pos >= 0
      if (count >= -1) {
        count++;
        return true;
      } else {
        return false;
      }
    }

    public override int Remaining {
      get { return count < 0 ? 0 : count; }
    }

    public override void Reset() {
      count = -1;
      index = -1;
    }
  }
}