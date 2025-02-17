// Decompiled with JetBrains decompiler
// Type: QuickFont.TextNode
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

#nullable disable
namespace QuickFont
{
  internal class TextNode
  {
    public TextNodeType Type;
    public string Text;
    public float Length;
    public float LengthTweak;
    public float Height;
    public TextNode Next;
    public TextNode Previous;

    public float ModifiedLength => this.Length + this.LengthTweak;

    public TextNode(TextNodeType type, string text)
    {
      this.Type = type;
      this.Text = text;
    }
  }
}
