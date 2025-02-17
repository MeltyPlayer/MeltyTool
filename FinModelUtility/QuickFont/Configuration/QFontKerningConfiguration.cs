// Decompiled with JetBrains decompiler
// Type: QuickFont.Configuration.QFontKerningConfiguration
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System.Collections.Generic;

#nullable disable
namespace QuickFont.Configuration
{
  public class QFontKerningConfiguration
  {
    private readonly Dictionary<char, CharacterKerningRule> _characterKerningRules = new Dictionary<char, CharacterKerningRule>();
    public byte AlphaEmptyPixelTolerance;

    public void BatchSetCharacterKerningRule(string chars, CharacterKerningRule rule)
    {
      foreach (char key in chars)
        this._characterKerningRules[key] = rule;
    }

    public void SetCharacterKerningRule(char c, CharacterKerningRule rule)
    {
      this._characterKerningRules[c] = rule;
    }

    public CharacterKerningRule GetCharacterKerningRule(char c)
    {
      return this._characterKerningRules.ContainsKey(c) ? this._characterKerningRules[c] : CharacterKerningRule.Normal;
    }

    public CharacterKerningRule GetOverridingCharacterKerningRuleForPair(string str)
    {
      if (str.Length < 2)
        return CharacterKerningRule.Normal;
      char c1 = str[0];
      char c2 = str[1];
      if (this.GetCharacterKerningRule(c1) == CharacterKerningRule.Zero || this.GetCharacterKerningRule(c2) == CharacterKerningRule.Zero)
        return CharacterKerningRule.Zero;
      return this.GetCharacterKerningRule(c1) == CharacterKerningRule.NotMoreThanHalf || this.GetCharacterKerningRule(c2) == CharacterKerningRule.NotMoreThanHalf ? CharacterKerningRule.NotMoreThanHalf : CharacterKerningRule.Normal;
    }

    public QFontKerningConfiguration()
    {
      this.BatchSetCharacterKerningRule("_^", CharacterKerningRule.Zero);
      this.SetCharacterKerningRule('"', CharacterKerningRule.NotMoreThanHalf);
      this.SetCharacterKerningRule('\'', CharacterKerningRule.NotMoreThanHalf);
    }
  }
}
