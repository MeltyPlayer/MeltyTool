using System;

namespace KSoft.Phoenix.Engine
{
	public enum XmlFilePriority
	{
		None,

		Lists,
		GameData,
		ProtoData,

		kNumberOf
	};

	public enum XmlFileLoadState
	{
		NotLoaded,
		FileDoesNotExist,
		Failed,
		Preloading,
		Preloaded,
		Loading,
		Loaded,

		kNumberOf
	};

	public sealed class XmlFileInfo
		: IComparable<XmlFileInfo>
		, IEquatable<XmlFileInfo>
	{
		public static bool RespectWritableFlag { get { return true; } }

		public ContentStorage Location { get; set; }
		public GameDirectory Directory { get; set; }
		public string FileName { get; set; }
		public string RootName { get; set; }

		public bool Writable { get; set; }

		public int CompareTo(XmlFileInfo other)
		{
			if (this.Location != other.Location)
				return ((int) this.Location).CompareTo((int)other.Location);

			if (this.Directory != other.Directory)
				return ((int) this.Directory).CompareTo((int)other.Directory);

			return string.CompareOrdinal(this.FileName, other.FileName);
		}

		public bool Equals(XmlFileInfo other)
		{
			return this.Location == other.Location
				&&
				this.Directory == other.Directory
				&&
				this.FileName == other.FileName
				//&& RootName == other.RootName
				//&& Writable == other.Writable
				;
		}

		public override bool Equals(object obj)
		{
			return obj is XmlFileInfo && this.Equals((XmlFileInfo)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash *= 23 + this.Location.GetHashCode();
				hash *= 23 + this.Directory.GetHashCode();
				hash *= 23 + this.FileName.GetHashCode();
				return hash;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}.{1}.{2}",
			                     this.Location,
			                     this.Directory,
			                     this.FileName);
		}
	};

	public class XmlFileLoadStateChangedArgs
		: EventArgs
	{
		public XmlFileInfo XmlFile { get; private set; }
		public XmlFileLoadState NewState { get; private set; }

		public XmlFileLoadStateChangedArgs(XmlFileInfo xmlFile, XmlFileLoadState newState)
		{
			this.XmlFile = xmlFile;
			this.NewState = newState;
		}
	};

	[System.Diagnostics.DebuggerDisplay("{"+ nameof(DebuggerDisplay)  +"}")]
	public sealed class ProtoDataXmlFileInfo
	{
		public XmlFilePriority Priority;
		public XmlFileInfo FileInfo;
		public XmlFileInfo FileInfoWithUpdates;

		public ProtoDataXmlFileInfo(XmlFilePriority priority
			, XmlFileInfo fileInfo
			, XmlFileInfo fileInfoWithUpdates = null)
		{
			this.Priority = priority;
			this.FileInfo = fileInfo;
			this.FileInfoWithUpdates = fileInfoWithUpdates;
		}

		public string DebuggerDisplay { get {
			return string.Format("{0} {1}",
			                     this.Priority,
			                     this.FileInfo);
		} }
	};
}