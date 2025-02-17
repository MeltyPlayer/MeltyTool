// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontBuilderConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace QuickFont.Configuration
{
  public class QFontBuilderConfiguration : QFontConfiguration
  {
    private const string BASIC_SET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&€°µ";
    private const string FRENCH_QUOTES = "«»‹›";
    private const string SPANISH_QEST_EX = "¡¿";
    private const string CYRILLIC_SET = "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяљњќћџЉЊЌЋЏ";
    private const string EXTENDED_LATIN = "ÀŠŽŸžÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ";
    private const string GREEK_ALPHABET = "ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ";
    private const string TURKISH_I = "ıİŞ";
    private const string HEBREW_ALPHABET = "אבגדהוזחטיכךלמםנןסעפףצץקרשת";
    private const string ARABIC_ALPHABET = "ںکگپچژڈ¯؛ہءآأؤإئابةتثجحخدذرزسشصض×طظعغـفقكàلâمنهوçèéêëىيîï؟";
    private const string THAI_KHMER_ALPHABET = "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำิีึืฺุู฿เแโใไๅๆ็่้๊๋์ํ๎๏๐๑๒๓๔๕๖๗๘๙๚๛";
    private const string HIRAGANA = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ\u3097\u3098゙゛゜ゝゞゟ";
    private const string JAP_DIGITS = "㆐㆑\u3192\u3193\u3194\u3195㆖㆗㆘㆙㆚㆛㆜㆝㆞㆟";
    private const string ASIAN_QUOTES = "「」";
    private const string ESSENTIAL_KANJI = "⽇⽉";
    private const string KATAKANA = "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ・ーヽヾヿ";
    public int SuperSampleLevels = 1;
    public int PageMaxTextureSize = 4096;
    public int GlyphMargin = 2;
    public string CharSet = QFontBuilderConfiguration.BuildCharacterSet(QFontBuilderConfiguration.FigureOutBestCharacterSet());
    private CharacterSet _characters;
    public TextGenerationRenderHint TextGenerationRenderHint = TextGenerationRenderHint.SizeDependent;

    public CharacterSet Characters
    {
      get => this._characters;
      set
      {
        this._characters = value;
        this.CharSet = QFontBuilderConfiguration.BuildCharacterSet(this._characters);
      }
    }

    public QFontBuilderConfiguration()
    {
    }

    public QFontBuilderConfiguration(bool addDropShadow, bool transformToOrthogProjection = false)
      : base(addDropShadow, transformToOrthogProjection)
    {
    }

    public QFontBuilderConfiguration(QFontConfiguration fontConfiguration)
    {
      this.ShadowConfig = fontConfiguration.ShadowConfig;
      this.KerningConfig = fontConfiguration.KerningConfig;
      this.TransformToCurrentOrthogProjection = fontConfiguration.TransformToCurrentOrthogProjection;
    }

    private static string BuildCharacterSet(CharacterSet set)
    {
      Array values = Enum.GetValues(typeof (CharacterSet));
      string str = "";
      foreach (CharacterSet flag in values)
      {
        if (set.HasFlag((Enum) flag))
        {
          switch (flag)
          {
            case CharacterSet.BasicSet:
              str += "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,;'\"(!?)+-*/=_{}[]@~#\\<>|^%$£&€°µ";
              continue;
            case CharacterSet.FrenchQuotes:
              str += "«»‹›";
              continue;
            case CharacterSet.SpanishQuestEx:
              str += "¡¿";
              continue;
            case CharacterSet.CyrillicSet:
              str += "АБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзийклмнопрстуфхцчшщъыьэюяљњќћџЉЊЌЋЏ";
              continue;
            case CharacterSet.ExtendedLatin:
              str += "ÀŠŽŸžÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ";
              continue;
            case CharacterSet.GreekAlphabet:
              str += "ΈΉΊΌΎΏΐΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩΪΫάέήίΰαβγδεζηθικλμνξοπρςστυφχψωϊϋόύώ";
              continue;
            case CharacterSet.TurkishI:
              str += "ıİŞ";
              continue;
            case CharacterSet.HebrewAlphabet:
              str += "אבגדהוזחטיכךלמםנןסעפףצץקרשת";
              continue;
            case CharacterSet.ArabicAlphabet:
              str += "ںکگپچژڈ¯؛ہءآأؤإئابةتثجحخدذرزسشصض×طظعغـفقكàلâمنهوçèéêëىيîï؟";
              continue;
            case CharacterSet.ThaiKhmerAlphabet:
              str += "กขฃคฅฆงจฉชซฌญฎฏฐฑฒณดตถทธนบปผฝพฟภมยรฤลฦวศษสหฬอฮฯะัาำิีึืฺุู฿เแโใไๅๆ็่้๊๋์ํ๎๏๐๑๒๓๔๕๖๗๘๙๚๛";
              continue;
            case CharacterSet.Hiragana:
              str += "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖ\u3097\u3098゙゛゜ゝゞゟ";
              continue;
            case CharacterSet.JapDigits:
              str += "㆐㆑\u3192\u3193\u3194\u3195㆖㆗㆘㆙㆚㆛㆜㆝㆞㆟";
              continue;
            case CharacterSet.AsianQuotes:
              str += "「」";
              continue;
            case CharacterSet.EssentialKanji:
              str += "⽇⽉";
              continue;
            case CharacterSet.Katakana:
              str += "゠ァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ・ーヽヾヿ";
              continue;
            default:
              continue;
          }
        }
      }
      HashSet<char> source = new HashSet<char>();
      foreach (char ch in str)
        source.Add(ch);
      return new string(source.ToArray<char>());
    }

    private static CharacterSet FigureOutBestCharacterSet()
    {
      switch (CultureInfo.CurrentCulture.TextInfo.ANSICodePage)
      {
        case 874:
          return CharacterSet.Thai;
        case 932:
          return CharacterSet.Japanese;
        case 1251:
          return CharacterSet.Cyrillic;
        case 1252:
        case 1257:
          return CharacterSet.General;
        case 1253:
          return CharacterSet.Greek;
        case 1254:
          return CharacterSet.Turkish;
        case 1255:
          return CharacterSet.HebrewAlphabet;
        case 1256:
          return CharacterSet.Arabic;
        default:
          return CharacterSet.BasicSet;
      }
    }
  }
}
