using System.Collections.Generic;
using System.Xml;

namespace KSoft.Phoenix.Xmb
{
	partial class XmbFile
	{
		sealed class Element
		{
			internal int Index;
			Values.PtrHandle mAttributesOffsetPos, mAttributesOffset;
			Values.PtrHandle mChildrenOffsetPos, mChildrenOffset;

			public int RootElementIndex = TypeExtensions.kNone;
			public XmbVariant NameVariant;
			public XmbVariant InnerTextVariant;
			List<KeyValuePair<XmbVariant, XmbVariant>> Attributes;
			List<int> ChildrenIndices;

			#region IEndianStreamable Members
			public void ReadAttributes(XmbFile xmb, IO.EndianReader s)
			{
				if (this.mAttributesOffset.IsInvalidHandle)
					return;

				s.Seek((long) this.mAttributesOffset);
				for (int x = 0; x < this.Attributes.Capacity; x++)
				{
					XmbVariant k; XmbVariantSerialization.Read(s, out k);
					XmbVariant v; XmbVariantSerialization.Read(s, out v);

					var kv = new KeyValuePair<XmbVariant, XmbVariant>(k, v);
					this.Attributes.Add(kv);

					if (k.HasUnicodeData || v.HasUnicodeData)
						xmb.mHasUnicodeStrings = true;
				}
			}
			public void ReadChildren(IO.EndianReader s)
			{
				if (this.mChildrenOffset.IsInvalidHandle)
					return;

				s.Seek((long) this.mChildrenOffset);
				for (int x = 0; x < this.ChildrenIndices.Capacity; x++)
					this.ChildrenIndices.Add(s.ReadInt32());
			}
			public void Read(XmbFile xmb, XmbFileContext xmbContext, IO.EndianReader s)
			{
				s.Read(out this.RootElementIndex);
				XmbVariantSerialization.Read(s, out this.NameVariant);
				XmbVariantSerialization.Read(s, out this.InnerTextVariant);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				#region Attributes header
				int count;
				s.Read(out count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out this.mAttributesOffset);
				this.Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(count);
				#endregion

				#region Children header
				s.Read(out count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}
				s.ReadVirtualAddress(out this.mChildrenOffset);
				this.ChildrenIndices = new List<int>(count);
				#endregion

				if (this.NameVariant.HasUnicodeData || this.InnerTextVariant.HasUnicodeData)
					xmb.mHasUnicodeStrings = true;
			}

			public void WriteAttributes(IO.EndianWriter s)
			{
				if (this.Attributes.Count == 0)
					return;

				this.mAttributesOffset = s.PositionPtr;
				foreach (var kv in this.Attributes)
				{
					XmbVariantSerialization.Write(s, kv.Key);
					XmbVariantSerialization.Write(s, kv.Value);
				}

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long) this.mAttributesOffsetPos);
				s.WriteVirtualAddress(this.mAttributesOffset);
				s.Seek(pos);
			}
			public void WriteChildren(IO.EndianWriter s)
			{
				if (this.ChildrenIndices.Count == 0)
					return;

				this.mChildrenOffset = s.PositionPtr;
				foreach (int ci in this.ChildrenIndices)
					s.Write(ci);

				// Update element entry
				var pos = s.BaseStream.Position;
				s.Seek((long) this.mChildrenOffsetPos);
				s.WriteVirtualAddress(this.mChildrenOffset);
				s.Seek(pos);
			}
			public void Write(IO.EndianWriter s)
			{
				var xmbContext = s.UserData as XmbFileContext;

				s.Write(this.RootElementIndex);
				XmbVariantSerialization.Write(s, this.NameVariant);
				XmbVariantSerialization.Write(s, this.InnerTextVariant);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				#region Attributes header
				s.Write(this.Attributes.Count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				this.mAttributesOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion

				#region Children header
				s.Write(this.ChildrenIndices.Count);
				if (xmbContext.PointerSize == Shell.ProcessorSize.x64)
				{
					s.Pad32();
				}

				this.mChildrenOffsetPos = s.PositionPtr;
				s.WriteVirtualAddress(Values.PtrHandle.InvalidHandle32);
				#endregion
			}
			#endregion

			#region FromXml
			public void FromXmlProcessChildren(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlProcessAttributes(XmbFileBuilder builder, XmlElement e)
			{
			}
			public void FromXmlInitialize(XmbFileBuilder builder, int rootIndex, int index, XmlElement e)
			{
				this.Index = index;
				this.RootElementIndex = rootIndex;

				if (e.HasAttributes)
					this.Attributes = new List<KeyValuePair<XmbVariant, XmbVariant>>(e.Attributes.Count);
				if (e.HasChildNodes)
					this.ChildrenIndices = new List<int>(e.ChildNodes.Count);

				string name = e.Name;
				string text = e.Value;

				if (e.HasAttributes)
					this.FromXmlProcessAttributes(builder, e);
				if (e.HasChildNodes)
					this.FromXmlProcessChildren(builder, e);
			}
			#endregion
			#region ToXml
			void InnerTextToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (!this.InnerTextVariant.IsEmpty)
				{
					var text = doc.CreateTextNode(xmb.ToString(this.InnerTextVariant));
					e.AppendChild(text);
				}
			}
			void AttributesToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (this.Attributes.Count > 0) foreach (var kv in this.Attributes)
				{
					string k = xmb.ToString(kv.Key);
					string v = xmb.ToString(kv.Value);

					var attr = doc.CreateAttribute(k);
					attr.Value = v;

					// #HACK avoids exceptions like:
					// "The prefix '' cannot be redefined from '' to 'http://www.w3.org/2000/09/xmldsig#' within the same start element tag."
					// for XML files that weren't meant for the game but were transformed to XMB anyway
					if (string.CompareOrdinal(k, "xmlns")==0)
					{
						var comment = doc.CreateComment(attr.OuterXml);
						e.AppendChild(comment);
						continue;
					}

					e.Attributes.Append(attr);
				}
			}
			void ChildrenToXml(XmbFile xmb, XmlDocument doc, XmlElement e)
			{
				if (this.ChildrenIndices.Count > 0) foreach (int x in this.ChildrenIndices)
				{
					var element = xmb.mElements[x];

					element.ToXml(xmb, doc, e);
				}
			}
			public XmlElement ToXml(XmbFile xmb, XmlDocument doc, XmlElement root)
			{
				var e = doc.CreateElement(xmb.ToString(this.NameVariant));

				if (root != null)
					root.AppendChild(e);

				this.AttributesToXml(xmb, doc, e);
				this.ChildrenToXml(xmb, doc, e);
				this.InnerTextToXml(xmb, doc, e);

				return e;
			}
			#endregion
		};
	};
}