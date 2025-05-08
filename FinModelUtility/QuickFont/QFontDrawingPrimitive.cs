// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontDrawingPrimitive
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

#nullable disable
namespace QuickFont
{
  [DebuggerDisplay("Text = {_DisplayTest_dbg}")]
  public class QFontDrawingPrimitive
  {
    public Matrix4 ModelViewMatrix = Matrix4.Identity;

    public QFontDrawingPrimitive(QFont font, QFontRenderOptions options)
    {
      this.Font = font;
      this.Options = options;
    }

    public QFontDrawingPrimitive(QFont font)
    {
      this.Font = font;
      this.Options = new QFontRenderOptions();
    }

    public Vector3 PrintOffset { get; set; }

    public float LineSpacing
    {
      get
      {
        return (float) Math.Ceiling((double) this.Font.FontData.MaxGlyphHeight * (double) this.Options.LineSpacing);
      }
    }

    public bool IsMonospacingActive => this.Font.FontData.IsMonospacingActive(this.Options);

    public float MonoSpaceWidth => this.Font.FontData.GetMonoSpaceWidth(this.Options);

    public QFont Font { get; }

    public QFontRenderOptions Options { get; private set; }

    public SizeF LastSize { get; private set; }

    internal IList<QVertex> CurrentVertexRepr { get; } = (IList<QVertex>) new List<QVertex>();

    internal IList<QVertex> ShadowVertexRepr { get; } = (IList<QVertex>) new List<QVertex>();

    private void RenderDropShadow(
      float x,
      float y,
      char c,
      QFontGlyph nonShadowGlyph,
      QFont shadowFont,
      ref Rectangle clippingRectangle)
    {
      if (shadowFont == null || !this.Options.DropShadowActive)
        return;
      float num1 = (float) ((double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.DropShadowOffset.X + (double) nonShadowGlyph.Rect.Width * 0.5);
      float num2 = (float) ((double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.DropShadowOffset.Y + (double) nonShadowGlyph.Rect.Height * 0.5) + (float) nonShadowGlyph.YOffset;
      this.RenderGlyph(x + num1, y + num2, c, shadowFont, this.ShadowVertexRepr, clippingRectangle);
    }

    private bool ScissorsTest(
      ref float x,
      ref float y,
      ref float width,
      ref float height,
      ref float u1,
      ref float v1,
      ref float u2,
      ref float v2,
      Rectangle clipRectangle)
    {
      float y1 = (float) clipRectangle.Y;
      if ((double) y > (double) y1 + (double) clipRectangle.Height)
      {
        float num1 = height;
        float num2 = Math.Abs(y - (y1 + (float) clipRectangle.Height));
        y = y1 + (float) clipRectangle.Height;
        height -= num2;
        if ((double) height <= 0.0)
          return true;
        float num3 = Math.Abs(num2 / num1);
        v1 += num3 * (v2 - v1);
      }
      if ((double) y - (double) height < (double) y1)
      {
        float num4 = height;
        float num5 = Math.Abs(y1 - (y - height));
        height -= num5;
        if ((double) height <= 0.0)
          return true;
        float num6 = Math.Abs(num5 / num4);
        v2 -= num6 * (v2 - v1);
      }
      if ((double) x < (double) clipRectangle.X)
      {
        float num7 = width;
        float num8 = (float) clipRectangle.X - x;
        x = (float) clipRectangle.X;
        width -= num8;
        if ((double) width <= 0.0)
          return true;
        float num9 = num8 / num7;
        u1 += num9 * (u2 - u1);
      }
      if ((double) x + (double) width > (double) (clipRectangle.X + clipRectangle.Width))
      {
        float num10 = width;
        float num11 = x + width - (float) (clipRectangle.X + clipRectangle.Width);
        width -= num11;
        if ((double) width <= 0.0)
          return true;
        float num12 = num11 / num10;
        u2 -= num12 * (u2 - u1);
      }
      return false;
    }

    internal void RenderGlyph(
      float x,
      float y,
      char c,
      QFont font,
      IList<QVertex> store,
      Rectangle clippingRectangle)
    {
      QFontGlyph nonShadowGlyph = font.FontData.CharSetMapping[c];
      if (font.FontData.IsDropShadow)
      {
        x -= (float) (int) ((double) nonShadowGlyph.Rect.Width * 0.5);
        y -= (float) (int) ((double) nonShadowGlyph.Rect.Height * 0.5 + (double) nonShadowGlyph.YOffset);
      }
      else
        this.RenderDropShadow(x, y, c, nonShadowGlyph, font.FontData.DropShadowFont, ref clippingRectangle);
      y = -y;
      TexturePage page = font.FontData.Pages[nonShadowGlyph.Page];
      float u1 = (float) nonShadowGlyph.Rect.X / (float) page.Width;
      float v1 = (float) nonShadowGlyph.Rect.Y / (float) page.Height;
      float u2 = (float) (nonShadowGlyph.Rect.X + nonShadowGlyph.Rect.Width) / (float) page.Width;
      float v2 = (float) (nonShadowGlyph.Rect.Y + nonShadowGlyph.Rect.Height) / (float) page.Height;
      float x1 = x + this.PrintOffset.X;
      float y1 = y - (float) nonShadowGlyph.YOffset + this.PrintOffset.Y;
      float width = (float) nonShadowGlyph.Rect.Width;
      float height = (float) nonShadowGlyph.Rect.Height;
      if (clippingRectangle != new Rectangle() && this.ScissorsTest(ref x1, ref y1, ref width, ref height, ref u1, ref v1, ref u2, ref v2, clippingRectangle))
        return;
      Vector2 vector2_1 = new Vector2(u1, v1);
      Vector2 vector2_2 = new Vector2(u1, v2);
      Vector2 vector2_3 = new Vector2(u2, v2);
      Vector2 vector2_4 = new Vector2(u2, v1);
      Vector3 vector3_1 = new Vector3(x1, y1, this.PrintOffset.Z);
      Vector3 vector3_2 = new Vector3(x1, y1 - height, this.PrintOffset.Z);
      Vector3 vector3_3 = new Vector3(x1 + width, y1 - height, this.PrintOffset.Z);
      Vector3 vector3_4 = new Vector3(x1 + width, y1, this.PrintOffset.Z);
      Vector4 vector4 = Helper.ToVector4(!font.FontData.IsDropShadow ? this.Options.Colour : this.Options.DropShadowColour);
      IList<QVertex> qvertexList1 = store;
      QVertex qvertex1 = new QVertex();
      qvertex1.Position = vector3_1;
      qvertex1.TextureCoord = vector2_1;
      qvertex1.VertexColor = vector4;
      QVertex qvertex2 = qvertex1;
      qvertexList1.Add(qvertex2);
      IList<QVertex> qvertexList2 = store;
      qvertex1 = new QVertex();
      qvertex1.Position = vector3_2;
      qvertex1.TextureCoord = vector2_2;
      qvertex1.VertexColor = vector4;
      QVertex qvertex3 = qvertex1;
      qvertexList2.Add(qvertex3);
      IList<QVertex> qvertexList3 = store;
      qvertex1 = new QVertex();
      qvertex1.Position = vector3_3;
      qvertex1.TextureCoord = vector2_3;
      qvertex1.VertexColor = vector4;
      QVertex qvertex4 = qvertex1;
      qvertexList3.Add(qvertex4);
      IList<QVertex> qvertexList4 = store;
      qvertex1 = new QVertex();
      qvertex1.Position = vector3_1;
      qvertex1.TextureCoord = vector2_1;
      qvertex1.VertexColor = vector4;
      QVertex qvertex5 = qvertex1;
      qvertexList4.Add(qvertex5);
      IList<QVertex> qvertexList5 = store;
      qvertex1 = new QVertex();
      qvertex1.Position = vector3_3;
      qvertex1.TextureCoord = vector2_3;
      qvertex1.VertexColor = vector4;
      QVertex qvertex6 = qvertex1;
      qvertexList5.Add(qvertex6);
      IList<QVertex> qvertexList6 = store;
      qvertex1 = new QVertex();
      qvertex1.Position = vector3_4;
      qvertex1.TextureCoord = vector2_4;
      qvertex1.VertexColor = vector4;
      QVertex qvertex7 = qvertex1;
      qvertexList6.Add(qvertex7);
    }

    private float MeasureNextlineLength(string text)
    {
      float num = 0.0f;
      for (int index = 0; index < text.Length; ++index)
      {
        char key = text[index];
        switch (key)
        {
          case '\n':
          case '\r':
            goto label_10;
          default:
            if (this.IsMonospacingActive)
            {
              num += this.MonoSpaceWidth;
              continue;
            }
            if (key == ' ')
            {
              num += (float) Math.Ceiling((double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.WordSpacing);
              continue;
            }
            if (this.Font.FontData.CharSetMapping.ContainsKey(key))
            {
              QFontGlyph qfontGlyph = this.Font.FontData.CharSetMapping[key];
              num += (float) Math.Ceiling((double) qfontGlyph.Rect.Width + (double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.CharacterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, text, (TextNode) null));
              continue;
            }
            continue;
        }
      }
label_10:
      return num;
    }

    private Vector2 TransformPositionToViewport(Vector2 input)
    {
      Viewport? transformToViewport = this.Options.TransformToViewport;
      if (!transformToViewport.HasValue)
        return input;
      Viewport? currentViewport = ViewportHelper.CurrentViewport;
      return new Vector2((float) (((double) input.X - (double) transformToViewport.Value.X) * ((double) currentViewport.GetValueOrDefault().Width / (double) transformToViewport.Value.Width)), (float) (((double) input.Y - (double) transformToViewport.Value.Y) * ((double) currentViewport.GetValueOrDefault().Height / (double) transformToViewport.Value.Height)));
    }

    private static float TransformWidthToViewport(float input, QFontRenderOptions options)
    {
      Viewport? transformToViewport = options.TransformToViewport;
      if (!transformToViewport.HasValue)
        return input;
      Viewport? currentViewport = ViewportHelper.CurrentViewport;
      return input * (currentViewport.GetValueOrDefault().Width / transformToViewport.Value.Width);
    }

    private SizeF TransformMeasureFromViewport(SizeF input)
    {
      Viewport? transformToViewport = this.Options.TransformToViewport;
      if (!transformToViewport.HasValue)
        return input;
      Viewport? currentViewport = ViewportHelper.CurrentViewport;
      return new SizeF(input.Width * (transformToViewport.Value.Width / currentViewport.GetValueOrDefault().Width), input.Height * (transformToViewport.Value.Height / currentViewport.GetValueOrDefault().Height));
    }

    private Vector2 LockToPixel(Vector2 input)
    {
      if (!this.Options.LockToPixel)
        return input;
      float lockToPixelRatio = this.Options.LockToPixelRatio;
      return new Vector2((float) ((1.0 - (double) lockToPixelRatio) * (double) input.X + (double) lockToPixelRatio * (double) (int) Math.Round((double) input.X)), (float) ((1.0 - (double) lockToPixelRatio) * (double) input.Y + (double) lockToPixelRatio * (double) (int) Math.Round((double) input.Y)));
    }

    private Vector3 TransformToViewport(Vector3 input)
    {
      return new Vector3(this.LockToPixel(this.TransformPositionToViewport(input.Xy)))
      {
        Z = input.Z
      };
    }

    public SizeF Print(
      string text,
      Vector3 position,
      QFontAlignment alignment,
      Rectangle clippingRectangle = default (Rectangle))
    {
      this.PrintOffset = this.TransformToViewport(position);
      return this.PrintOrMeasure(text, alignment, false, clippingRectangle);
    }

    public SizeF Print(
      string text,
      Vector3 position,
      QFontAlignment alignment,
      Color color,
      Rectangle clippingRectangle = default (Rectangle))
    {
      this.Options.Colour = color;
      this.PrintOffset = this.TransformToViewport(position);
      return this.PrintOrMeasure(text, alignment, false, clippingRectangle);
    }

    public SizeF Print(
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      Rectangle clippingRectangle = default (Rectangle))
    {
      return this.Print(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment), this.TransformToViewport(position), clippingRectangle);
    }

    public SizeF Print(
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      Color colour,
      Rectangle clippingRectangle = default (Rectangle))
    {
      return this.Print(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment), this.TransformToViewport(position), colour, clippingRectangle);
    }

    public SizeF Print(ProcessedText processedText, Vector3 position, Rectangle clippingRectangle = default (Rectangle))
    {
      this.PrintOffset = this.TransformToViewport(position);
      return this.PrintOrMeasure(processedText, false, clippingRectangle);
    }

    public SizeF Print(
      ProcessedText processedText,
      Vector3 position,
      Color colour,
      Rectangle clippingRectangle = default (Rectangle))
    {
      this.Options.Colour = colour;
      this.PrintOffset = this.TransformToViewport(position);
      return this.PrintOrMeasure(processedText, false, clippingRectangle);
    }

    public SizeF Measure(string text, QFontAlignment alignment = QFontAlignment.Left)
    {
      return this.TransformMeasureFromViewport(this.PrintOrMeasure(text, alignment, true));
    }

    public SizeF Measure(string text, float maxWidth, QFontAlignment alignment)
    {
      return this.Measure(text, new SizeF(maxWidth, -1f), alignment);
    }

    public SizeF Measure(string text, SizeF maxSize, QFontAlignment alignment)
    {
      return this.Measure(QFontDrawingPrimitive.ProcessText(this.Font, this.Options, text, maxSize, alignment));
    }

    public SizeF Measure(ProcessedText processedText)
    {
      return this.TransformMeasureFromViewport(this.PrintOrMeasure(processedText, true));
    }

    private SizeF PrintOrMeasure(
      string text,
      QFontAlignment alignment,
      bool measureOnly,
      Rectangle clippingRectangle = default (Rectangle))
    {
      float width = 0.0f;
      float num = 0.0f;
      float y = 0.0f;
      float val2_1 = float.MinValue;
      float val2_2 = float.MaxValue;
      text = text.Replace("\r\n", "\r");
      switch (alignment)
      {
        case QFontAlignment.Right:
          num -= this.MeasureNextlineLength(text);
          break;
        case QFontAlignment.Centre:
          num -= (float) (int) (0.5 * (double) this.MeasureNextlineLength(text));
          break;
      }
      float val1 = 0.0f;
      for (int index = 0; index < text.Length; ++index)
      {
        char ch = text[index];
        switch (ch)
        {
          case '\n':
          case '\r':
            y += this.LineSpacing;
            val1 = 0.0f;
            num = 0.0f;
            switch (alignment)
            {
              case QFontAlignment.Right:
                num -= this.MeasureNextlineLength(text[(index + 1)..]);
                continue;
              case QFontAlignment.Centre:
                num -= (float) (int) (0.5 * (double) this.MeasureNextlineLength(text[(index + 1)..]));
                continue;
              default:
                continue;
            }
          default:
            val2_2 = Math.Min(num, val2_2);
            if (ch != ' ' && this.Font.FontData.CharSetMapping.ContainsKey(ch) && !measureOnly)
              this.RenderGlyph(num, y, ch, this.Font, this.CurrentVertexRepr, clippingRectangle);
            if (this.IsMonospacingActive)
              num += this.MonoSpaceWidth;
            else if (ch == ' ')
              num += (float) Math.Ceiling((double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.WordSpacing);
            else if (this.Font.FontData.CharSetMapping.ContainsKey(ch))
            {
              QFontGlyph qfontGlyph = this.Font.FontData.CharSetMapping[ch];
              num += (float) Math.Ceiling((double) qfontGlyph.Rect.Width + (double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.CharacterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, text, (TextNode) null));
              val1 = Math.Max(val1, (float) (qfontGlyph.Rect.Height + qfontGlyph.YOffset));
            }
            val2_1 = Math.Max(num, val2_1);
            break;
        }
      }
      if ((double) Math.Abs(val2_2 - float.MaxValue) > 1.4012984643248171E-45)
        width = val2_1 - val2_2;
      this.LastSize = new SizeF(width, y + this.LineSpacing);
      return this.LastSize;
    }

    private SizeF PrintOrMeasure(
      ProcessedText processedText,
      bool measureOnly,
      Rectangle clippingRectangle = default (Rectangle))
    {
      float num1 = 0.0f;
      float val1 = 0.0f;
      float num2 = 0.0f;
      float y = 0.0f;
      float width = processedText.MaxSize.Width;
      QFontAlignment alignment = processedText.Alignment;
      TextNodeList textNodeList = processedText.TextNodeList;
      for (TextNode textNode = textNodeList.Head; textNode != null; textNode = textNode.Next)
        textNode.LengthTweak = 0.0f;
      switch (alignment)
      {
        case QFontAlignment.Right:
          num2 -= (float) Math.Ceiling((double) this.TextNodeLineLength(textNodeList.Head, width) - (double) width);
          break;
        case QFontAlignment.Centre:
          num2 -= (float) Math.Ceiling(0.5 * (double) this.TextNodeLineLength(textNodeList.Head, width));
          break;
        case QFontAlignment.Justify:
          this.JustifyLine(textNodeList.Head, width);
          break;
      }
      bool flag1 = false;
      float num3 = 0.0f;
      for (TextNode node = textNodeList.Head; node != null; node = node.Next)
      {
        bool flag2 = false;
        if (node.Type == TextNodeType.LineBreak)
          flag2 = true;
        else if (((!this.Options.WordWrap ? 0 : (this.SkipTrailingSpace(node, num3, width) ? 1 : 0)) & (flag1 ? 1 : 0)) != 0)
          flag2 = true;
        else if ((double) num3 + (double) node.ModifiedLength <= (double) width || !flag1)
        {
          flag1 = true;
          if (!measureOnly)
            this.RenderWord(num2 + num3, y, node, ref clippingRectangle);
          num3 += node.ModifiedLength;
          val1 = Math.Max(val1, node.Height);
          num1 = Math.Max(num3, num1);
        }
        else if (this.Options.WordWrap)
        {
          flag2 = true;
          if (node.Previous != null)
            node = node.Previous;
        }
        else
          continue;
        if (flag2)
        {
          if ((double) processedText.MaxSize.Height <= 0.0 || (double) y + (double) this.LineSpacing - 0.0 < (double) processedText.MaxSize.Height)
          {
            y += this.LineSpacing;
            num2 = 0.0f;
            num3 = 0.0f;
            flag1 = false;
            if (node.Next != null)
            {
              switch (alignment)
              {
                case QFontAlignment.Right:
                  num2 -= (float) Math.Ceiling((double) this.TextNodeLineLength(node.Next, width) - (double) width);
                  continue;
                case QFontAlignment.Centre:
                  num2 -= (float) Math.Ceiling(0.5 * (double) this.TextNodeLineLength(node.Next, width));
                  continue;
                case QFontAlignment.Justify:
                  this.JustifyLine(node.Next, width);
                  continue;
                default:
                  continue;
              }
            }
          }
          else
            break;
        }
      }
      this.LastSize = new SizeF(num1, (float) ((double) y + (double) this.LineSpacing - 0.0));
      return this.LastSize;
    }

    private void RenderWord(float x, float y, TextNode node, ref Rectangle clippingRectangle)
    {
      if (node.Type != TextNodeType.Word)
        return;
      int num1 = node.Text.Length - 1;
      if (this.CrumbledWord(node))
        ++num1;
      int num2 = 0;
      int num3 = 0;
      if (num1 != 0)
      {
        num2 = (int) node.LengthTweak / num1;
        num3 = (int) node.LengthTweak - num2 * num1;
      }
      for (int index = 0; index < node.Text.Length; ++index)
      {
        char ch = node.Text[index];
        if (this.Font.FontData.CharSetMapping.ContainsKey(ch))
        {
          QFontGlyph qfontGlyph = this.Font.FontData.CharSetMapping[ch];
          this.RenderGlyph(x, y, ch, this.Font, this.CurrentVertexRepr, clippingRectangle);
          if (this.IsMonospacingActive)
            x += this.MonoSpaceWidth;
          else
            x += (float) (int) Math.Ceiling((double) qfontGlyph.Rect.Width + (double) this.Font.FontData.MeanGlyphWidth * (double) this.Options.CharacterSpacing + (double) this.Font.FontData.GetKerningPairCorrection(index, node.Text, node));
          x += (float) num2;
          if (num3 > 0)
          {
            ++x;
            --num3;
          }
          else if (num3 < 0)
          {
            --x;
            ++num3;
          }
        }
      }
    }

    private float TextNodeLineLength(TextNode node, float maxLength)
    {
      if (node == null)
        return 0.0f;
      bool flag = false;
      float lengthSoFar;
      for (lengthSoFar = 0.0f; node != null && node.Type != TextNodeType.LineBreak && !(this.SkipTrailingSpace(node, lengthSoFar, maxLength) & flag) && ((double) lengthSoFar + (double) node.Length <= (double) maxLength || !flag); node = node.Next)
      {
        flag = true;
        lengthSoFar += node.Length;
      }
      return lengthSoFar;
    }

    private bool CrumbledWord(TextNode node)
    {
      return node.Type == TextNodeType.Word && node.Next != null && node.Next.Type == TextNodeType.Word;
    }

    private void JustifyLine(TextNode node, float targetLength)
    {
      bool flag1 = false;
      if (node == null)
        return;
      TextNode textNode1 = node;
      int num1 = 0;
      int num2 = 0;
      bool flag2 = false;
      float lengthSoFar = 0.0f;
      TextNode textNode2 = node;
      for (; node != null && node.Type != TextNodeType.LineBreak; node = node.Next)
      {
        if (this.SkipTrailingSpace(node, lengthSoFar, targetLength) & flag2)
        {
          flag1 = true;
          break;
        }
        if ((double) lengthSoFar + (double) node.Length < (double) targetLength || !flag2)
        {
          textNode2 = node;
          if (node.Type == TextNodeType.Space)
            ++num2;
          if (node.Type == TextNodeType.Word)
          {
            num1 += node.Text.Length - 1;
            if (this.CrumbledWord(node))
              ++num1;
          }
          flag2 = true;
          lengthSoFar += node.Length;
        }
        else
        {
          flag1 = true;
          break;
        }
      }
      float num3 = 0.0f;
      int num4 = 0;
      int num5 = 0;
      bool flag3 = false;
      TextNode textNode3 = (TextNode) null;
      for (node = textNode2.Next; node != null && node.Type != TextNodeType.LineBreak; node = node.Next)
      {
        if (node.Type == TextNodeType.Space)
        {
          num3 += node.Length;
          ++num4;
        }
        else if (node.Type == TextNodeType.Word)
        {
          textNode3 = node;
          flag3 = true;
          num3 += node.Length;
          num5 += node.Text.Length - 1;
          break;
        }
      }
      if (!flag1)
        return;
      bool flag4 = flag3 && ((double) num3 + (double) lengthSoFar - (double) targetLength) * (double) this.Options.JustifyContractionPenalty < (double) targetLength - (double) lengthSoFar && ((double) targetLength - ((double) lengthSoFar + (double) num3 + 1.0)) / (double) targetLength > -(double) this.Options.JustifyCapContract;
      if ((flag4 || (double) lengthSoFar >= (double) targetLength) && (!flag4 || (double) lengthSoFar + (double) num3 <= (double) targetLength))
        return;
      if (flag4)
      {
        lengthSoFar += num3 + 1f;
        num1 += num5;
        num2 += num4;
      }
      int num6 = (int) ((double) targetLength - (double) lengthSoFar);
      int num7 = 0;
      int num8 = 0;
      if (flag4)
      {
        if ((double) num6 / (double) targetLength < -(double) this.Options.JustifyCapContract)
          num6 = (int) (-(double) this.Options.JustifyCapContract * (double) targetLength);
      }
      else if ((double) num6 / (double) targetLength > (double) this.Options.JustifyCapExpand)
        num6 = (int) ((double) this.Options.JustifyCapExpand * (double) targetLength);
      if (num1 == 0)
        num7 = num6;
      else if (num2 == 0)
      {
        num8 = num6;
      }
      else
      {
        num8 = !flag4 ? (int) ((double) num6 * (double) this.Options.JustifyCharacterWeightForExpand * (double) num1 / (double) num2) : (int) ((double) num6 * (double) this.Options.JustifyCharacterWeightForContract * (double) num1 / (double) num2);
        if (!flag4 && num8 > num6 || flag4 && num8 < num6)
          num8 = num6;
        num7 = num6 - num8;
      }
      int num9 = 0;
      int num10 = 0;
      if (num1 != 0)
      {
        num9 = num8 / num1;
        num10 = num8 - num9 * num1;
      }
      int num11 = 0;
      int num12 = 0;
      if (num2 != 0)
      {
        num11 = num7 / num2;
        num12 = num7 - num11 * num2;
      }
      for (node = textNode1; node != null; node = node.Next)
      {
        if (node.Type == TextNodeType.Space)
        {
          node.LengthTweak = (float) num11;
          if (num12 > 0)
          {
            ++node.LengthTweak;
            --num12;
          }
          else if (num12 < 0)
          {
            --node.LengthTweak;
            ++num12;
          }
        }
        else if (node.Type == TextNodeType.Word)
        {
          int num13 = node.Text.Length - 1;
          if (this.CrumbledWord(node))
            ++num13;
          node.LengthTweak = (float) (num13 * num9);
          if (num10 >= num13)
          {
            node.LengthTweak += (float) num13;
            num10 -= num13;
          }
          else if (num10 <= -num13)
          {
            node.LengthTweak -= (float) num13;
            num10 += num13;
          }
          else
          {
            node.LengthTweak += (float) num10;
            num10 = 0;
          }
        }
        if (!flag4 && node == textNode2 || flag4 && node == textNode3)
          break;
      }
    }

    private bool SkipTrailingSpace(TextNode node, float lengthSoFar, float boundWidth)
    {
      return node.Type == TextNodeType.Space && node.Next != null && node.Next.Type == TextNodeType.Word && (double) node.ModifiedLength + (double) node.Next.ModifiedLength + (double) lengthSoFar > (double) boundWidth;
    }

    public static ProcessedText ProcessText(
      QFont font,
      QFontRenderOptions options,
      string text,
      SizeF maxSize,
      QFontAlignment alignment)
    {
      maxSize.Width = QFontDrawingPrimitive.TransformWidthToViewport(maxSize.Width, options);
      TextNodeList textNodeList1 = new TextNodeList(text);
      textNodeList1.MeasureNodes(font.FontData, options);
      List<TextNode> textNodeList2 = new List<TextNode>();
      foreach (TextNode textNode in textNodeList1)
      {
        if ((!options.WordWrap || (double) textNode.Length >= (double) maxSize.Width) && textNode.Type == TextNodeType.Word)
          textNodeList2.Add(textNode);
      }
      foreach (TextNode node in textNodeList2)
        textNodeList1.Crumble(node, 1);
      textNodeList1.MeasureNodes(font.FontData, options);
      return new ProcessedText()
      {
        TextNodeList = textNodeList1,
        MaxSize = maxSize,
        Alignment = alignment
      };
    }
  }
}
