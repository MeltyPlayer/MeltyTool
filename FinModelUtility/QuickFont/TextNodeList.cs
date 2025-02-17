// Decompiled with JetBrains decompiler
// Type: QuickFont.TextNodeList
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections;
using System.Text;

#nullable disable
namespace QuickFont
{
  internal class TextNodeList : IEnumerable
  {
    public TextNode Head;
    public TextNode Tail;

    public TextNodeList(string text)
    {
      text = text.Replace("\r\n", "\r");
      bool flag = false;
      StringBuilder stringBuilder = new StringBuilder();
      foreach (char ch in text)
      {
        switch (ch)
        {
          case '\n':
          case '\r':
          case ' ':
            if (flag)
            {
              this.Add(new TextNode(TextNodeType.Word, stringBuilder.ToString()));
              flag = false;
            }
            switch (ch)
            {
              case '\n':
              case '\r':
                this.Add(new TextNode(TextNodeType.LineBreak, (string) null));
                continue;
              case ' ':
                this.Add(new TextNode(TextNodeType.Space, (string) null));
                continue;
              default:
                continue;
            }
          default:
            if (!flag)
            {
              flag = true;
              stringBuilder = new StringBuilder();
            }
            stringBuilder.Append(ch);
            break;
        }
      }
      if (!flag)
        return;
      this.Add(new TextNode(TextNodeType.Word, stringBuilder.ToString()));
    }

    public void MeasureNodes(QFontData fontData, QFontRenderOptions options)
    {
      foreach (TextNode node in this)
      {
        if ((double) Math.Abs(node.Length) < 1.4012984643248171E-45)
          node.Length = this.MeasureTextNodeLength(node, fontData, options);
      }
    }

    private float MeasureTextNodeLength(
      TextNode node,
      QFontData fontData,
      QFontRenderOptions options)
    {
      bool flag = fontData.IsMonospacingActive(options);
      float monoSpaceWidth = fontData.GetMonoSpaceWidth(options);
      if (node.Type == TextNodeType.Space)
        return flag ? monoSpaceWidth : (float) Math.Ceiling((double) fontData.MeanGlyphWidth * (double) options.WordSpacing);
      float num = 0.0f;
      float val1 = 0.0f;
      if (node.Type == TextNodeType.Word)
      {
        for (int index = 0; index < node.Text.Length; ++index)
        {
          char key = node.Text[index];
          if (fontData.CharSetMapping.ContainsKey(key))
          {
            QFontGlyph qfontGlyph = fontData.CharSetMapping[key];
            if (flag)
              num += monoSpaceWidth;
            else
              num += (float) Math.Ceiling((double) fontData.CharSetMapping[key].Rect.Width + (double) fontData.MeanGlyphWidth * (double) options.CharacterSpacing + (double) fontData.GetKerningPairCorrection(index, node.Text, node));
            val1 = Math.Max(val1, (float) (qfontGlyph.YOffset + qfontGlyph.Rect.Height));
          }
        }
      }
      node.Height = val1;
      return num;
    }

    public void Crumble(TextNode node, int baseCaseSize)
    {
      if (node.Text.Length <= baseCaseSize)
        return;
      TextNode node1 = this.SplitNode(node);
      TextNode next = node1.Next;
      this.Crumble(node1, baseCaseSize);
      this.Crumble(next, baseCaseSize);
    }

    public TextNode SplitNode(TextNode node)
    {
      if (node.Type != TextNodeType.Word)
        throw new Exception("Cannot slit text node of type: " + node.Type.ToString());
      int num = node.Text.Length / 2;
      string text1 = node.Text.Substring(0, num);
      string text2 = node.Text.Substring(num, node.Text.Length - num);
      TextNode textNode1 = new TextNode(TextNodeType.Word, text1);
      TextNode textNode2 = new TextNode(TextNodeType.Word, text2);
      textNode1.Next = textNode2;
      textNode2.Previous = textNode1;
      if (node.Previous == null)
      {
        this.Head = textNode1;
      }
      else
      {
        node.Previous.Next = textNode1;
        textNode1.Previous = node.Previous;
      }
      if (node.Next == null)
      {
        this.Tail = textNode2;
      }
      else
      {
        node.Next.Previous = textNode2;
        textNode2.Next = node.Next;
      }
      return textNode1;
    }

    public void Add(TextNode node)
    {
      if (this.Head == null)
      {
        this.Head = node;
        this.Tail = node;
      }
      else
      {
        this.Tail.Next = node;
        node.Previous = this.Tail;
        this.Tail = node;
      }
    }

    public override string ToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (TextNode textNode in this)
      {
        if (textNode.Type == TextNodeType.Space)
          stringBuilder.Append(" ");
        if (textNode.Type == TextNodeType.LineBreak)
          stringBuilder.Append(Environment.NewLine);
        if (textNode.Type == TextNodeType.Word)
          stringBuilder.Append(textNode.Text ?? "");
      }
      return stringBuilder.ToString();
    }

    public IEnumerator GetEnumerator()
    {
      return (IEnumerator) new TextNodeList.TextNodeListEnumerator(this);
    }

    private sealed class TextNodeListEnumerator : IEnumerator
    {
      private TextNode _currentNode;
      private TextNodeList _targetList;

      public TextNodeListEnumerator(TextNodeList targetList) => this._targetList = targetList;

      public object Current => (object) this._currentNode;

      public bool MoveNext()
      {
        this._currentNode = this._currentNode != null ? this._currentNode.Next : this._targetList.Head;
        return this._currentNode != null;
      }

      public void Reset() => this._currentNode = (TextNode) null;
    }
  }
}
