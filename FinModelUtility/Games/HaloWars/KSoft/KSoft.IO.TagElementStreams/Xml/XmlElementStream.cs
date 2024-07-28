using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	public sealed partial class XmlElementStream : TagElementTextStream<XmlDocument, XmlElement>
	{
		/// <summary>XmlNodes which we support explicit streaming on</summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[Contracts.Pure]
		public static bool StreamSourceIsValid(XmlNodeType type)
		{
			switch (type)
			{
				case XmlNodeType.Element:
				case XmlNodeType.Attribute:
				case XmlNodeType.Text: // aka, Cursor
					return true;

				default: return false;
			}
		}

		#region Cursor
		public override string CursorName { get { return this.Cursor?.Name; } }

		public override void InitializeAtRootElement()
		{
			this.Cursor = this.Document.DocumentElement;
		}
		#endregion

		#region Util
		public override bool AttributeExists(string name)
		{
			if (this.Cursor == null)
				return false;
			if (!this.ValidateNameArg(name))
				return false;

			XmlNode n = this.Cursor.Attributes[name];

			return n != null;
		}

		public override bool AttributesExist { get { return this.Cursor != null && this.Cursor.HasAttributes; } }

		public override IEnumerable<string> AttributeNames { get {
			if (this.AttributesExist)
				foreach (XmlAttribute attr in this.Cursor.Attributes)
					yield return attr.Name;
		} }

		[SuppressMessage("Microsoft.Design", "CA1820:TestForEmptyStringsUsingStringLength")]
		public override bool ElementsExists(string name)
		{
			if (this.Cursor == null)
				return false;
			if (!this.ValidateNameArg(name))
				return false;

			XmlElement n = this.Cursor[name];

			return n != null
				// #REVIEW CA1820: When I changed this to IsNotNullOrEmpty, it broke pretty much everything. Need to evaluate dropping the n.Value check altogether.
				// Eg, in KSoft.Blam.Engine.EngineRegistry.SerializePrototypes, when trying to read "Engines", the XmlElement is not null but the Value is very much null (only contains other elements)
				&& n.Value != string.Empty;
		}

		public override bool ElementsExist { get { return this.Cursor != null && this.Cursor.HasChildNodes; } }

		public override IEnumerable<XmlElement> Elements { get {
			if (this.ElementsExist)
				foreach (XmlNode n in this.Cursor)
					if (n is XmlElement)
						yield return (XmlElement)n;
		} }

		public override IEnumerable<XmlElement> ElementsByName(string localName)
		{
			if (this.ElementsExist)
				foreach (XmlNode n in this.Cursor.ChildNodes)
					if (n is XmlElement && n.Name == localName)
						yield return (XmlElement)n;

#if false // this returns ALL descendants, no just immediate children
			var elements = Cursor.GetElementsByTagName(localName);

			foreach(XmlNode n in elements)
				if(n is XmlElement) yield return (XmlElement)n;
#endif
		}

		public override string GetElementName(XmlElement element)
		{
			if (element == null)
				return null;

			return element.Name;
		}

		/// <see cref="XmlElement.ChildNodes.Count"/>
		protected override int PredictElementCount(XmlElement cursor) => cursor.ChildNodes.Count;
		#endregion

		#region Constructor
		XmlElementStream()
		{
			this.CommentsEnabled = true;
		}

		/// <summary>Initialize an element stream from a stream with <see cref="owner"/> as the initial owner object</summary>
		/// <param name="sourceStream">Stream we're to load the XML from</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		[SuppressMessage("Microsoft.Design", "CA3075:InsecureDTDProcessing")]
		public XmlElementStream(System.IO.Stream sourceStream,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null, string streamNameOverride = null)
		{
			Contract.Requires<ArgumentNullException>(sourceStream != null);
			Contract.Requires<ArgumentException>(sourceStream.HasPermissions(permissions));

			if (streamNameOverride.IsNullOrEmpty())
				this.SetStreamName(sourceStream);
			else
				this.StreamName = streamNameOverride;

			var doc = new Xml.XmlDocumentWithLocation
			{
				FileName = this.StreamName
			};
			this.Document = doc;
			try
			{
				using (var xmlReader = XmlReader.Create(sourceStream))
				{
					this.Document.Load(xmlReader);
				}
			} catch (Exception ex)
			{
				throw new System.IO.InvalidDataException("Failed to load " + this.StreamName, ex);
			}

			this.StreamMode = this.StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>Initialize an element stream from the XML file <paramref name="filename"/> with <see cref="owner"/> as the initial owner object</summary>
		/// <param name="filename">Name of the XML file we're to load</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		public XmlElementStream(string filename,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(filename != null);

			if (!System.IO.File.Exists(filename))
				throw new System.IO.FileNotFoundException("XmlElementStream: Load", filename);

			this.Document = new Xml.XmlDocumentWithLocation();
			try
			{
				var xml_reader_settings = new XmlReaderSettings
				{
					 XmlResolver = null,
					 DtdProcessing = DtdProcessing.Ignore
				};
				using (var xml_reader = XmlReader.Create(this.StreamName = filename, xml_reader_settings))
				{
					this.Document.Load(xml_reader);
				}
			}
			catch (Exception ex)
			{
				throw new System.IO.InvalidDataException("Failed to load " + this.StreamName, ex);
			}

			this.StreamMode = this.StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>
		/// Initialize an element stream from the XML nodes <paramref name="document"/>
		/// and <paramref name="cursor"/> with <paramref name="owner"/> as the initial owner object
		/// </summary>
		/// <param name="document"><paramref name="cursor"/>'s owner document</param>
		/// <param name="cursor">Starting element cursor</param>
		/// <param name="permissions">Supported access permissions for this stream</param>
		/// <param name="owner">Initial owner object</param>
		public XmlElementStream(XmlDocument document, XmlElement cursor,
			System.IO.FileAccess permissions = System.IO.FileAccess.ReadWrite, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(document != null);
			Contract.Requires(ReferenceEquals(cursor.OwnerDocument, document));

			this.Document = document;
			this.Cursor = cursor;

			this.StreamName = string.Format(Util.InvariantCultureInfo, "XmlDocument:{0}", document.Name);

			this.StreamMode = this.StreamPermissions = permissions;

			this.Owner = owner;
		}

		/// <summary>Initialize a new element stream with write permissions</summary>
		/// <param name="owner">Initial owner object</param>
		/// <param name="rootName">Name of the document element</param>
		/// <returns></returns>
		public static XmlElementStream CreateForWrite(string rootName, object owner = null)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(rootName));

			var root = new XmlDocument()
			{
				XmlResolver = null,
			};
			root.AppendChild(root.CreateElement(rootName));

			XmlElementStream @this = new XmlElementStream
			{
				Document = root,
				Owner = owner,
			};

			@this.StreamMode = @this.StreamPermissions = System.IO.FileAccess.Write;

			@this.InitializeAtRootElement();

			return @this;
		}
		#endregion

		public override bool SupportsComments { get { return true; } }
	};
}
