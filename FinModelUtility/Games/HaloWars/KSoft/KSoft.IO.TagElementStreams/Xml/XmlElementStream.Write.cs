using System.Xml;

namespace KSoft.IO
{
	partial class XmlElementStream
	{
		protected override void AppendElement(XmlElement e)
		{
			// if there is a node in scope, add the element after it and use it as the new scope
			if(this.Cursor != null)
				this.Cursor.AppendChild(e);
			else // if there is no XML node in scope, assume we're adding to the root
				this.Document.AppendChild(e);
		}

		protected override void NestElement(XmlElement e, out XmlElement oldCursor)
		{
			oldCursor = null;

			if (this.Cursor != null)
			{
				this.Cursor.AppendChild(e);

				oldCursor = this.Cursor;
				this.Cursor = e;
			}
			else // if there is no XML node in scope, assume we're adding to the root
			{
				this.Document.DocumentElement.AppendChild(e);
				this.Cursor = e;
			}
		}

		#region WriteElement impl
		protected override void WriteElement(XmlElement n, string value)
		{
			n.InnerText = value;

			//var text = m_root.CreateTextNode(value);
			//n.AppendChild(text);
		}
		#endregion

		#region WriteElement
		protected override XmlElement WriteElementAppend(string name)
		{
			this.ValidateWritePermission();

			XmlElement e = this.Document.CreateElement(name);
			this.AppendElement(e);

			return e;
		}

		protected override XmlElement WriteElementNest(string name, out XmlElement oldCursor)
		{
			this.ValidateWritePermission();

			XmlElement e = this.Document.CreateElement(name);
			this.NestElement(e, out oldCursor);

			return e;
		}
		#endregion

		#region WriteAttribute
		protected override void CursorWriteAttribute(string name, string value)
		{
			this.ValidateWritePermission();

			this.Cursor.SetAttribute(name, value);
		}
		#endregion

		protected override void WriteCommentImpl(string comment)
		{
			if (!string.IsNullOrEmpty(comment))
				this.Cursor.AppendChild(this.Document.CreateComment(comment));
		}
	};
}