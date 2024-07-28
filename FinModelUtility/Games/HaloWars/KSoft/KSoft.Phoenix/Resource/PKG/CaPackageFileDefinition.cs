using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public sealed class CaPackageFileDefinition
		: IO.ITagElementStringNameStreamable
	{
		public const string kFileExtension = ".pkgdef";

		/// <summary>This should be the source file's name or a user defined name</summary>
		public string PkgName { get; private set; }

		public long Alignment;

		public List<string> FileNames { get; private set; }
			= new List<string>();

		#region ITagElementStringNameStreamable
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterUserDataBookmark(this))
			{
				s.StreamAttributeOpt("name", this, obj => this.PkgName, Predicates.IsNotNullOrEmpty);
				s.StreamAttributeOpt("alignment", this, obj => this.Alignment, Predicates.IsNotZero);

				using (var bm = s.EnterCursorBookmarkOpt("Files", this.FileNames, Predicates.HasItems))
					s.StreamElements("File", this.FileNames);
			}
		}
		#endregion

		public static string SanitizeWorkingEnvironmentPath(string workPath)
		{
			string result = workPath;

			result = Util.ReplaceAltDirectorySeparatorWithNormalChar(result);
			result = Util.AppendDirectorySeparatorChar(result);
			result = result.ToLowerIfContainsUppercase();

			return result;
		}

		public bool RedefineForWorkingEnvironment(string workPath
			, bool alwaysUseXmlOverXmb = false
			, TextWriter verboseOutput = null)
		{
			bool made_changes = false;

			this.FileNames.Sort(string.CompareOrdinal);

			for (int x = 0; x < this.FileNames.Count; x++)
			{
				string filename = this.FileNames[x];
				if (alwaysUseXmlOverXmb &&
					TryToReferenceXmlOverXmbFile(workPath, ref filename, verboseOutput))
				{

				}

				string filepath = Path.Combine(workPath, filename);
				filepath = filepath.ToLowerIfContainsUppercase();

				if (!File.Exists(filepath))
				{
					if (verboseOutput != null)
						verboseOutput.WriteLine("\tRemoving entry '{0}': Source file does not exist: {1}",
							filename, filepath);
					// remove and decrement x, to account for for loop increment
					this.FileNames.RemoveAt(x--);
					continue;
				}

				// I don't care if this is a change, it is how the engine expects file names
				filename = Util.PrependDirectorySeparatorChar(filename);
			}

			return made_changes;
		}

		private static bool TryToReferenceXmlOverXmbFile(string workPath
			, ref string fileName
			, TextWriter verboseOutput)
		{
			if (!ResourceUtils.IsXmbFile(fileName))
				return false;

			string xml_name = fileName;
			ResourceUtils.RemoveXmbExtension(ref xml_name);

			// does the XML file exist?
			string xml_path = Path.Combine(workPath, xml_name);
			if (!File.Exists(xml_path))
				return false;

			if (verboseOutput != null)
				verboseOutput.WriteLine("\tReplacing XMB ref with {0}",
					xml_name);

			// #TODO

			return true;
		}
	};
}

