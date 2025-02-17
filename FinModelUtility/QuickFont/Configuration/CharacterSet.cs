// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.CharacterSet
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;

#nullable disable
namespace QuickFont.Configuration
{
  [Flags]
  public enum CharacterSet
  {
    BasicSet = 0,
    FrenchQuotes = 1,
    SpanishQuestEx = 2,
    CyrillicSet = 4,
    ExtendedLatin = 8,
    GreekAlphabet = 16, // 0x00000010
    TurkishI = 32, // 0x00000020
    HebrewAlphabet = 64, // 0x00000040
    ArabicAlphabet = 128, // 0x00000080
    ThaiKhmerAlphabet = 256, // 0x00000100
    Hiragana = 512, // 0x00000200
    JapDigits = 1024, // 0x00000400
    AsianQuotes = 2048, // 0x00000800
    EssentialKanji = 4096, // 0x00001000
    Katakana = 8192, // 0x00002000
    General = ExtendedLatin | SpanishQuestEx | FrenchQuotes, // 0x0000000B
    Cyrillic = CyrillicSet | FrenchQuotes, // 0x00000005
    Greek = GreekAlphabet | FrenchQuotes, // 0x00000011
    Turkish = TurkishI | ExtendedLatin, // 0x00000028
    Hebrew = HebrewAlphabet, // 0x00000040
    Arabic = ArabicAlphabet | FrenchQuotes, // 0x00000081
    Japanese = Katakana | EssentialKanji | AsianQuotes | JapDigits | Hiragana, // 0x00003E00
    Thai = ThaiKhmerAlphabet | FrenchQuotes, // 0x00000101
  }
}
