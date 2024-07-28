using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.XML
{
	internal abstract class BListXmlSerializerBase<T>
		: IDisposable
		, IO.ITagElementStringNameStreamable
	{
		public abstract BListXmlParams Params { get; }
		public abstract Collections.BListBase<T> List { get; }

		#region IXmlElementStreamable Members
		protected abstract void Read<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, int iteration)
			where TDoc : class
			where TCursor : class;
		protected abstract void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs, T data)
			where TDoc : class
			where TCursor : class;

		protected virtual void ReadDetermineListSize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			int child_element_count = s.TryGetCursorElementCount();
			if (this.List.Capacity < child_element_count)
				this.List.Capacity = child_element_count;
		}
		protected virtual IEnumerable<TCursor> ReadGetNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			return this.Params.UseElementName
				? s.ElementsByName(this.Params.ElementName)
				: s.Elements;
		}
		protected virtual void ReadNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			this.ReadDetermineListSize(s, xs);

			int x = 0;
			foreach (var n in this.ReadGetNodes(s))
			{
				using (s.EnterCursorBookmark(n))
					this.Read(s, xs, x++);
			}

			this.List.OptimizeStorage();
		}
		protected virtual string WriteGetElementName(T data)
		{
			return this.Params.ElementName;
		}
		protected virtual void WriteNodes<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs)
			where TDoc : class
			where TCursor : class
		{
			foreach (T data in this.List)
				using (s.EnterCursorBookmark(this.WriteGetElementName(data)))
					this.Write(s, xs, data);
		}

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			bool should_stream = true;
			string root_name = this.Params.GetOptionalRootName();
			var xs = s.GetSerializerInterface();

			if (s.IsReading) // If the stream doesn't have the expected element, don't try to stream
				should_stream = root_name == null || s.ElementsExists(root_name);
			else if (s.IsWriting)
				should_stream = this.List != null && this.List.IsEmpty == false;

			if (should_stream) using (s.EnterCursorBookmark(root_name))
			{
					 if (s.IsReading)
						 this.ReadNodes(s, xs);
				else if (s.IsWriting)
					this.WriteNodes(s, xs);
			}
		}
		#endregion

		#region IDisposable Members
		public virtual void Dispose()
		{
		}
		#endregion
	};
}